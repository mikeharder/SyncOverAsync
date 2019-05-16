# SyncOverAsync - Java

## Overview
* Implements an HTTP client which uses either async or sync-over-async to wait for responses
* Issues a fixed number of requests every second, regardless of how many requests are already queued
* Reports results once per second to monitor process health

## Prerequisites
* JDK >= 1.8 ([Download](https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html) | [Zulu for Azure](https://www.azul.com/downloads/azure-only/zulu/))
* Maven >= 3 ([Download](https://maven.apache.org/download.cgi?Preferred=ftp://mirror.reverse.net/pub/apache/)
* Add `{JDK installation location}/bin` and `{Maven extraction location}/bin` to `PATH`
* Set environment variable `JAVA_HOME` as `{JDK installation}` (usually C:\Program Files\Java\jdk1.8.0_xxx)
* (*Optional*) Set environment variable `MAVEN_HOME` as `{Maven extraction location}`

## Command-Line Parameters
```
> mvn compile [ -Duri=http://localhost:8080/5000 ] [ -Drps=100 ] [ -Dsoa=NETTY_SYNC_OVER_ASYNC ]

  -Duri        Request URI. Default: http://localhost:8080/5000
  -Drqs        Requests per second. Default: 100
  -Dsoa        Sync over async strategy, accepted values are NETTY_ASYNC, NETTY_SYNC_OVER_ASYNC, NETTY_REACTOR, OKHTTP_SYNC. Default: NETTY_SYNC_OVER_ASYNC
  -Dduration   How many seconds this application will run. Ctrl-C on the maven command doesn't kill the Java process so this is necessary for freeing up the resources. Default: 15
```

## Results - Async - Healthy
```
> mvn compile -Dsoa=NETTY_ASYNC -Drps=1000
[INFO] Scanning for projects...
[INFO]
[INFO] ---------------------< com.azure:sync-over-async >----------------------
[INFO] Building sync-over-async 1.0.0
[INFO] --------------------------------[ jar ]---------------------------------
[INFO]
[INFO] --- maven-resources-plugin:2.6:resources (default-resources) @ sync-over-async ---
[WARNING] Using platform encoding (Cp1252 actually) to copy filtered resources, i.e. build is platform dependent!
[INFO] Copying 0 resource
[INFO]
[INFO] --- maven-compiler-plugin:3.8.0:compile (default-compile) @ sync-over-async ---
[INFO] Nothing to compile - all classes are up to date
[INFO]
[INFO] --- exec-maven-plugin:1.6.0:exec (default) @ sync-over-async ---
Running config: uri: http://localhost:8080/5000, request/sec: 1000, syncOverAsync: NETTY_ASYNC
2019-05-13T23:10:14.894-07:00   Tot Req 1       Tot Rsp 0       Out Req 1       Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 3
2019-05-13T23:10:15.868-07:00   Tot Req 1       Tot Rsp 0       Out Req 1       Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 11
2019-05-13T23:10:16.868-07:00   Tot Req 541     Tot Rsp 0       Out Req 541     Cur Q/S 540     Cur R/S 0       Cur Lat 0ms     Threads 11
2019-05-13T23:10:17.874-07:00   Tot Req 1983    Tot Rsp 0       Out Req 1983    Cur Q/S 1433    Cur R/S 0       Cur Lat 0ms     Threads 11
2019-05-13T23:10:18.874-07:00   Tot Req 5287    Tot Rsp 0       Out Req 5287    Cur Q/S 3304    Cur R/S 0       Cur Lat 0ms     Threads 13
2019-05-13T23:10:19.951-07:00   Tot Req 6405    Tot Rsp 0       Out Req 6405    Cur Q/S 1039    Cur R/S 0       Cur Lat 0ms     Threads 15
2019-05-13T23:10:20.870-07:00   Tot Req 7328    Tot Rsp 0       Out Req 7328    Cur Q/S 1003    Cur R/S 0       Cur Lat 0ms     Threads 15
2019-05-13T23:10:21.868-07:00   Tot Req 8325    Tot Rsp 380     Out Req 7945    Cur Q/S 1000    Cur R/S 381     Cur Lat 1976ms  Threads 15
2019-05-13T23:10:22.880-07:00   Tot Req 9337    Tot Rsp 1881    Out Req 7456    Cur Q/S 999     Cur R/S 1481    Cur Lat 7514ms  Threads 15
2019-05-13T23:10:23.862-07:00   Tot Req 10322   Tot Rsp 3587    Out Req 6735    Cur Q/S 1003    Cur R/S 1737    Cur Lat 8907ms  Threads 15
2019-05-13T23:10:24.860-07:00   Tot Req 11317   Tot Rsp 6196    Out Req 5121    Cur Q/S 996     Cur R/S 2614    Cur Lat 15125ms Threads 15
2019-05-13T23:10:25.861-07:00   Tot Req 12320   Tot Rsp 7297    Out Req 5023    Cur Q/S 1001    Cur R/S 1099    Cur Lat 5541ms  Threads 15
2019-05-13T23:10:26.861-07:00   Tot Req 13320   Tot Rsp 8286    Out Req 5034    Cur Q/S 1001    Cur R/S 989     Cur Lat 4979ms  Threads 15
2019-05-13T23:10:27.860-07:00   Tot Req 14320   Tot Rsp 9283    Out Req 5037    Cur Q/S 1000    Cur R/S 997     Cur Lat 5007ms  Threads 15
2019-05-13T23:10:28.860-07:00   Tot Req 15319   Tot Rsp 10306   Out Req 5013    Cur Q/S 999     Cur R/S 1023    Cur Lat 5139ms  Threads 15
2019-05-13T23:10:29.860-07:00   Tot Req 16320   Tot Rsp 11298   Out Req 5022    Cur Q/S 1001    Cur R/S 992     Cur Lat 4968ms  Threads 15
2019-05-13T23:10:30.861-07:00   Tot Req 17320   Tot Rsp 12304   Out Req 5016    Cur Q/S 999     Cur R/S 1004    Cur Lat 5042ms  Threads 15
2019-05-13T23:10:31.862-07:00   Tot Req 18322   Tot Rsp 13315   Out Req 5007    Cur Q/S 1000    Cur R/S 1009    Cur Lat 5057ms  Threads 15
2019-05-13T23:10:32.861-07:00   Tot Req 19321   Tot Rsp 14317   Out Req 5004    Cur Q/S 1001    Cur R/S 1004    Cur Lat 5025ms  Threads 15
2019-05-13T23:10:33.861-07:00   Tot Req 20321   Tot Rsp 15316   Out Req 5005    Cur Q/S 1000    Cur R/S 999     Cur Lat 5005ms  Threads 15
2019-05-13T23:10:34.859-07:00   Tot Req 21320   Tot Rsp 16316   Out Req 5004    Cur Q/S 999     Cur R/S 1000    Cur Lat 5015ms  Threads 15
2019-05-13T23:10:35.862-07:00   Tot Req 22321   Tot Rsp 17317   Out Req 5004    Cur Q/S 1000    Cur R/S 1000    Cur Lat 5010ms  Threads 15
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
* The client is configured to make 1000 requests per second, and the server waits 5 seconds before each response.
* In the steady-state, we expect:
  * 5000 pending requests (1000 requests/second * 5 seconds/request = 5000 requests)
  * Average latency around 5000 ms (plus a few ms for transit)
  * Stable number of threads, much lower than the number of requests
* Results show the client is healthy when using async

## Results - SyncOverAsync - Unhealthy
```
> mvn compile -Drps=1000
[INFO] Scanning for projects...
[INFO]
[INFO] ---------------------< com.azure:sync-over-async >----------------------
[INFO] Building sync-over-async 1.0.0
[INFO] --------------------------------[ jar ]---------------------------------
[INFO]
[INFO] --- maven-resources-plugin:2.6:resources (default-resources) @ sync-over-async ---
[WARNING] Using platform encoding (Cp1252 actually) to copy filtered resources, i.e. build is platform dependent!
[INFO] Copying 0 resource
[INFO]
[INFO] --- maven-compiler-plugin:3.8.0:compile (default-compile) @ sync-over-async ---
[INFO] Nothing to compile - all classes are up to date
[INFO]
[INFO] --- exec-maven-plugin:1.6.0:exec (default) @ sync-over-async ---
Running config: uri: http://localhost:8080/5000, request/sec: 1000, syncOverAsync: NETTY_SYNC_OVER_ASYNC
2019-05-13T23:12:21.021-07:00   Tot Req 1303    Tot Rsp 0       Out Req 1303    Cur Q/S 839     Cur R/S 0       Cur Lat 0ms     Threads 1306
2019-05-13T23:12:21.997-07:00   Tot Req 2523    Tot Rsp 0       Out Req 2523    Cur Q/S 1203    Cur R/S 0       Cur Lat 0ms     Threads 2526
2019-05-13T23:12:22.980-07:00   Tot Req 3329    Tot Rsp 0       Out Req 3329    Cur Q/S 820     Cur R/S 0       Cur Lat 0ms     Threads 3415
2019-05-13T23:12:24.046-07:00   Tot Req 3330    Tot Rsp 0       Out Req 3330    Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 4123
2019-05-13T23:12:24.993-07:00   Tot Req 3338    Tot Rsp 0       Out Req 3338    Cur Q/S 8       Cur R/S 0       Cur Lat 0ms     Threads 5557
2019-05-13T23:12:26.313-07:00   Tot Req 3339    Tot Rsp 0       Out Req 3339    Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 6619
2019-05-13T23:12:26.992-07:00   Tot Req 3339    Tot Rsp 0       Out Req 3339    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 7703
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9008
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9007
2019-05-13T23:12:38.198-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 9007
2019-05-13T23:12:39.028-07:00   Tot Req 3341    Tot Rsp 0       Out Req 3341    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 10167
2019-05-13T23:12:55.487-07:00   Tot Req 3342    Tot Rsp 120     Out Req 3222    Cur Q/S 0       Cur R/S 7       Cur Lat 3582647ms       Threads 8596
2019-05-13T23:12:55.487-07:00   Tot Req 3342    Tot Rsp 120     Out Req 3222    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 8594
```

* The client is unhealthy when using sync-over-async and sending 1000 requests per second
* A new thread is being created every time a new request is initiated
* No cycle for Netty to read from sockets
* Reached over 10,000 threads

## Results - SyncOverAsync - Healthy
It is possible to use SyncOverAsync if the request rate is low enough:

```
> mvn compile -Drps=300
[INFO] Scanning for projects...
[INFO]
[INFO] ---------------------< com.azure:sync-over-async >----------------------
[INFO] Building sync-over-async 1.0.0
[INFO] --------------------------------[ jar ]---------------------------------
[INFO]
[INFO] --- maven-resources-plugin:2.6:resources (default-resources) @ sync-over-async ---
[WARNING] Using platform encoding (Cp1252 actually) to copy filtered resources, i.e. build is platform dependent!
[INFO] Copying 0 resource
[INFO]
[INFO] --- maven-compiler-plugin:3.8.0:compile (default-compile) @ sync-over-async ---
[INFO] Nothing to compile - all classes are up to date
[INFO]
[INFO] --- exec-maven-plugin:1.6.0:exec (default) @ sync-over-async ---
Running config: uri: http://localhost:8080/5000, request/sec: 300, syncOverAsync: NETTY_SYNC_OVER_ASYNC
2019-05-13T23:16:14.986-07:00   Tot Req 379     Tot Rsp 0       Out Req 379     Cur Q/S 300     Cur R/S 0       Cur Lat 0ms     Threads 382
2019-05-13T23:16:15.982-07:00   Tot Req 685     Tot Rsp 0       Out Req 685     Cur Q/S 299     Cur R/S 0       Cur Lat 0ms     Threads 688
2019-05-13T23:16:16.966-07:00   Tot Req 718     Tot Rsp 0       Out Req 718     Cur Q/S 33      Cur R/S 0       Cur Lat 0ms     Threads 2074
2019-05-13T23:16:17.972-07:00   Tot Req 719     Tot Rsp 0       Out Req 719     Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 3823
2019-05-13T23:16:21.791-07:00   Tot Req 720     Tot Rsp 0       Out Req 720     Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 4561
2019-05-13T23:16:21.791-07:00   Tot Req 720     Tot Rsp 0       Out Req 720     Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 4561
2019-05-13T23:16:21.791-07:00   Tot Req 720     Tot Rsp 0       Out Req 720     Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 4561
2019-05-13T23:16:21.975-07:00   Tot Req 720     Tot Rsp 0       Out Req 720     Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 4577
2019-05-13T23:16:24.522-07:00   Tot Req 721     Tot Rsp 0       Out Req 721     Cur Q/S 0       Cur R/S 0       Cur Lat 0ms     Threads 5411
2019-05-13T23:16:24.522-07:00   Tot Req 722     Tot Rsp 0       Out Req 722     Cur Q/S 8       Cur R/S 0       Cur Lat 0ms     Threads 5410
2019-05-13T23:16:25.157-07:00   Tot Req 722     Tot Rsp 0       Out Req 722     Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 5378
2019-05-13T23:16:25.973-07:00   Tot Req 728     Tot Rsp 0       Out Req 728     Cur Q/S 7       Cur R/S 0       Cur Lat 0ms     Threads 4878
2019-05-13T23:16:28.635-07:00   Tot Req 2798    Tot Rsp 0       Out Req 2798    Cur Q/S 777     Cur R/S 0       Cur Lat 0ms     Threads 5037
2019-05-13T23:16:28.636-07:00   Tot Req 2798    Tot Rsp 0       Out Req 2798    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 5029
2019-05-13T23:16:30.119-07:00   Tot Req 2798    Tot Rsp 0       Out Req 2798    Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 4675
2019-05-13T23:16:30.120-07:00   Tot Req 2799    Tot Rsp 0       Out Req 2799    Cur Q/S 1000    Cur R/S 0       Cur Lat 0ms     Threads 4674
2019-05-13T23:16:30.961-07:00   Tot Req 3371    Tot Rsp 0       Out Req 3371    Cur Q/S 679     Cur R/S 0       Cur Lat 0ms     Threads 4844
2019-05-13T23:16:31.965-07:00   Tot Req 5154    Tot Rsp 0       Out Req 5154    Cur Q/S 1777    Cur R/S 0       Cur Lat 0ms     Threads 6334
2019-05-13T23:16:32.965-07:00   Tot Req 5780    Tot Rsp 0       Out Req 5780    Cur Q/S 626     Cur R/S 0       Cur Lat 0ms     Threads 6350
2019-05-13T23:16:33.964-07:00   Tot Req 6051    Tot Rsp 0       Out Req 6051    Cur Q/S 267     Cur R/S 0       Cur Lat 0ms     Threads 6478
2019-05-13T23:16:34.964-07:00   Tot Req 6372    Tot Rsp 0       Out Req 6372    Cur Q/S 325     Cur R/S 0       Cur Lat 0ms     Threads 6769
2019-05-13T23:16:36.086-07:00   Tot Req 6706    Tot Rsp 0       Out Req 6706    Cur Q/S 296     Cur R/S 0       Cur Lat 0ms     Threads 6948
2019-05-13T23:16:37.117-07:00   Tot Req 7026    Tot Rsp 0       Out Req 7026    Cur Q/S 311     Cur R/S 0       Cur Lat 0ms     Threads 7050
2019-05-13T23:16:37.962-07:00   Tot Req 7279    Tot Rsp 0       Out Req 7279    Cur Q/S 299     Cur R/S 0       Cur Lat 0ms     Threads 7290
2019-05-13T23:16:38.961-07:00   Tot Req 7579    Tot Rsp 0       Out Req 7579    Cur Q/S 300     Cur R/S 0       Cur Lat 0ms     Threads 7592
2019-05-13T23:16:39.962-07:00   Tot Req 7879    Tot Rsp 0       Out Req 7879    Cur Q/S 300     Cur R/S 0       Cur Lat 0ms     Threads 7894
2019-05-13T23:16:40.961-07:00   Tot Req 8179    Tot Rsp 0       Out Req 8179    Cur Q/S 300     Cur R/S 0       Cur Lat 0ms     Threads 8193
2019-05-13T23:16:41.962-07:00   Tot Req 8479    Tot Rsp 5326    Out Req 3153    Cur Q/S 299     Cur R/S 5320    Cur Lat 238589ms        Threads 8270
2019-05-13T23:16:42.962-07:00   Tot Req 8779    Tot Rsp 7275    Out Req 1504    Cur Q/S 300     Cur R/S 1949    Cur Lat 66368ms Threads 8270
2019-05-13T23:16:43.962-07:00   Tot Req 9079    Tot Rsp 7574    Out Req 1505    Cur Q/S 300     Cur R/S 299     Cur Lat 4996ms  Threads 8270
2019-05-13T23:16:44.962-07:00   Tot Req 9379    Tot Rsp 7875    Out Req 1504    Cur Q/S 299     Cur R/S 300     Cur Lat 5034ms  Threads 8270
2019-05-13T23:16:45.962-07:00   Tot Req 9679    Tot Rsp 8175    Out Req 1504    Cur Q/S 300     Cur R/S 300     Cur Lat 5013ms  Threads 8270
2019-05-13T23:16:46.962-07:00   Tot Req 9979    Tot Rsp 8445    Out Req 1534    Cur Q/S 300     Cur R/S 270     Cur Lat 4621ms  Threads 8270
2019-05-13T23:16:47.962-07:00   Tot Req 10279   Tot Rsp 8777    Out Req 1502    Cur Q/S 300     Cur R/S 332     Cur Lat 5583ms  Threads 8270
2019-05-13T23:16:48.962-07:00   Tot Req 10579   Tot Rsp 9075    Out Req 1504    Cur Q/S 300     Cur R/S 298     Cur Lat 4977ms  Threads 8270
2019-05-13T23:16:49.963-07:00   Tot Req 10880   Tot Rsp 9376    Out Req 1504    Cur Q/S 300     Cur R/S 300     Cur Lat 5011ms  Threads 8270
2019-05-13T23:16:50.961-07:00   Tot Req 11179   Tot Rsp 9675    Out Req 1504    Cur Q/S 299     Cur R/S 299     Cur Lat 5011ms  Threads 8270
2019-05-13T23:16:51.963-07:00   Tot Req 11479   Tot Rsp 9975    Out Req 1504    Cur Q/S 299     Cur R/S 299     Cur Lat 5012ms  Threads 8270
2019-05-13T23:16:52.961-07:00   Tot Req 11779   Tot Rsp 10276   Out Req 1503    Cur Q/S 300     Cur R/S 301     Cur Lat 5028ms  Threads 8270
2019-05-13T23:16:53.962-07:00   Tot Req 12079   Tot Rsp 10576   Out Req 1503    Cur Q/S 299     Cur R/S 299     Cur Lat 5011ms  Threads 8270
```

* It took a while to stablize
* Still using > 8000 threads

## Results - Sync - Healthy
When we run 1000 requests per second with a synchronous but less performant client OkHttp, it turns out to be a lot more successful than sync over async with Netty.

```
> mvn compile -Dsoa=OKHTTP_SYNC -Drps=1000 -Dduration=30
[INFO] Scanning for projects...
[INFO]
[INFO] ---------------------< com.azure:sync-over-async >----------------------
[INFO] Building sync-over-async 1.0.0
[INFO] --------------------------------[ jar ]---------------------------------
[INFO]
[INFO] --- maven-resources-plugin:2.6:resources (default-resources) @ sync-over-async ---
[WARNING] Using platform encoding (Cp1252 actually) to copy filtered resources, i.e. build is platform dependent!
[INFO] Copying 0 resource
[INFO]
[INFO] --- maven-compiler-plugin:3.8.0:compile (default-compile) @ sync-over-async ---
[INFO] Nothing to compile - all classes are up to date
[INFO]
[INFO] --- exec-maven-plugin:1.6.0:exec (default) @ sync-over-async ---
Running config: uri: http://localhost:8080/5000, request/sec: 1000, syncOverAsync: OKHTTP_SYNC, duration: 30 sec
2019-05-14T11:32:03.716-07:00   Tot Req 1195    Tot Rsp 0       Out Req 1195    Cur Q/S 1000    Cur R/S 0       Cur Lat 0ms     Threads 1200
2019-05-14T11:32:04.693-07:00   Tot Req 2193    Tot Rsp 0       Out Req 2193    Cur Q/S 1000    Cur R/S 0       Cur Lat 0ms     Threads 2198
2019-05-14T11:32:05.693-07:00   Tot Req 3193    Tot Rsp 0       Out Req 3193    Cur Q/S 999     Cur R/S 0       Cur Lat 0ms     Threads 3198
2019-05-14T11:32:06.692-07:00   Tot Req 4193    Tot Rsp 0       Out Req 4193    Cur Q/S 1001    Cur R/S 0       Cur Lat 0ms     Threads 4198
2019-05-14T11:32:07.692-07:00   Tot Req 5191    Tot Rsp 0       Out Req 5191    Cur Q/S 998     Cur R/S 0       Cur Lat 0ms     Threads 5196
2019-05-14T11:32:08.834-07:00   Tot Req 6140    Tot Rsp 0       Out Req 6140    Cur Q/S 824     Cur R/S 0       Cur Lat 0ms     Threads 6144
2019-05-14T11:32:09.693-07:00   Tot Req 7192    Tot Rsp 2182    Out Req 5010    Cur Q/S 1239    Cur R/S 2570    Cur Lat 11172ms Threads 6587
2019-05-14T11:32:10.693-07:00   Tot Req 8193    Tot Rsp 3181    Out Req 5012    Cur Q/S 1001    Cur R/S 999     Cur Lat 5002ms  Threads 6587
2019-05-14T11:32:11.734-07:00   Tot Req 9143    Tot Rsp 4020    Out Req 5123    Cur Q/S 901     Cur R/S 796     Cur Lat 4434ms  Threads 6587
2019-05-14T11:32:12.819-07:00   Tot Req 10065   Tot Rsp 4077    Out Req 5988    Cur Q/S 847     Cur R/S 52      Cur Lat 337ms   Threads 6587
2019-05-14T11:32:13.693-07:00   Tot Req 11192   Tot Rsp 6023    Out Req 5169    Cur Q/S 1311    Cur R/S 2265    Cur Lat 9559ms  Threads 6587
2019-05-14T11:32:14.693-07:00   Tot Req 12193   Tot Rsp 7159    Out Req 5034    Cur Q/S 1001    Cur R/S 1136    Cur Lat 5821ms  Threads 6587
2019-05-14T11:32:15.693-07:00   Tot Req 13193   Tot Rsp 8181    Out Req 5012    Cur Q/S 1000    Cur R/S 1022    Cur Lat 5122ms  Threads 6587
2019-05-14T11:32:16.692-07:00   Tot Req 14193   Tot Rsp 9035    Out Req 5158    Cur Q/S 1001    Cur R/S 854     Cur Lat 4289ms  Threads 6587
2019-05-14T11:32:17.738-07:00   Tot Req 15126   Tot Rsp 9071    Out Req 6055    Cur Q/S 891     Cur R/S 34      Cur Lat 216ms   Threads 6587
2019-05-14T11:32:18.692-07:00   Tot Req 16193   Tot Rsp 11169   Out Req 5024    Cur Q/S 1118    Cur R/S 2199    Cur Lat 11044ms Threads 6587
2019-05-14T11:32:19.693-07:00   Tot Req 17193   Tot Rsp 12153   Out Req 5040    Cur Q/S 1000    Cur R/S 984     Cur Lat 4935ms  Threads 6587
2019-05-14T11:32:20.692-07:00   Tot Req 18193   Tot Rsp 13182   Out Req 5011    Cur Q/S 1000    Cur R/S 1029    Cur Lat 5158ms  Threads 6587
2019-05-14T11:32:21.693-07:00   Tot Req 19185   Tot Rsp 14130   Out Req 5055    Cur Q/S 990     Cur R/S 946     Cur Lat 4792ms  Threads 6587
2019-05-14T11:32:22.697-07:00   Tot Req 20198   Tot Rsp 14813   Out Req 5385    Cur Q/S 1008    Cur R/S 680     Cur Lat 3276ms  Threads 6587
2019-05-14T11:32:23.695-07:00   Tot Req 21196   Tot Rsp 16172   Out Req 5024    Cur Q/S 1001    Cur R/S 1363    Cur Lat 6835ms  Threads 6587
2019-05-14T11:32:24.693-07:00   Tot Req 22194   Tot Rsp 17576   Out Req 4618    Cur Q/S 1000    Cur R/S 1406    Cur Lat 6709ms  Threads 6587
2019-05-14T11:32:25.693-07:00   Tot Req 23193   Tot Rsp 18680   Out Req 4513    Cur Q/S 1000    Cur R/S 1105    Cur Lat 4760ms  Threads 6587
2019-05-14T11:32:26.693-07:00   Tot Req 24194   Tot Rsp 19478   Out Req 4716    Cur Q/S 1000    Cur R/S 797     Cur Lat 3648ms  Threads 6587
2019-05-14T11:32:27.693-07:00   Tot Req 25193   Tot Rsp 20180   Out Req 5013    Cur Q/S 999     Cur R/S 702     Cur Lat 2891ms  Threads 6587
2019-05-14T11:32:28.693-07:00   Tot Req 26193   Tot Rsp 21646   Out Req 4547    Cur Q/S 1001    Cur R/S 1467    Cur Lat 6840ms  Threads 6587
2019-05-14T11:32:29.693-07:00   Tot Req 27193   Tot Rsp 23264   Out Req 3929    Cur Q/S 1000    Cur R/S 1618    Cur Lat 7111ms  Threads 6587
2019-05-14T11:32:30.693-07:00   Tot Req 28192   Tot Rsp 24460   Out Req 3732    Cur Q/S 999     Cur R/S 1196    Cur Lat 4675ms  Threads 6587
2019-05-14T11:32:31.696-07:00   Tot Req 29196   Tot Rsp 25226   Out Req 3970    Cur Q/S 1000    Cur R/S 762     Cur Lat 3388ms  Threads 6587
2019-05-14T11:32:32.697-07:00   Tot Req 30187   Tot Rsp 25899   Out Req 4288    Cur Q/S 990     Cur R/S 672     Cur Lat 2589ms  Threads 6587
```

## Results - Sync - Unhealthy
But the limitation of the sync client shows when requests per second goes higher.

```
mvn compile -Dsoa=OKHTTP_SYNC -Drps=1500 -Dduration=30
[INFO] Scanning for projects...
[INFO]
[INFO] ---------------------< com.azure:sync-over-async >----------------------
[INFO] Building sync-over-async 1.0.0
[INFO] --------------------------------[ jar ]---------------------------------
[INFO]
[INFO] --- maven-resources-plugin:2.6:resources (default-resources) @ sync-over-async ---
[WARNING] Using platform encoding (Cp1252 actually) to copy filtered resources, i.e. build is platform dependent!
[INFO] Copying 0 resource
[INFO]
[INFO] --- maven-compiler-plugin:3.8.0:compile (default-compile) @ sync-over-async ---
[INFO] Nothing to compile - all classes are up to date
[INFO]
[INFO] --- exec-maven-plugin:1.6.0:exec (default) @ sync-over-async ---
Running config: uri: http://localhost:8080/5000, request/sec: 1500, syncOverAsync: OKHTTP_SYNC, duration: 30 sec
2019-05-14T11:33:14.034-07:00   Tot Req 1515    Tot Rsp 0       Out Req 1515    Cur Q/S 1262    Cur R/S 0       Cur Lat 0ms     Threads 1519
2019-05-14T11:33:14.995-07:00   Tot Req 3283    Tot Rsp 0       Out Req 3283    Cur Q/S 1787    Cur R/S 0       Cur Lat 0ms     Threads 3288
2019-05-14T11:33:16.003-07:00   Tot Req 4782    Tot Rsp 0       Out Req 4782    Cur Q/S 1487    Cur R/S 0       Cur Lat 0ms     Threads 4787
2019-05-14T11:33:17.011-07:00   Tot Req 5602    Tot Rsp 0       Out Req 5602    Cur Q/S 809     Cur R/S 0       Cur Lat 0ms     Threads 5607
2019-05-14T11:33:18.272-07:00   Tot Req 7546    Tot Rsp 5       Out Req 7541    Cur Q/S 1546    Cur R/S 3       Cur Lat 5ms     Threads 7545
2019-05-14T11:33:19.597-07:00   Tot Req 8648    Tot Rsp 18      Out Req 8630    Cur Q/S 828     Cur R/S 9       Cur Lat 40ms    Threads 8635
2019-05-14T11:33:20.035-07:00   Tot Req 10442   Tot Rsp 21      Out Req 10421   Cur Q/S 4105    Cur R/S 6       Cur Lat 8ms     Threads 10429
2019-05-14T11:33:21.438-07:00   Tot Req 10594   Tot Rsp 85      Out Req 10509   Cur Q/S 108     Cur R/S 45      Cur Lat 2752ms  Threads 10514
2019-05-14T11:33:22.008-07:00   Tot Req 12107   Tot Rsp 147     Out Req 11960   Cur Q/S 2668    Cur R/S 109     Cur Lat 308ms   Threads 11965
2019-05-14T11:33:22.996-07:00   Tot Req 15278   Tot Rsp 3154    Out Req 12124   Cur Q/S 3209    Cur R/S 3043    Cur Lat 7968ms  Threads 13807
2019-05-14T11:33:25.469-07:00   Tot Req 16546   Tot Rsp 3206    Out Req 13340   Cur Q/S 512     Cur R/S 21      Cur Lat 318ms   Threads 13807
2019-05-14T11:33:25.470-07:00   Tot Req 16546   Tot Rsp 3206    Out Req 13340   Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 13807
2019-05-14T11:33:26-07:00       Tot Req 19690   Tot Rsp 3307    Out Req 16383   Cur Q/S 5887    Cur R/S 189     Cur Lat 324ms   Threads 16389
2019-05-14T11:33:27.002-07:00   Tot Req 20493   Tot Rsp 3345    Out Req 17148   Cur Q/S 805     Cur R/S 38      Cur Lat 469ms   Threads 17158
2019-05-14T11:33:27.997-07:00   Tot Req 22784   Tot Rsp 3692    Out Req 19092   Cur Q/S 2300    Cur R/S 348     Cur Lat 907ms   Threads 19097
2019-05-14T11:33:28.995-07:00   Tot Req 24271   Tot Rsp 3732    Out Req 20539   Cur Q/S 1489    Cur R/S 40      Cur Lat 223ms   Threads 20544
2019-05-14T11:33:30.029-07:00   Tot Req 25597   Tot Rsp 3766    Out Req 21831   Cur Q/S 1281    Cur R/S 32      Cur Lat 336ms   Threads 21836
2019-05-14T11:33:31.002-07:00   Tot Req 27281   Tot Rsp 3774    Out Req 23507   Cur Q/S 1732    Cur R/S 8       Cur Lat 50ms    Threads 23512
2019-05-14T11:33:32.001-07:00   Tot Req 28788   Tot Rsp 3784    Out Req 25004   Cur Q/S 1508    Cur R/S 10      Cur Lat 105ms   Threads 25010
2019-05-14T11:33:32.995-07:00   Tot Req 30284   Tot Rsp 3858    Out Req 26426   Cur Q/S 1505    Cur R/S 74      Cur Lat 563ms   Threads 26431
2019-05-14T11:33:33.995-07:00   Tot Req 31781   Tot Rsp 3947    Out Req 27834   Cur Q/S 1497    Cur R/S 89      Cur Lat 711ms   Threads 27839
2019-05-14T11:33:34.999-07:00   Tot Req 33288   Tot Rsp 3967    Out Req 29321   Cur Q/S 1500    Cur R/S 19      Cur Lat 234ms   Threads 29326
2019-05-14T11:33:36.028-07:00   Tot Req 34781   Tot Rsp 3978    Out Req 30803   Cur Q/S 1449    Cur R/S 10      Cur Lat 137ms   Threads 30808
2019-05-14T11:33:37.002-07:00   Tot Req 36132   Tot Rsp 4195    Out Req 31937   Cur Q/S 1388    Cur R/S 223     Cur Lat 3247ms  Threads 31941
2019-05-14T11:33:38.146-07:00   Tot Req 37782   Tot Rsp 4288    Out Req 33494   Cur Q/S 1437    Cur R/S 81      Cur Lat 1041ms  Threads 33500
2019-05-14T11:33:40.537-07:00   Tot Req 39122   Tot Rsp 4303    Out Req 34819   Cur Q/S 559     Cur R/S 6       Cur Lat 191ms   Threads 34824
2019-05-14T11:33:40.537-07:00   Tot Req 39122   Tot Rsp 4303    Out Req 34819   Cur Q/S N/A     Cur R/S N/A     Cur Lat N/Ams   Threads 34824
2019-05-14T11:33:41.013-07:00   Tot Req 40840   Tot Rsp 4314    Out Req 36526   Cur Q/S 3647    Cur R/S 23      Cur Lat 122ms   Threads 36531
2019-05-14T11:33:42.012-07:00   Tot Req 42175   Tot Rsp 4325    Out Req 37850   Cur Q/S 1335    Cur R/S 11      Cur Lat 166ms   Threads 37855
2019-05-14T11:33:43.007-07:00   Tot Req 44237   Tot Rsp 4340    Out Req 39897   Cur Q/S 2070    Cur R/S 15      Cur Lat 181ms   Threads 39903
```