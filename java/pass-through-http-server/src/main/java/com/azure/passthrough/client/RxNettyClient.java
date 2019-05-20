package com.azure.passthrough.client;

import com.microsoft.rest.v2.SwaggerMethodParser;
import com.microsoft.rest.v2.http.HttpClient;
import com.microsoft.rest.v2.http.HttpMethod;
import com.microsoft.rest.v2.http.HttpRequest;
import com.microsoft.rest.v2.http.NettyClient;
import com.microsoft.rest.v2.protocol.HttpResponseDecoder;
import com.microsoft.rest.v2.serializer.JacksonAdapter;
import io.reactivex.Flowable;
import reactor.core.publisher.Mono;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.concurrent.atomic.AtomicLong;

public class RxNettyClient implements SyncHttpClient, AsyncHttpClient {
    private HttpClient client;

    public RxNettyClient() {
        client = NettyClient.createDefault();
                ;
//                .tcpConfiguration(tcpClient -> tcpClient.proxy(ts -> ts.type(Proxy.HTTP).address(new InetSocketAddress("localhost", 8888))));
    }

    public Mono<String> sendAsync(String uri, AtomicLong sent) {
        try {
            HttpRequest request = new HttpRequest(null, HttpMethod.GET, new URL(uri), new HttpResponseDecoder(null, new JacksonAdapter()));
            return Mono.from(client.sendRequestAsync(request).doOnSubscribe(d -> sent.incrementAndGet())
                    .flatMap(res -> res.bodyAsString())
                    .flatMapPublisher(Flowable::just));
        } catch (MalformedURLException e) {
            throw new RuntimeException(e);
        }
    }

    // not recommended
    public String send(String uri, AtomicLong sent) {
        return sendAsync(uri, sent).block();
    }
}
