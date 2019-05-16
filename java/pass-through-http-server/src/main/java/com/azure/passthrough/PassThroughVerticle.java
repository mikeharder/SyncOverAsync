package com.azure.passthrough;

import com.azure.passthrough.client.AsyncHttpClient;
import com.azure.passthrough.client.OkHttpClient;
import com.azure.passthrough.client.ReactorNettyClient;
import com.azure.passthrough.client.SyncHttpClient;
import io.vertx.core.AbstractVerticle;
import io.vertx.core.Future;
import io.vertx.core.Vertx;
import io.vertx.ext.web.Router;
import io.vertx.ext.web.RoutingContext;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class PassThroughVerticle extends AbstractVerticle {
    private static final String THREADS_HEADER = "Threads";
    private final Vertx vertx;
    private final ExecutorService pool = Executors.newCachedThreadPool();

    public PassThroughVerticle(Vertx vertx) {
        this.vertx = vertx;
    }

    @Override
    public void start(Future<Void> future) {
        Router router = Router.router(vertx);
        router.get("/").handler(this::run);

        vertx.createHttpServer()
            .requestHandler(router)
            .listen(8081, result -> {
                if (result.succeeded()) {
                    future.complete();
                } else {
                    future.fail(result.cause());
                }
            });
    }

    @Override
    public void stop() {
        pool.shutdown();
        System.exit(0);
    }

    private void run(RoutingContext routingContext) {
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
                AsyncHttpClient client = new ReactorNettyClient(uri);
                    client.sendAsync().doOnSuccess(s -> {
                        routingContext.response().setStatusCode(200).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(s);
                    }).doOnError(t -> {
                        routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(t.getMessage());
                    }).subscribe();
                break;
            }
            case SYNCOVERASYNC:
            case SYNC: {
                SyncHttpClient client;
                if (threadingModel == ThreadingModel.SYNC) {
                    client = new OkHttpClient(uri);
                } else {
                    client = new ReactorNettyClient(uri);
                }
                CompletableFuture.supplyAsync(() -> client.send(), pool)
                        .whenComplete((res, t) -> {
                            if (res != null) {
                                routingContext.response().setStatusCode(200).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(res);
                            } else {
                                routingContext.response().setStatusCode(500).putHeader(THREADS_HEADER, Integer.toString(Thread.activeCount())).end(t == null ? "" : t.getMessage());
                            }
                        });
                break;
            }
        }
    }
}
