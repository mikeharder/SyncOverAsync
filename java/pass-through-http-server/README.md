# SyncOverAsync - Java - Pass Through Http Server

## Overview
* Implements an HTTP server on port 8081
* Responds to requests for the root path `/`
* Query String Parameters
  * `uri` (required): URI of the backend service to request (example: `?uri=http://localhost:8080/1000`)
  * `threadingModel` (optional): Threading model to use when waiting for the backend response
    * `async` (default): Full async (reactor-netty async)
    * `syncoverasync`: Sync over async (reactor-netty sync over async)
    * `sync`: Full sync (`_webClient.DownloadString(uri)`
      * `WebClient.DownloadString()` is only implemented as full sync on netfx48.  On netcoreapp22, it's implemented as sync-over-async.
* If the `uri` is missing or invalid, or if the `threadingModel` is invalid, the server returns `HTTP 500 Internal Server Error`
* Responds to all valid requests with the content from the backend response
* Responds to requests for `/reset` which resets the thread pool to a cached pool, for `reset/{int}` which resets the thread pool to a fixed pool
```
> curl -v "http://localhost:8081?uri=http://localhost:8080"
* Rebuilt URL to: http://localhost:8081/?uri=http://localhost:8080
*   Trying ::1...
* TCP_NODELAY set
* Connected to localhost (::1) port 8081 (#0)
> GET /?uri=http://localhost:8080 HTTP/1.1
> Host: localhost:8081
> User-Agent: curl/7.55.1
> Accept: */*
>
< HTTP/1.1 200 OK
< Threads: 14
< content-length: 13
<
Hello, World!* Connection #0 to host localhost left intact
```

## Prerequisites
* JDK >= 1.8 ([Download](https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html) | [Zulu for Azure](https://www.azul.com/downloads/azure-only/zulu/))
* Maven >= 3 ([Download](https://maven.apache.org/download.cgi?Preferred=ftp://mirror.reverse.net/pub/apache/)
* Add `{JDK installation location}/bin` and `{Maven extraction location}/bin` to `PATH`
* Set environment variable `JAVA_HOME` as `{JDK installation}` (usually C:\Program Files\Java\jdk1.8.0_xxx)
* (*Optional*) Set environment variable `MAVEN_HOME` as `{Maven extraction location}`

## Run
```
> mvn compile
> java -jar target\pass-through-http-server-1.0.0-jar-with-dependencies.jar
```
