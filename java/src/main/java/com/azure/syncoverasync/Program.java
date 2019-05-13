package com.azure.syncoverasync;

import org.apache.commons.lang3.time.StopWatch;
import reactor.core.publisher.Flux;
import reactor.core.scheduler.Schedulers;

import java.time.Duration;
import java.time.OffsetDateTime;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicLong;

public class Program {

    private static final StopWatch STOP_WATCH = new StopWatch();

    private static AtomicLong _requests = new AtomicLong(0);
    private static AtomicLong _responses = new AtomicLong(0);
    private static AtomicLong _responseLatencyTicks = new AtomicLong(0);

    private static ReactorNettyClient client;

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
            }
        }

        System.out.println(String.format("Running config: uri: %s, request/sec: %d, syncOverAsync: %s", uri, requestPerSec, syncOverAsync));

        WriteResults();

        client = new ReactorNettyClient(uri);

        ExecutorService pool = null;
        if (syncOverAsync == SyncOverAsyncOption.NETTY_SYNC_OVER_ASYNC) {
            pool = Executors.newCachedThreadPool();
        }

        while (!Thread.currentThread().isInterrupted())
        {
            if (_requests.get() < (requestPerSec * STOP_WATCH.getTime(TimeUnit.MILLISECONDS) / 1000.0))
            {
                _requests.incrementAndGet();

                long start = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);

                if (syncOverAsync == SyncOverAsyncOption.NETTY_ASYNC) {
                    client.sendAsync().doOnSuccess(s -> {
                        long end = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);

                        _responseLatencyTicks.addAndGet(end - start);
                        _responses.incrementAndGet();
                    }).subscribe();
                } else if (syncOverAsync == SyncOverAsyncOption.NETTY_SYNC_OVER_ASYNC) {
                    CompletableFuture.runAsync(() -> client.send(), pool)
                            .whenCompleteAsync((res, t) -> {
                                long end = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);

                                _responseLatencyTicks.addAndGet(end - start);
                                _responses.incrementAndGet();
                            });
                }
            }
            else
            {
                Thread.sleep(1);
            }
        }
    }

    private static void WriteResults()
    {
        AtomicLong lastRequests = new AtomicLong((long) 0);
        AtomicLong lastResponses = new AtomicLong((long) 0);
        AtomicLong lastResponseLatencyTicks = new AtomicLong((long) 0);
        AtomicLong lastElapsedMs = new AtomicLong();

        STOP_WATCH.start();

        Flux.interval(Duration.ofSeconds(1))
                .doOnNext(s -> {
                    try {
                        Thread.sleep(1000);
                    } catch (InterruptedException e) {
                        throw new RuntimeException(e);
                    }

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
        .subscribe();
    }

    private static void WriteResult(long totalRequests, long totalResponses,
                                    long currentRequests, long currentResponses, long currentResponseLatencyMs, long currentElapsedMs)
    {
        long threads = Thread.activeCount();

        StringBuilder builder = new StringBuilder();

        builder.append(OffsetDateTime.now().toString());
        builder.append("\tTot Req\t").append(totalRequests);
        builder.append("\tTot Rsp\t").append(totalResponses);
        builder.append("\tOut Req\t").append(totalRequests - totalResponses);
        builder.append("\tCur Q/S\t").append(Math.round(currentRequests * 1000 / currentElapsedMs));
        builder.append("\tCur R/S\t").append(Math.round(currentResponses * 1000 / currentElapsedMs));
        builder.append("\tCur Lat\t").append(currentRequests == 0 ? "N/A" : Math.round(currentResponseLatencyMs / currentRequests)).append("ms");
        builder.append("\tThreads\t").append(threads);

        System.out.println(builder.toString());
    }
}
