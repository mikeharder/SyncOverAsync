# SyncOverAsync - .NET - DelayHttpServer

## Overview
* Implements an HTTP server on port 8081
* Responds to requests for the root path `/`
* Query String Parameters
  * `uri` (required): URI of the backend service to request (example: `?uri=http://localhost:8080/1000`)
  * `threadingModel` (optional): Threading model to use when waiting for the backend response
    * `async` (default): Full async (`await _httpClient.GetAsync(uri)`)
    * `syncoverasync`: Sync over async (`_httpClient.GetAsync(uri).Result`)
    * `sync`: Full sync (`_webClient.DownloadString(uri)`
      * `WebClient.DownloadString()` is only implemented as full sync on netfx48.  On netcoreapp22, it's implemented as sync-over-async.
  * `minWorkerThreads` (optional): Minimum number of worker threads in the ThreadPool
* If the `uri` is missing or invalid, or if the `threadingModel` is invalid, the server returns `HTTP 500 Internal Server Error`
* Responds to all valid requests with the content from the backend response
```
> curl -v "http://localhost:8081?uri=http://localhost:8080"

< HTTP/1.1 200 OK
< Date: Wed, 15 May 2019 19:11:35 GMT
< Content-Type: text/plain; charset=utf-8
< Server: Kestrel
< Transfer-Encoding: chunked
< Threads: 50
<
Hello, World!
```

## Prerequisites
* .NET Core SDK >= 3.0.100-preview5-011568 ([Download](https://dotnet.microsoft.com/download/dotnet-core/3.0) | [Docker](https://hub.docker.com/_/microsoft-dotnet-core-sdk/))
* .NET Framework >= 4.8 (Windows Only) ([Download](https://dotnet.microsoft.com/download/dotnet-framework/net48))

## Run
```
> dotnet run -c release -f netfx48

Configuration: Release
GC: Server

Hosting environment: Development
Content root path: D:\Git\SyncOverAsync\net\PassthroughHttpServer
Now listening on: http://0.0.0.0:8081
Application started. Press Ctrl+C to shut down.
```