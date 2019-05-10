package com.azure.syncoverasync;

import org.apache.commons.lang3.time.StopWatch;
import reactor.core.publisher.Mono;
import reactor.core.scheduler.Schedulers;

import java.time.OffsetDateTime;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.Executor;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicLong;

public class Program {

    private static final StopWatch STOP_WATCH = new StopWatch();

    private static AtomicLong _requests;
    private static AtomicLong _responses;
    private static AtomicLong _responseLatencyTicks;

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

        Executors.newSingleThreadExecutor().submit(() -> WriteResults());



        client = new ReactorNettyClient(uri);

//        CountDownLatch latch = new CountDownLatch(1);

//        ExecutorService pool = Executors.newCachedThreadPool();

        while (true)
        {
            if (_requests.get() < (requestPerSec * STOP_WATCH.getTime(TimeUnit.MILLISECONDS) / 1000.0))
            {
                Mono.defer(() -> Mono.just(client.send())).subscribeOn(Schedulers.elastic()).subscribe();
            }
            else
            {
                Thread.sleep(1);
            }
        }
    }

    private static void WriteResults()
    {
        long lastRequests = (long)0;
        long lastResponses = (long)0;
        long lastResponseLatencyTicks = (long)0;
        long lastElapsedMs = 0;

        STOP_WATCH.start();

        while (true)
        {
            try {
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }

            long requests = _requests.get();
            long currentRequests = requests - lastRequests;
            lastRequests = requests;

            long responses = _responses.get();
            long currentResponses = responses - lastResponses;
            lastResponses = responses;

            long responseLatencyTicks = _responseLatencyTicks.get();
            long currentResponseLatencyTicks = responseLatencyTicks - lastResponseLatencyTicks;
            lastResponseLatencyTicks = responseLatencyTicks;

            long elapsed = STOP_WATCH.getTime(TimeUnit.MILLISECONDS);
            long currentElapsed = elapsed - lastElapsedMs;
            lastElapsedMs = elapsed;

            WriteResult(requests, responses, currentRequests, currentResponses, currentResponseLatencyTicks, currentElapsed);
        }
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
        builder.append("\tCur Lat\t").append(Math.round(currentResponseLatencyMs / currentRequests)).append("ms");
        builder.append("\tThreads\t").append(threads);

        System.out.println(builder.toString());
    }
}
