# SyncOverAsync - .NET - DelayHttpServer

## Overview
* Implements an HTTP server on port 8080
* If the request path is empty, responds immediately
* If the request path contains an integer, the server waits this many milliseconds before responding.
  * Example: A request to `http://localhost:8080/5000` would respond after 5 seconds (5000 milliseconds)
* If the request path cannot be parsed as an integer, the server returns `HTTP 500 Internal Server Error`
* Responds to all valid requests with the following response
```
< HTTP/1.1 200 OK
< Date: Fri, 10 May 2019 16:20:48 GMT
< Content-Type: text/plain
< Server: Kestrel
< Content-Length: 13
<
Hello, World!
```

## Prerequisites
* .NET Core SDK >= 3.0.100-preview5-011568 ([Download](https://dotnet.microsoft.com/download/dotnet-core/3.0) | [Docker](https://hub.docker.com/_/microsoft-dotnet-core-sdk/))

## Run
```
> dotnet run -c release

Configuration: Release
GC: Server

Hosting environment: Development
Content root path: D:\Git\SyncOverAsync\net\DelayHttpServer\bin\release\netcoreapp2.2\
Now listening on: http://0.0.0.0:8080
Application started. Press Ctrl+C to shut down.
```