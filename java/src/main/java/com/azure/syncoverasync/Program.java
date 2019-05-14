package com.azure.syncoverasync;

import org.apache.commons.lang3.time.StopWatch;
import reactor.core.Disposable;
import reactor.core.publisher.Flux;
import reactor.core.scheduler.Schedulers;

import java.time.Duration;
import java.time.OffsetDateTime;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicLong;

public class Program {

    private static final StopWatch STOP_WATCH = new StopWatch();
    private static final AtomicBoolean KILLED = new AtomicBoolean(false);
    private static AtomicLong _requests = new AtomicLong(0);
    private static AtomicLong _responses = new AtomicLong(0);
    private static AtomicLong _responseLatencyTicks = new AtomicLong(0);
    private static int duration = 15; // sec

    private static ReactorNettyClient client;
    private static ExecutorService pool;
    private static Disposable reporterDisposable;

    public static void main(String[] args) throws Exception {
        String uri = null;
        int requestPerSec = 0;
        SyncOverAsyncOption syncOverAsync = SyncOverAsyncOption.NETTY_SYNC_OVER_ASYNC;

        assert (args.length >= 6);

        for (int i = 0; i != args.length; i+=2) {
            if (args[i].equalsIgnoreCase("-uri")) {
                uri = args[i+1];
            } else if (args[i].equalsIgnoreCase("-rps")) {
                requestPerSec = Integer.parseInt(args[i+1]);
            } else if (args[i].equalsIgnoreCase("-soa")) {
                syncOverAsync = SyncOverAsyncOption.valueOf(args[i+1]);
            } else if (args[i].equalsIgnoreCase("-duration")) {
                duration = Integer.parseInt(args[i+1]);
            }
        }

        System.out.println(String.format("Running config: uri: %s, request/sec: %d, syncOverAsync: %s, duration: %d sec", uri, requestPerSec, syncOverAsync, duration));

        WriteResults();

        client = new ReactorNettyClient(uri);

        if (syncOverAsync == SyncOverAsyncOption.NETTY_SYNC_OVER_ASYNC) {
            pool = Executors.newCachedThreadPool();
        }

//        if (syncOverAsync == SyncOverAsyncOption.NETTY_REACTOR) {
//            Mono.just(_requests)
//                    .repeatWhen(fl -> fl.)
//        }
        List<Disposable> disposables = new ArrayList<>();

        while (!KILLED.get()) {
            if (_requests.get() < (requestPerSec * STOP_WATCH.getTime(TimeUnit.MILLISECONDS) / 1000.0)) {
                _requests.incrementAndGet();

                long start = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);

                if (syncOverAsync == SyncOverAsyncOption.NETTY_ASYNC) {
                    disposables.add(client.sendAsync().doOnSuccess(s -> {
                        long end = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);
                        _responseLatencyTicks.addAndGet(end - start);
                        _responses.incrementAndGet();
                    }).subscribe());
                } else if (syncOverAsync == SyncOverAsyncOption.NETTY_SYNC_OVER_ASYNC) {
                    CompletableFuture.supplyAsync(() -> client.send(), pool)
                            .whenComplete((res, t) -> {
                                long end = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);

                                _responseLatencyTicks.addAndGet(end - start);
                                _responses.incrementAndGet();
                            });
                }
            } else {
                Thread.sleep(1);
            }
        }

        System.out.println("Exiting...");
        for (Disposable disposable : disposables) {
            if (!disposable.isDisposed()) {
                disposable.dispose();
            }
        }
        System.exit(0);
    }

    private static void WriteResults() {
        AtomicLong lastRequests = new AtomicLong((long) 0);
        AtomicLong lastResponses = new AtomicLong((long) 0);
        AtomicLong lastResponseLatencyTicks = new AtomicLong((long) 0);
        AtomicLong lastElapsedMs = new AtomicLong();

        STOP_WATCH.start();

        reporterDisposable = Flux.interval(Duration.ofSeconds(1))
                .take(duration)
                .doOnNext(s -> {

                    long requests = _requests.get();
                    long currentRequests = requests - lastRequests.get();
                    lastRequests.set(requests);

                    long responses = _responses.get();
                    long currentResponses = responses - lastResponses.get();
                    lastResponses.set(responses);

                    long responseLatencyTicks = _responseLatencyTicks.get();
                    long currentResponseLatencyTicks = responseLatencyTicks - lastResponseLatencyTicks.get();
                    lastResponseLatencyTicks.set(responseLatencyTicks);

                    long elapsed = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);
                    long currentElapsed = elapsed - lastElapsedMs.get();
                    lastElapsedMs.set(elapsed);

                    WriteResult(requests, responses, currentRequests, currentResponses, currentResponseLatencyTicks, currentElapsed);
                })
                .subscribeOn(Schedulers.newSingle("reporter"))
                .doOnComplete(() -> {
                    System.out.println("Reporter exiting...");
                    KILLED.set(true);
                    if (pool != null) {
                        pool.shutdown();
                    }
                })
                .subscribe();
    }

    private static void WriteResult(long totalRequests, long totalResponses,
                                    long currentRequests, long currentResponses, long currentResponseLatencyMs, long currentElapsedMs) {
        long threads = Thread.activeCount();

        StringBuilder builder = new StringBuilder();

        builder.append(OffsetDateTime.now().toString());
        builder.append("\tTot Req\t").append(totalRequests);
        builder.append("\tTot Rsp\t").append(totalResponses);
        builder.append("\tOut Req\t").append(totalRequests - totalResponses);
        builder.append("\tCur Q/S\t").append(currentRequests == 0 ? "N/A" : Math.round(currentRequests * 1000 / currentElapsedMs));
        builder.append("\tCur R/S\t").append(currentRequests == 0 ? "N/A" : Math.round(currentResponses * 1000 / currentElapsedMs));
        builder.append("\tCur Lat\t").append(currentRequests == 0 ? "N/A" : Math.round(currentResponseLatencyMs / currentRequests)).append("ms");
        builder.append("\tThreads\t").append(threads);

        System.out.println(builder.toString());
    }
}
