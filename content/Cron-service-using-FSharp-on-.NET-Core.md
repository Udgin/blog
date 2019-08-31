+++
title = "Cron service using F# on .NET Core"
date = 2017-05-28
[taxonomies]
tags = ["dotnet","fsharp"]
categories = ["programming"]
+++
As continuation for [my previous post](https://eapyl.github.io/article/Daemon-cron-using-F-19-May-2017.html) I want to create a nancy service to run my cron jobs.

The source is [here](https://github.com/eapyl/fsharp-nancy-service).

A service to run jobs:
```fsharp
module Service =
    let start (logger:ILogger) (items:Item[]) =
        let version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
        logger.LogInformation("Staring service {version}", version)
        let itemCount = Array.length items
        logger.LogInformation("Item count is {Length}", itemCount)
        let proceedItem item =
            async {
                logger.LogTrace("ExecuteForItem {ip}", item.id)
            }
        let jobs = items |> Array.map (fun item ->
            {
                action = proceedItem item;
                cron = item.cron
            })
        let daemon = run jobs
        logger.LogInformation("Started service")
        daemon
```

It contains only method `start` to create cron daemon.

All other classes is related to Nancy platform and it is easy to write using [nancy documentation](https://github.com/NancyFx/Nancy/wiki/Documentation).