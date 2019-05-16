package com.azure.passthrough;

import io.vertx.core.Vertx;

public class PassThroughHttpServer {
    public static void main(String[] args) {
        Vertx vertx = Vertx.vertx();
        vertx.deployVerticle(new PassThroughVerticle(vertx));
    }
}
