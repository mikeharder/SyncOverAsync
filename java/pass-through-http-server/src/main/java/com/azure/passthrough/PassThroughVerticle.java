package com.azure.passthrough;

import com.azure.passthrough.client.OkHttpClient;
import com.azure.passthrough.client.ReactorNettyClient;
import com.azure.passthrough.client.SyncHttpClient;
import io.vertx.core.AbstractVerticle;
import io.vertx.core.Future;
import io.vertx.core.Vertx;
import io.vertx.core.http.HttpServer;
import io.vertx.ext.web.Router;
import io.vertx.ext.web.RoutingContext;
import reactor.core.publisher.Flux;
import reactor.core.scheduler.Schedulers;

import java.time.Duration;
import java.time.OffsetDateTime;
import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

public class PassThroughVerticle extends AbstractVerticle {
    private static final String THREADS_HEADER = "Threads";
    private final Vertx vertx;
    private ExecutorService pool = Executors.newCachedThreadPool();
    private ReactorNettyClient reactorNettyClient = new ReactorNettyClient();
    private OkHttpClient okHttpClient = new OkHttpClient();
    private HttpServer vertxServer;

    private AtomicLong clientConnections = new AtomicLong(0);
    private AtomicLong clientRequests = new AtomicLong(0);
    private AtomicLong serverSent = new AtomicLong(0);
    private AtomicLong serverResponses = new AtomicLong(0);
    private AtomicLong clientResponses = new AtomicLong(0);

    public PassThroughVerticle(Vertx vertx) {
        this.vertx = vertx;
    }

    @Override
    public void start(Future<Void> future) {
        Router router = Router.router(vertx);
        router.get("/").handler(this::run);
        router.get("/resetPool").handler(ctx -> {
            pool.shutdown();
            pool = Executors.newCachedThreadPool();
        });
        router.get("/resetPool/:size").handler(ctx -> {
            pool.shutdown();
            pool = Executors.newFixedThreadPool(Integer.parseInt(ctx.pathParam("size")));
        });


        vertxServer = vertx.createHttpServer()
            .connectionHandler(conn -> clientConnections.incrementAndGet())
            .requestHandler(router)
            .listen(8081, result -> {
                if (result.succeeded()) {
                    future.complete();
                } else {
                    future.fail(result.cause());
                }
            });

        printStatus();
        refreshStatus();
    }

    @Override
    public void stop() {
        pool.shutdown();
        vertxServer.close();
        System.exit(0);
    }

    private void printStatus() {
        Flux.interval(Duration.ofSeconds(1))
                .subscribeOn(Schedulers.newSingle("status"))
                .subscribe(l -> {
                    System.out.println(String.format("%s\tClient conn %d\tClient Req %d\tServer Req %d\tServer Res %d\tClient Res %d\tThread count %d",
                            OffsetDateTime.now(), clientConnections.get(), clientRequests.get(), serverSent.get(), serverResponses.get(), clientResponses.get(), Thread.activeCount()));
                });
    }

    private void refreshStatus() {
        AtomicLong lastReq = new AtomicLong(0);
        AtomicInteger secondsSinceLastReq = new AtomicInteger(0);
        Flux.interval(Duration.ofSeconds(1))
                .subscribeOn(Schedulers.newSingle("refresh"))
                .subscribe(l -> {
                    if (lastReq.get() == clientRequests.get()) {
                        secondsSinceLastReq.incrementAndGet();
                        if (secondsSinceLastReq.get() > 30) {
                            clientConnections.set(0);
                            clientRequests.set(0);
                            serverSent.set(0);
                            serverResponses.set(0);
                            clientResponses.set(0);
                        }
                    } else {
                        secondsSinceLastReq.set(0);
                        lastReq.set(clientRequests.get());
                    }
                });
    }

    private void run(RoutingContext routingContext) {
        clientRequests.incrementAndGet();
        String uri = routingContext.request().getParam("uri");
        ThreadingModel threadingModel = ThreadingModel.SYNCOVERASYNC;
        String threadingModelString = routingContext.request().getParam("threadingModel");
        if (threadingModelString != null) {
            try {
                threadingModel = ThreadingModel.parse(threadingModelString);
            } catch (Throwable t) {
                routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end("Invalid threading model \"" + threadingModelString +"\"");
                return;
            }
        }

        if (uri == null) {
            routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end("uri is null");
            return;
        }

        switch (threadingModel) {
            case ASYNC: {
                    reactorNettyClient.sendAsync(uri, serverSent).doOnSuccess(s -> {
                        serverResponses.incrementAndGet();
                        routingContext.response().setStatusCode(200).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(s);
                        clientResponses.incrementAndGet();
                    }).doOnError(t -> {
                        serverResponses.incrementAndGet();
                        routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(t.getMessage());
                        clientResponses.incrementAndGet();
                    }).subscribe();
                break;
            }
            case SYNCOVERASYNC:
            case SYNC: {
                SyncHttpClient client;
                if (threadingModel == ThreadingModel.SYNC) {
                    client = okHttpClient;
                } else {
                    client = reactorNettyClient;
                }
                CompletableFuture.supplyAsync(() -> client.send(uri, serverSent), pool)
                        .whenComplete((res, t) -> {
                            serverResponses.incrementAndGet();
                            if (res != null) {
                                routingContext.response().setStatusCode(200).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(res);
                            } else {
                                routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(t == null ? "" : t.getMessage());
                            }
                            clientResponses.incrementAndGet();
                        });
                break;
            }
        }
    }
}
