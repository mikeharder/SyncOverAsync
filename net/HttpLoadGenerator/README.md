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

2019-05-15T19:15:12.3659412Z    Tot Req 101     Out Req 101     Suc Rsp 0       Err Rsp 0       Cur Q/s 101     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T19:15:13.3872407Z    Tot Req 201     Out Req 201     Suc Rsp 0       Err Rsp 0       Cur Q/s 98      Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T19:15:14.3883179Z    Tot Req 303     Out Req 303     Suc Rsp 0       Err Rsp 0       Cur Q/s 102     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T19:15:15.3899520Z    Tot Req 403     Out Req 403     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T19:15:16.3909960Z    Tot Req 503     Out Req 500     Suc Rsp 3       Err Rsp 0       Cur Q/s 100     Cur S/s 3       Cur E/s 0       Cur Lat 5003ms
2019-05-15T19:15:17.3916402Z    Tot Req 603     Out Req 500     Suc Rsp 103     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5012ms
2019-05-15T19:15:18.3924807Z    Tot Req 703     Out Req 500     Suc Rsp 203     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5011ms
2019-05-15T19:15:19.3940548Z    Tot Req 803     Out Req 500     Suc Rsp 303     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5012ms
2019-05-15T19:15:20.3956889Z    Tot Req 903     Out Req 500     Suc Rsp 403     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 5010ms
2019-05-15T19:15:21.3974299Z    Tot Req 1000    Out Req 499     Suc Rsp 501     Err Rsp 0       Cur Q/s 97      Cur S/s 98      Cur E/s 0       Cur Lat 5010ms
2019-05-15T19:15:22.3984271Z    Tot Req 1000    Out Req 397     Suc Rsp 603     Err Rsp 0       Cur Q/s 0       Cur S/s 102     Cur E/s 0       Cur Lat 5011ms
2019-05-15T19:15:23.3999767Z    Tot Req 1000    Out Req 296     Suc Rsp 704     Err Rsp 0       Cur Q/s 0       Cur S/s 101     Cur E/s 0       Cur Lat 5011ms
2019-05-15T19:15:24.4007951Z    Tot Req 1000    Out Req 196     Suc Rsp 804     Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 5011ms
2019-05-15T19:15:25.4017234Z    Tot Req 1000    Out Req 96      Suc Rsp 904     Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 5012ms
2019-05-15T19:15:26.4025651Z    Tot Req 1000    Out Req 0       Suc Rsp 1000    Err Rsp 0       Cur Q/s 0       Cur S/s 96      Cur E/s 0       Cur Lat 5011ms
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

## Detailed Results
After the test completes, detailed results are written to the specified file (default `latency.csv`).  This file can be post-processed for more detailed analysis, charts, etc.

```
StartMs,LatencyMs,HttpStatus
0.2677,1002.7453,200
11.5182,1002.491,200
21.5586,1010.4289,200
30.6076,1001.377,200
40.6098,1001.3743,200
...
```
