# SyncOverAsync - .NET - BlockingHttpClient

## Overview
* Implements an HTTP client which uses either async or sync-over-async to wait for responses
* Issues a fixed number of requests every second, regardless of how many requests are already queued
* Reports results once per second to monitor process health

## Prerequisites
* .NET SDK >= 2.2.203
  * [Download](https://dotnet.microsoft.com/download/dotnet-core/2.2)
  * [Docker](https://hub.docker.com/_/microsoft-dotnet-core-sdk/)

## Command-Line Parameters
```
> dotnet run -c release
BlockingHttpClient 1.0.0
Copyright (C) 2019 BlockingHttpClient

ERROR(S):
  Required option 'u, uri' is missing.
  -u, --uri                  Required.
  -r, --requestsPerSecond    (Default: 10)
  -s, --syncOverAsync        (Default: false)
```

## Results - Async - Healthy
```
>dotnet run -c release -u http://localhost:8080/5000 -r 100
Configuration: Release
GC: Server

2019-05-10T16:33:50.8428174Z    Tot Req 100     Tot Rsp 0       Out Req 100     Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 40
2019-05-10T16:33:51.8474952Z    Tot Req 201     Tot Rsp 0       Out Req 201     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 40
2019-05-10T16:33:52.8514344Z    Tot Req 302     Tot Rsp 0       Out Req 302     Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 51
2019-05-10T16:33:53.8554873Z    Tot Req 402     Tot Rsp 0       Out Req 402     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 51
2019-05-10T16:33:54.8714883Z    Tot Req 504     Tot Rsp 0       Out Req 504     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 51
2019-05-10T16:33:55.8754339Z    Tot Req 604     Tot Rsp 0       Out Req 604     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 51
2019-05-10T16:33:56.8794039Z    Tot Req 705     Tot Rsp 0       Out Req 705     Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 51
2019-05-10T16:33:57.8831952Z    Tot Req 805     Tot Rsp 105     Out Req 700     Cur Q/S 100     Cur R/S 105     Cur Lat 7012ms  Threads 52
2019-05-10T16:33:58.8874102Z    Tot Req 905     Tot Rsp 205     Out Req 700     Cur Q/S 100     Cur R/S 100     Cur Lat 7011ms  Threads 52
2019-05-10T16:33:59.8920747Z    Tot Req 1006    Tot Rsp 304     Out Req 702     Cur Q/S 101     Cur R/S 99      Cur Lat 7010ms  Threads 52
2019-05-10T16:34:00.8962193Z    Tot Req 1106    Tot Rsp 405     Out Req 701     Cur Q/S 100     Cur R/S 101     Cur Lat 7010ms  Threads 52
2019-05-10T16:34:01.9009203Z    Tot Req 1207    Tot Rsp 505     Out Req 702     Cur Q/S 101     Cur R/S 100     Cur Lat 7010ms  Threads 52
2019-05-10T16:34:02.9052941Z    Tot Req 1307    Tot Rsp 700     Out Req 607     Cur Q/S 100     Cur R/S 194     Cur Lat 6045ms  Threads 52
2019-05-10T16:34:03.9096754Z    Tot Req 1408    Tot Rsp 901     Out Req 507     Cur Q/S 101     Cur R/S 200     Cur Lat 6004ms  Threads 52
2019-05-10T16:34:04.9142927Z    Tot Req 1508    Tot Rsp 1006    Out Req 502     Cur Q/S 100     Cur R/S 105     Cur Lat 5123ms  Threads 52
2019-05-10T16:34:05.9191653Z    Tot Req 1608    Tot Rsp 1108    Out Req 500     Cur Q/S 100     Cur R/S 102     Cur Lat 5008ms  Threads 52
2019-05-10T16:34:06.9239612Z    Tot Req 1709    Tot Rsp 1208    Out Req 501     Cur Q/S 101     Cur R/S 100     Cur Lat 5008ms  Threads 52
```

* Columns
  * `Tot Req`: Total requests sent (since the process started)
  * `Tot Rsp`: Total resposnes received (since the process started)
  * `Out Req`: Outstanding requests (`[Tot Req] - [Tot Rsp]`)
  * `Cur Q/S`: Requests (Q) sent in the last second
  * `Cur R/S`: Responses (R) received in the last second
  * `Cur Lat`: Average latency of responses recieved in the last second
  * `Threads`: Current number of OS threads used by process
* The client is configured to make 100 requests per second, and the server waits 5 seconds before each response.
* In the steady-state, we expect:
  * 500 pending requests (100 requests/second * 5 seconds/request = 500 requests)
  * Average latency around 5000 ms (plus a few ms for transit)
  * Stable number of threads, much lower than the number of requests
* Results show the client is healthy when using async

## Results - SyncOverAsync - Unhealthy
```
>dotnet run -c release -u http://localhost:8080/5000 -r 100 -s
Configuration: Release
GC: Server

2019-05-10T16:41:14.2436592Z    Tot Req 108     Tot Rsp 0       Out Req 108     Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 47
2019-05-10T16:41:16.2326531Z    Tot Req 308     Tot Rsp 0       Out Req 308     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 60
2019-05-10T16:41:18.2320112Z    Tot Req 508     Tot Rsp 0       Out Req 508     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 62
2019-05-10T16:41:20.2326863Z    Tot Req 708     Tot Rsp 0       Out Req 708     Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 64
2019-05-10T16:41:21.2618117Z    Tot Req 811     Tot Rsp 12      Out Req 799     Cur Q/S 100     Cur R/S 12      Cur Lat 7129ms  Threads 65
2019-05-10T16:41:23.2323610Z    Tot Req 1008    Tot Rsp 13      Out Req 995     Cur Q/S 100     Cur R/S 1       Cur Lat 8959ms  Threads 67
2019-05-10T16:41:24.2494097Z    Tot Req 1109    Tot Rsp 15      Out Req 1094    Cur Q/S 99      Cur R/S 2       Cur Lat 10454ms Threads 68
2019-05-10T16:41:26.2329733Z    Tot Req 1308    Tot Rsp 16      Out Req 1292    Cur Q/S 100     Cur R/S 1       Cur Lat 11933ms Threads 70
2019-05-10T16:41:27.2424594Z    Tot Req 1409    Tot Rsp 18      Out Req 1391    Cur Q/S 100     Cur R/S 2       Cur Lat 13419ms Threads 71
2019-05-10T16:41:28.2728379Z    Tot Req 1512    Tot Rsp 31      Out Req 1481    Cur Q/S 100     Cur R/S 13      Cur Lat 14046ms Threads 72
2019-05-10T16:41:30.2321314Z    Tot Req 1707    Tot Rsp 33      Out Req 1674    Cur Q/S 100     Cur R/S 1       Cur Lat 15774ms Threads 74
```

* The client is unhealthy when using sync-over-async and sending 100 requests per second
* Outstanding requests are increasing much faster than incoming responses
* Latency is increasing, since responses are blocked waiting for available threads
* .NET is trying to help by adding threads to the threadpool, but the rate is far too low.  The .NET ThreadPool adds a maximum of 1-2 threads/second, but 100 blocking requests are added each second.  The ThreadPool will never catch up.
* This scenario requires at least 500 threads to handle the load, since 500 threads will be blocked waiting for responses (100 requests/second * 5 seconds/request).
* Possible mitigation: `ThreadPool.SetMinThreads(workerThreads: 500, completionPortThreads: 500)`

## Results - SyncOverAsync - Healthy
It is possible to use SyncOverAsync if the request rate is low enough:

```
>dotnet run -c release -u http://localhost:8080/5000 -r 2 -s
Configuration: Release
GC: Server

2019-05-10T16:50:09.8062734Z    Tot Req 2       Tot Rsp 0       Out Req 2       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 36
2019-05-10T16:50:10.8110188Z    Tot Req 5       Tot Rsp 0       Out Req 5       Cur Q/S 3       Cur R/S 0       Cur Lat NaNms   Threads 37
2019-05-10T16:50:11.8154161Z    Tot Req 7       Tot Rsp 0       Out Req 7       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 41
2019-05-10T16:50:12.8213634Z    Tot Req 9       Tot Rsp 0       Out Req 9       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 43
2019-05-10T16:50:13.8377999Z    Tot Req 11      Tot Rsp 0       Out Req 11      Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 44
2019-05-10T16:50:14.8741740Z    Tot Req 13      Tot Rsp 0       Out Req 13      Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 45
2019-05-10T16:50:15.9273495Z    Tot Req 15      Tot Rsp 1       Out Req 14      Cur Q/S 2       Cur R/S 1       Cur Lat 7123ms  Threads 46
2019-05-10T16:50:17.3093556Z    Tot Req 18      Tot Rsp 4       Out Req 14      Cur Q/S 2       Cur R/S 2       Cur Lat 7014ms  Threads 46
2019-05-10T16:50:18.8075936Z    Tot Req 21      Tot Rsp 7       Out Req 14      Cur Q/S 2       Cur R/S 2       Cur Lat 7008ms  Threads 46
2019-05-10T16:50:20.3068153Z    Tot Req 24      Tot Rsp 10      Out Req 14      Cur Q/S 2       Cur R/S 2       Cur Lat 7007ms  Threads 46
2019-05-10T16:50:21.8041947Z    Tot Req 27      Tot Rsp 13      Out Req 14      Cur Q/S 2       Cur R/S 2       Cur Lat 5674ms  Threads 46
2019-05-10T16:50:22.8087639Z    Tot Req 29      Tot Rsp 16      Out Req 13      Cur Q/S 2       Cur R/S 3       Cur Lat 5870ms  Threads 46
2019-05-10T16:50:23.8127914Z    Tot Req 31      Tot Rsp 21      Out Req 10      Cur Q/S 2       Cur R/S 5       Cur Lat 6651ms  Threads 46
2019-05-10T16:50:24.8174420Z    Tot Req 33      Tot Rsp 23      Out Req 10      Cur Q/S 2       Cur R/S 2       Cur Lat 5005ms  Threads 46
2019-05-10T16:50:25.8220062Z    Tot Req 35      Tot Rsp 25      Out Req 10      Cur Q/S 2       Cur R/S 2       Cur Lat 5007ms  Threads 46
2019-05-10T16:50:26.8266766Z    Tot Req 37      Tot Rsp 27      Out Req 10      Cur Q/S 2       Cur R/S 2       Cur Lat 5011ms  Threads 45
2019-05-10T16:50:27.8308667Z    Tot Req 39      Tot Rsp 29      Out Req 10      Cur Q/S 2       Cur R/S 2       Cur Lat 5006ms  Threads 45
```