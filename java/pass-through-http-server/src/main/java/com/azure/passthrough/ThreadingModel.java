package com.azure.passthrough;

public enum ThreadingModel {
    SYNC,
    ASYNC,
    SYNCOVERASYNC,
    RXASYNC,
    RXSYNCOVERASYNC;

    public static ThreadingModel parse(String name) {
        return valueOf(name.toUpperCase());
    }
}
