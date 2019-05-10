package com.azure.syncoverasync;

import reactor.core.publisher.Mono;
import reactor.netty.http.client.HttpClient;

import java.nio.charset.StandardCharsets;

public class ReactorNettyClient {
    private HttpClient client;

    public ReactorNettyClient(String uri) {
        client = HttpClient.create().baseUrl(uri);
    }

    public Mono<String> sendAsync() {
        return client.get().responseContent()
                .aggregate()
                .map(bb -> bb.toString(StandardCharsets.UTF_8));
    }

    // not recommended
    public String send() {
        return sendAsync().block();
    }
}
