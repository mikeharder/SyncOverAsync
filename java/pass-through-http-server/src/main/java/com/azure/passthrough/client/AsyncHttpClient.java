package com.azure.passthrough.client;

import reactor.core.publisher.Mono;

import java.util.concurrent.atomic.AtomicLong;

public interface AsyncHttpClient {
    Mono<String> sendAsync(String uri, AtomicLong sent);
}
