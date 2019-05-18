package com.azure.passthrough.client;

import okhttp3.Request;

import java.io.IOException;
import java.util.concurrent.atomic.AtomicLong;

public class OkHttpClient implements SyncHttpClient {
    private okhttp3.OkHttpClient client;

    public OkHttpClient() {
        client = new okhttp3.OkHttpClient();
    }

    public String send(String uri, AtomicLong sent) {
        Request request = new Request.Builder()
                .get()
                .url(uri)
                .build();
        try {
            sent.incrementAndGet();
            return client.newCall(request).execute().body().string();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}
