package com.azure.syncoverasync;

import reactor.core.publisher.Mono;
import reactor.netty.http.client.HttpClient;

public class ReactorNettyClient implements SyncHttpClient, AsyncHttpClient {
    private HttpClient client;

    public ReactorNettyClient(String uri) {
        client = HttpClient.create().baseUrl(uri)
                ;
//                .tcpConfiguration(tcpClient -> tcpClient.proxy(ts -> ts.type(Proxy.HTTP).address(new InetSocketAddress("localhost", 8888))));
    }

    public Mono<String> sendAsync() {
        return client.get().responseContent().aggregate().asString();
    }

    // not recommended
    public String send() {
        return sendAsync().block();
    }
}
