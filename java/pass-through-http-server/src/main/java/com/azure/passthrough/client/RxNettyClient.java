package com.azure.passthrough.client;

import com.microsoft.rest.v2.RestProxy;
import com.microsoft.rest.v2.annotations.GET;
import com.microsoft.rest.v2.annotations.Host;
import com.microsoft.rest.v2.annotations.HostParam;
import com.microsoft.rest.v2.http.HttpClientConfiguration;
import com.microsoft.rest.v2.http.HttpPipeline;
import com.microsoft.rest.v2.http.HttpRequest;
import com.microsoft.rest.v2.http.HttpResponse;
import com.microsoft.rest.v2.http.NettyClient;
import com.microsoft.rest.v2.policy.DecodingPolicyFactory;
import com.microsoft.rest.v2.policy.RequestPolicy;
import com.microsoft.rest.v2.policy.RequestPolicyFactory;
import com.microsoft.rest.v2.policy.RequestPolicyOptions;
import com.microsoft.rest.v2.protocol.HttpResponseDecoder;
import io.reactivex.Flowable;
import io.reactivex.Single;
import reactor.core.publisher.Mono;

import java.net.InetSocketAddress;
import java.net.Proxy;
import java.net.Proxy.Type;
import java.util.concurrent.atomic.AtomicLong;

public class RxNettyClient implements SyncHttpClient, AsyncHttpClient {
    @Host("{url}")
    public interface RxNettyService {
        @GET("/")
        Single<String> get(@HostParam("url") String url);
    }

    private RxNettyService client;

    public RxNettyClient() {
        client = RestProxy.create(RxNettyService.class, HttpPipeline.build(new NettyClient.Factory().create(
                new HttpClientConfiguration(new Proxy(Type.HTTP, new InetSocketAddress("localhost", 8888)))),
                new DecodingPolicyFactory()))
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

    public static class DecodingPolicyFactory implements RequestPolicyFactory {
        public DecodingPolicyFactory() {
        }

        public RequestPolicy create(RequestPolicy next, RequestPolicyOptions options) {
            return new DecodingPolicy(next);
        }

        private final class DecodingPolicy implements RequestPolicy {
            private final RequestPolicy next;

            private DecodingPolicy(RequestPolicy next) {
                this.next = next;
            }

            public Single<HttpResponse> sendAsync(HttpRequest request) {
                return this.next.sendAsync(request).flatMap((response) -> {
                    return response.bodyAsString()
                            .map(s -> {
                                response.withDeserializedBody(s).withIsDecoded(true);
                                return response;
                            });
                });
            }
        }
    }
}
