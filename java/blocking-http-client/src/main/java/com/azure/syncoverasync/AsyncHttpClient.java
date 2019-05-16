package com.azure.syncoverasync;

import reactor.core.publisher.Mono;

public interface AsyncHttpClient {
    Mono<String> sendAsync();
}
