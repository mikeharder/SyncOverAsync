package com.azure.passthrough.client;

import reactor.core.publisher.Mono;

public interface AsyncHttpClient {
    Mono<String> sendAsync(String uri);
}
