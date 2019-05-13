package com.azure.syncoverasync;

import reactor.core.publisher.Mono;
import reactor.netty.http.client.HttpClient;

public class ReactorNettyClient {
    private HttpClient client;

    public ReactorNettyClient(String uri) {
        client = HttpClient.create().baseUrl(uri);
    }

    public Mono<Void> sendAsync() {
        return client.get().response().ignoreElement().then();
    }

    // not recommended
    public void send() {
        sendAsync().block();
    }
}
