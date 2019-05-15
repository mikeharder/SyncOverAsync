# SyncOverAsync - .NET - BlockingHttpClient

## Overview
* Implements an HTTP client which uses either async or sync-over-async to wait for responses
* Issues a fixed number of requests every second, regardless of how many requests are already queued
* Reports results once per second to monitor process health

## Prerequisites
* .NET Core SDK >= 2.2.203 ([Download](https://dotnet.microsoft.com/download/dotnet-core/2.2) | [Docker](https://hub.docker.com/_/microsoft-dotnet-core-sdk/))

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
  -w, --minWorkerThreads
  -c, --minCompletionPortThreads
```

## Results - Async - Healthy
```
>dotnet run -c release -u http://localhost:8080/5000 -r 100
Configuration: Release
GC: Server
ThreadPool.GetMinThreads(): 12, 12
ThreadPool.GetMaxThreads(): 32767, 1000

2019-05-10T22:26:20.5976335Z    Tot Req 101     Pen Req 0       Out Req 101     Tot Rsp 0       Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 48
2019-05-10T22:26:21.6018695Z    Tot Req 202     Pen Req 0       Out Req 202     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 48
2019-05-10T22:26:22.6067910Z    Tot Req 303     Pen Req 0       Out Req 303     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 59
2019-05-10T22:26:23.6108868Z    Tot Req 403     Pen Req 0       Out Req 403     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 59
2019-05-10T22:26:24.6143717Z    Tot Req 503     Pen Req 0       Out Req 503     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 59
2019-05-10T22:26:25.6183560Z    Tot Req 604     Pen Req 0       Out Req 604     Tot Rsp 0       Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 59
2019-05-10T22:26:26.6223472Z    Tot Req 704     Pen Req 0       Out Req 704     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 59
2019-05-10T22:26:27.6265531Z    Tot Req 805     Pen Req 0       Out Req 701     Tot Rsp 104     Cur Q/S 101     Cur R/S 104     Cur Lat 7016ms  Threads 59
2019-05-10T22:26:28.6309288Z    Tot Req 905     Pen Req 0       Out Req 701     Tot Rsp 204     Cur Q/S 100     Cur R/S 100     Cur Lat 7011ms  Threads 59
2019-05-10T22:26:29.6391063Z    Tot Req 1006    Pen Req 0       Out Req 701     Tot Rsp 305     Cur Q/S 100     Cur R/S 100     Cur Lat 7009ms  Threads 59
2019-05-10T22:26:30.6438534Z    Tot Req 1107    Pen Req 0       Out Req 702     Tot Rsp 405     Cur Q/S 101     Cur R/S 100     Cur Lat 7010ms  Threads 59
2019-05-10T22:26:31.6478602Z    Tot Req 1207    Pen Req 0       Out Req 701     Tot Rsp 506     Cur Q/S 100     Cur R/S 101     Cur Lat 7010ms  Threads 59
2019-05-10T22:26:32.6522474Z    Tot Req 1307    Pen Req 0       Out Req 605     Tot Rsp 702     Cur Q/S 100     Cur R/S 195     Cur Lat 6039ms  Threads 59
2019-05-10T22:26:33.6571592Z    Tot Req 1408    Pen Req 0       Out Req 508     Tot Rsp 900     Cur Q/S 101     Cur R/S 197     Cur Lat 6009ms  Threads 59
2019-05-10T22:26:34.6629850Z    Tot Req 1508    Pen Req 0       Out Req 501     Tot Rsp 1007    Cur Q/S 99      Cur R/S 106     Cur Lat 5120ms  Threads 59
2019-05-10T22:26:35.6669508Z    Tot Req 1609    Pen Req 0       Out Req 501     Tot Rsp 1108    Cur Q/S 101     Cur R/S 101     Cur Lat 5009ms  Threads 59
2019-05-10T22:26:36.6713541Z    Tot Req 1709    Pen Req 0       Out Req 501     Tot Rsp 1208    Cur Q/S 100     Cur R/S 100     Cur Lat 5008ms  Threads 59
2019-05-10T22:26:37.6754590Z    Tot Req 1810    Pen Req 0       Out Req 501     Tot Rsp 1309    Cur Q/S 101     Cur R/S 101     Cur Lat 5009ms  Threads 59
```

* Columns
  * `Tot Req`: Total requests queued (since the process started)
  * `Pen Req`: Requests queued but not yet executed (`[requests queued] - [requests executed]`)
  * `Out Req`: Requests executed but response has not been received (`[requests executed] - [responses received]`)
  * `Tot Rsp`: Total resposnes received (since the process started)
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
ThreadPool.GetMinThreads(): 12, 12
ThreadPool.GetMaxThreads(): 32767, 1000

2019-05-10T22:29:34.5363557Z    Tot Req 101     Pen Req 89      Out Req 12      Tot Rsp 0       Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 49
2019-05-10T22:29:36.5247057Z    Tot Req 300     Pen Req 198     Out Req 102     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 64
2019-05-10T22:29:38.5249561Z    Tot Req 501     Pen Req 399     Out Req 102     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 67
2019-05-10T22:29:40.5239663Z    Tot Req 701     Pen Req 599     Out Req 102     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 69
2019-05-10T22:29:41.5557299Z    Tot Req 804     Pen Req 702     Out Req 90      Tot Rsp 12      Cur Q/S 100     Cur R/S 12      Cur Lat 7143ms  Threads 70
2019-05-10T22:29:43.5240569Z    Tot Req 1001    Pen Req 899     Out Req 90      Tot Rsp 12      Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 72
2019-05-10T22:29:45.5246530Z    Tot Req 1201    Pen Req 1098    Out Req 91      Tot Rsp 12      Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 74
2019-05-10T22:29:46.5331470Z    Tot Req 1301    Pen Req 1198    Out Req 90      Tot Rsp 13      Cur Q/S 99      Cur R/S 1       Cur Lat 12890ms Threads 75
2019-05-10T22:29:47.5402530Z    Tot Req 1402    Pen Req 1299    Out Req 88      Tot Rsp 15      Cur Q/S 100     Cur R/S 2       Cur Lat 13378ms Threads 76
2019-05-10T22:29:48.5719189Z    Tot Req 1505    Pen Req 1402    Out Req 74      Tot Rsp 29      Cur Q/S 100     Cur R/S 14      Cur Lat 13985ms Threads 77
2019-05-10T22:29:50.5253807Z    Tot Req 1701    Pen Req 1598    Out Req 73      Tot Rsp 30      Cur Q/S 100     Cur R/S 1       Cur Lat 15725ms Threads 79
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
ThreadPool.GetMinThreads(): 12, 12
ThreadPool.GetMaxThreads(): 32767, 1000

2019-05-10T22:30:31.6247899Z    Tot Req 3       Pen Req 0       Out Req 3       Tot Rsp 0       Cur Q/S 3       Cur R/S 0       Cur Lat NaNms   Threads 35
2019-05-10T22:30:32.6284636Z    Tot Req 5       Pen Req 0       Out Req 5       Tot Rsp 0       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 37
2019-05-10T22:30:33.6329818Z    Tot Req 7       Pen Req 0       Out Req 7       Tot Rsp 0       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 41
2019-05-10T22:30:34.6377881Z    Tot Req 9       Pen Req 0       Out Req 9       Tot Rsp 0       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 43
2019-05-10T22:30:35.6506753Z    Tot Req 11      Pen Req 0       Out Req 11      Tot Rsp 0       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 44
2019-05-10T22:30:37.1118941Z    Tot Req 14      Pen Req 2       Out Req 12      Tot Rsp 0       Cur Q/S 2       Cur R/S 0       Cur Lat NaNms   Threads 45
2019-05-10T22:30:38.1209730Z    Tot Req 16      Pen Req 2       Out Req 12      Tot Rsp 2       Cur Q/S 2       Cur R/S 2       Cur Lat 7062ms  Threads 45
2019-05-10T22:30:39.6177303Z    Tot Req 19      Pen Req 1       Out Req 13      Tot Rsp 5       Cur Q/S 2       Cur R/S 2       Cur Lat 7006ms  Threads 45
2019-05-10T22:30:41.1173976Z    Tot Req 22      Pen Req 2       Out Req 12      Tot Rsp 8       Cur Q/S 2       Cur R/S 2       Cur Lat 7007ms  Threads 45
2019-05-10T22:30:42.1275574Z    Tot Req 24      Pen Req 2       Out Req 12      Tot Rsp 10      Cur Q/S 2       Cur R/S 2       Cur Lat 7016ms  Threads 45
2019-05-10T22:30:43.1322576Z    Tot Req 26      Pen Req 2       Out Req 12      Tot Rsp 12      Cur Q/S 2       Cur R/S 2       Cur Lat 6262ms  Threads 46
2019-05-10T22:30:44.1361970Z    Tot Req 28      Pen Req 0       Out Req 12      Tot Rsp 16      Cur Q/S 2       Cur R/S 4       Cur Lat 5894ms  Threads 48
2019-05-10T22:30:45.1401243Z    Tot Req 30      Pen Req 0       Out Req 10      Tot Rsp 20      Cur Q/S 2       Cur R/S 4       Cur Lat 6771ms  Threads 49
2019-05-10T22:30:46.1503915Z    Tot Req 32      Pen Req 0       Out Req 10      Tot Rsp 22      Cur Q/S 2       Cur R/S 2       Cur Lat 5274ms  Threads 50
2019-05-10T22:30:47.1544159Z    Tot Req 34      Pen Req 0       Out Req 10      Tot Rsp 24      Cur Q/S 2       Cur R/S 2       Cur Lat 5278ms  Threads 50
2019-05-10T22:30:48.1591992Z    Tot Req 36      Pen Req 0       Out Req 10      Tot Rsp 26      Cur Q/S 2       Cur R/S 2       Cur Lat 5276ms  Threads 50```
```

Or if the `minWorkerThreads` is set large enough:

```
>dotnet run -c release -u http://localhost:8080/5000 -r 100 -s -w 1000
Configuration: Release
GC: Server
ThreadPool.GetMinThreads(): 1000, 12
ThreadPool.GetMaxThreads(): 32767, 1000

2019-05-10T22:31:09.3228193Z    Tot Req 100     Pen Req 0       Out Req 100     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 141
2019-05-10T22:31:10.3270503Z    Tot Req 202     Pen Req 0       Out Req 202     Tot Rsp 0       Cur Q/S 101     Cur R/S 0       Cur Lat NaNms   Threads 240
2019-05-10T22:31:11.3351978Z    Tot Req 302     Pen Req 0       Out Req 302     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 354
2019-05-10T22:31:12.3396355Z    Tot Req 403     Pen Req 0       Out Req 403     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 452
2019-05-10T22:31:13.3595381Z    Tot Req 505     Pen Req 0       Out Req 505     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 554
2019-05-10T22:31:14.3644569Z    Tot Req 606     Pen Req 0       Out Req 606     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 655
2019-05-10T22:31:15.3726323Z    Tot Req 706     Pen Req 0       Out Req 706     Tot Rsp 0       Cur Q/S 100     Cur R/S 0       Cur Lat NaNms   Threads 758
2019-05-10T22:31:16.3771312Z    Tot Req 807     Pen Req 0       Out Req 701     Tot Rsp 106     Cur Q/S 100     Cur R/S 105     Cur Lat 7017ms  Threads 774
2019-05-10T22:31:17.3814327Z    Tot Req 907     Pen Req 0       Out Req 701     Tot Rsp 206     Cur Q/S 100     Cur R/S 100     Cur Lat 7010ms  Threads 774
2019-05-10T22:31:18.3859799Z    Tot Req 1008    Pen Req 0       Out Req 702     Tot Rsp 306     Cur Q/S 101     Cur R/S 100     Cur Lat 7011ms  Threads 774
2019-05-10T22:31:19.3907868Z    Tot Req 1108    Pen Req 0       Out Req 701     Tot Rsp 407     Cur Q/S 100     Cur R/S 101     Cur Lat 7012ms  Threads 774
2019-05-10T22:31:20.3959185Z    Tot Req 1209    Pen Req 0       Out Req 702     Tot Rsp 507     Cur Q/S 100     Cur R/S 99      Cur Lat 7011ms  Threads 774
2019-05-10T22:31:21.4004085Z    Tot Req 1309    Pen Req 0       Out Req 608     Tot Rsp 701     Cur Q/S 100     Cur R/S 193     Cur Lat 6040ms  Threads 774
2019-05-10T22:31:22.4049573Z    Tot Req 1410    Pen Req 0       Out Req 505     Tot Rsp 905     Cur Q/S 101     Cur R/S 203     Cur Lat 6009ms  Threads 774
2019-05-10T22:31:23.4201475Z    Tot Req 1511    Pen Req 0       Out Req 500     Tot Rsp 1011    Cur Q/S 99      Cur R/S 104     Cur Lat 5084ms  Threads 774
2019-05-10T22:31:24.4251274Z    Tot Req 1612    Pen Req 0       Out Req 501     Tot Rsp 1111    Cur Q/S 101     Cur R/S 100     Cur Lat 5008ms  Threads 774
2019-05-10T22:31:25.4303891Z    Tot Req 1712    Pen Req 0       Out Req 501     Tot Rsp 1211    Cur Q/S 99      Cur R/S 99      Cur Lat 5008ms  Threads 774
```