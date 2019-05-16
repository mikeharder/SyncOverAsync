package com.azure.passthrough.client;

import okhttp3.Request;

import java.io.IOException;

public class OkHttpClient implements SyncHttpClient {
    private okhttp3.OkHttpClient client;
    private String uri;

    public OkHttpClient(String uri) {
        client = new okhttp3.OkHttpClient();
        this.uri = uri;
    }

    public String send() {
        Request request = new Request.Builder()
                .get()
                .url(uri)
                .build();
        try {
            return client.newCall(request).execute().body().string();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}
