package com.azure.passthrough.client;

import java.util.concurrent.atomic.AtomicLong;

public interface SyncHttpClient {
    String send(String uri, AtomicLong sent);
}
