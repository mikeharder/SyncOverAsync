package com.azure.passthrough;

public enum ThreadingModel {
    SYNC,
    ASYNC,
    SYNCOVERASYNC;

    public static ThreadingModel parse(String name) {
        return valueOf(name.toUpperCase());
    }
}
