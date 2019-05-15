# SyncOverAsync - .NET - HttpLoadGenerator

## Overview
* Implements an HTTP load generator
* Issues a fixed number of requests every second, regardless of how many requests are already queued
* Reports intermediate results once per second
* 

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

## Results
```
>dotnet run -c release -u http://localhost:8080/1000 -r 100
Configuration: Release
GC: Server

Warming up for 10 seconds...
2019-05-15T01:17:44.1180384Z    Tot Req 101     Out Req 101     Suc Rsp 0       Err Rsp 0       Cur Q/s 101     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T01:17:45.1189828Z    Tot Req 201     Out Req 201     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T01:17:46.1200140Z    Tot Req 301     Out Req 301     Suc Rsp 0       Err Rsp 0       Cur Q/s 100     Cur S/s 0       Cur E/s 0       Cur Lat NaNms
2019-05-15T01:17:47.1209859Z    Tot Req 401     Out Req 301     Suc Rsp 100     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 3015ms
2019-05-15T01:17:48.1219831Z    Tot Req 501     Out Req 214     Suc Rsp 287     Err Rsp 0       Cur Q/s 100     Cur S/s 187     Cur E/s 0       Cur Lat 2079ms
2019-05-15T01:17:49.1231961Z    Tot Req 601     Out Req 114     Suc Rsp 487     Err Rsp 0       Cur Q/s 100     Cur S/s 200     Cur E/s 0       Cur Lat 2009ms
2019-05-15T01:17:50.1245014Z    Tot Req 702     Out Req 101     Suc Rsp 601     Err Rsp 0       Cur Q/s 101     Cur S/s 114     Cur E/s 0       Cur Lat 1237ms
2019-05-15T01:17:51.1260965Z    Tot Req 802     Out Req 100     Suc Rsp 702     Err Rsp 0       Cur Q/s 100     Cur S/s 101     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:52.1270687Z    Tot Req 902     Out Req 101     Suc Rsp 801     Err Rsp 0       Cur Q/s 100     Cur S/s 99      Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:53.1275739Z    Tot Req 1000    Out Req 99      Suc Rsp 901     Err Rsp 0       Cur Q/s 98      Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:54.1289056Z    Tot Req 1000    Out Req 0       Suc Rsp 1000    Err Rsp 0       Cur Q/s 0       Cur S/s 99      Cur E/s 0       Cur Lat 1008ms

Running for 10 seconds...
2019-05-15T01:17:55.1309625Z    Tot Req 101     Out Req 100     Suc Rsp 1       Err Rsp 0       Cur Q/s 101     Cur S/s 1       Cur E/s 0       Cur Lat 1000ms
2019-05-15T01:17:56.1318402Z    Tot Req 200     Out Req 100     Suc Rsp 100     Err Rsp 0       Cur Q/s 99      Cur S/s 99      Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:57.1334273Z    Tot Req 301     Out Req 101     Suc Rsp 200     Err Rsp 0       Cur Q/s 101     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:58.1340813Z    Tot Req 401     Out Req 101     Suc Rsp 300     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:17:59.1349338Z    Tot Req 501     Out Req 101     Suc Rsp 400     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:18:00.1354162Z    Tot Req 601     Out Req 101     Suc Rsp 500     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:18:01.1367617Z    Tot Req 701     Out Req 101     Suc Rsp 600     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1007ms
2019-05-15T01:18:02.1373850Z    Tot Req 801     Out Req 101     Suc Rsp 700     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:18:03.1388756Z    Tot Req 901     Out Req 101     Suc Rsp 800     Err Rsp 0       Cur Q/s 100     Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:18:04.1398267Z    Tot Req 1000    Out Req 100     Suc Rsp 900     Err Rsp 0       Cur Q/s 99      Cur S/s 100     Cur E/s 0       Cur Lat 1008ms
2019-05-15T01:18:05.1412760Z    Tot Req 1000    Out Req 0       Suc Rsp 1000    Err Rsp 0       Cur Q/s 0       Cur S/s 100     Cur E/s 0       Cur Lat 1009ms
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
