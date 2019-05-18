package com.azure.passthrough;

import io.vertx.core.Vertx;
import io.vertx.core.VertxOptions;

public class PassThroughHttpServer {
    public static void main(String[] args) {
        Vertx vertx = Vertx.vertx(new VertxOptions().setWorkerPoolSize(100));
        vertx.deployVerticle(new PassThroughVerticle(vertx));
    }
}
