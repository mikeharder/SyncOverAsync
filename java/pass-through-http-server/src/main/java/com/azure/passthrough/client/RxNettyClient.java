package com.azure.passthrough.client;

import com.microsoft.rest.v2.RestProxy;
import com.microsoft.rest.v2.annotations.GET;
import io.reactivex.Flowable;
import io.reactivex.Single;
import reactor.core.publisher.Mono;

import java.util.concurrent.atomic.AtomicLong;

public class RxNettyClient implements SyncHttpClient, AsyncHttpClient {
    public interface RxNettyService {
        @GET("{url}")
        Single<String> get(String url);
    }

    private RxNettyService client;

    public RxNettyClient() {
        client = RestProxy.create(RxNettyService.class)
                ;
//                .tcpConfiguration(tcpClient -> tcpClient.proxy(ts -> ts.type(Proxy.HTTP).address(new InetSocketAddress("localhost", 8888))));
    }

    public Mono<String> sendAsync(String uri, AtomicLong sent) {
        return Mono.from(client.get(uri).doOnSubscribe(d -> sent.incrementAndGet())
                .flatMapPublisher(Flowable::just));
    }

    // not recommended
    public String send(String uri, AtomicLong sent) {
        return sendAsync(uri, sent).block();
    }
}
