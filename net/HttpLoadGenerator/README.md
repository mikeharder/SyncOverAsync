# SyncOverAsync - .NET - HttpLoadGenerator

## Overview
* Implements an HTTP load generator
* Issues a fixed number of requests every second, regardless of how many requests are already queued
* Reports intermediate results once per second

## Prerequisites
* .NET Core SDK >= 2.2.203 ([Download](https://dotnet.microsoft.com/download/dotnet-core/2.2) | [Docker](https://hub.docker.com/_/microsoft-dotnet-core-sdk/))

## Command-Line Parameters
```
> dotnet run -c release

HttpLoadGenerator 1.0.0
Copyright (C) 2019 HttpLoadGenerator

ERROR(S):
  Required option 'u, uri' is missing.

  -u, --uri                      Required.
  -r, --requestsPerSecond        (Default: 10)
  -d, --durationSeconds          (Default: 10)
  -w, --warmupDurationSeconds    (Default: 10) Warmup duration in seconds.  Set to 0 to skip warmup.
  -o, --outputFile               (Default: latency.csv)
```

## Results - Async - Healthy
```
> dotnet run -c release -u "http://localhost:8081?uri=http://localhost:8080/5000" -r 100

2019-05-15T19:44:03.2662832Z    Tot Req 99      Out Req 99      Suc Rsp 0       Err Rsp 0       Cur Q/s 98      Cur S/s 0       Cur E/s 0       Cur Lat NaNms   Srv Thd 66
2019-05-15T19:44:04.2865652Z    Tot Req 201     Out Req 201     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms   Srv Thd 66
2019-05-15T19:44:05.3061603Z    Tot Req 303     Out Req 303     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms   Srv Thd 66
2019-05-15T19:44:06.3163373Z    Tot Req 404     Out Req 404     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms   Srv Thd 66
2019-05-15T19:44:07.3264046Z    Tot Req 506     Out Req 501     Suc Rsp 5       Err Rsp 0       Cur Q/s 101     Cur S/s 5       Cur E/s 0       Cur Lat 5010ms  Srv Thd 70
2019-05-15T19:44:08.3362078Z    Tot Req 606     Out Req 500     Suc Rsp 106     Err Rsp 0       Cur Q/s 99      Cur S/s 100     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:09.3464505Z    Tot Req 707     Out Req 500     Suc Rsp 207     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:10.3663714Z    Tot Req 809     Out Req 500     Suc Rsp 309     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:11.3762315Z    Tot Req 910     Out Req 500     Suc Rsp 410     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5014ms  Srv Thd 70
2019-05-15T19:44:12.3961670Z    Tot Req 1000    Out Req 488     Suc Rsp 512     Err Rsp 0       Cur Q/s 88      Cur S/s 100     Cur E/s 0       Cur Lat 5014ms  Srv Thd 70
2019-05-15T19:44:13.4165269Z    Tot Req 1000    Out Req 386     Suc Rsp 614     Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 5014ms  Srv Thd 70
2019-05-15T19:44:14.4360626Z    Tot Req 1000    Out Req 284     Suc Rsp 716     Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:15.4561818Z    Tot Req 1000    Out Req 182     Suc Rsp 818     Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:16.4570099Z    Tot Req 1000    Out Req 80      Suc Rsp 920     Err Rsp 0       Cur Q/s 0       Cur S/s 102     Cur E/s 0       Cur Lat 5013ms  Srv Thd 70
2019-05-15T19:44:17.4762273Z    Tot Req 1000    Out Req 0       Suc Rsp 1000    Err Rsp 0       Cur Q/s 0       Cur S/s 78      Cur E/s 0       Cur Lat 5012ms  Srv Thd 70
```

* Columns
  * `Tot Req`: Total requests queued (since the process started)
  * `Out Req`: Requests executed but response has not been received
  * `Suc Rsp`: Successful responses received (since the process started)
  * `Err Rsp`: Error responses received (since the process started)
  * `Cur Q/s`: Requests (Q) sent in the last second
  * `Cur S/s`: Successful responses (S) received in the last second
  * `Cur E/s`: Error responses (E) received in the last second
  * `Cur Lat`: Average latency of all responses (successful and error) received in the last second
  * `Srv Thd`: Threads used by the web server application

## Results - SyncOverAsync - Unhealthy


## Detailed Results
After the test completes, detailed results are written to the specified file (default `latency.csv`).  This file can be post-processed for more detailed analysis, charts, etc.

```
StartMs,LatencyMs,HttpStatus,ServerThreads
0.2677,1002.7453,200,70
11.5182,1002.491,200,70
21.5586,1010.4289,200,70
30.6076,1001.377,200,70
40.6098,1001.3743,200,70
...
```
