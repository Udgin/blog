## How to set up SQL code coverage - 15 December, 2017
Tags: mssql, code coverage, sql, .net

If you are creating a lot of SQL code, it is a good idea to do unit testing for it and see [code coverage](https://en.wikipedia.org/wiki/Code_coverage). Luckly there is two nice tools/libs to do it: [tSQLt](http://tsqlt.org/) and [SQLCover](https://github.com/GoEddie/SQLCover).

In this article I will show how to create/set up SQLCover and run it for each CI build.
The basic idea is we are creating command line application which runs our tSQLt unit tests via SQLCover and generates HTML report with code coverage information.

Unfortunately there is no SQLCover package in NuGet, but we can download it from [here](http://the.agilesql.club/SQLCover/download.php).

Create a new command line application, add references to SQLCover.dll and Microsoft.SqlServer.TransactSql.ScriptDom.dll.
After that there is an example of Program.cs:
```
    private const string RunAllSqlTestsCommand = "exec tSQLt.RunAll;";
    private const string DefaultDatabaseName = "MyDB";

    private const string ReportName = "SQLCoverage.html";

    static void Main(string[] args)
    {
        var connectionString = ConfigurationManager.ConnectionStrings[DefaultDatabaseName].ConnectionString;
        var coverage = new CodeCoverage(connectionString, DefaultDatabaseName);
        var result = coverage.Cover(RunAllSqlTestsCommand);
        var updatedResult =  result.Html();
        File.WriteAllText(ReportName, updatedResult);
    }
```

After execution you will see SQLCoverage.html with code coverage statistics in the same folder.

One note from SQLCover owner: "When we target local sql instances we delete the trace files but when targetting remote instances we are unable to delete the files as we do not (or potentially) do not have access. If this is the case keep an eye on the log directory and remove old "SQLCover-Trace-.xel" and "SQLCover-Trace-.xem" files."

Thanks.

---
## Simplest syslog server for rfc5424 (TCP) - 12 September, 2017
Tags: docker, dotnet, syslog, logspout, serilog

I am working on onside project where we have more than 5 docker containers. Previously we used just serilog to do our logging in all our containers. So each project had his own logging mechanism. Finally we decided to centralize our logging system ans start using docker logs. There are a lot of solutions using clouds (e.g. [loggly](https://www.loggly.com/)) or quite complex systems to grab, analyze and show your logs (like [kafka](https://kafka.apache.org), [kibana](https://www.elastic.co/products/kibana)). But we wanted to save our logs to simple txt files without any additional complex stuff.

There are nice project which allows automatically read docker container logs and push them to one syslog server - [logspout](https://github.com/gliderlabs/logspout).

We tried to find simple syslog server without success. So there is a [small syslog server](https://github.com/eapyl/syslog-collector) written in dotnet core. I used serilog to write logs to files ([Rolling File](https://github.com/serilog/serilog-sinks-rollingfile)).

So docker-compose configuration looks like:
```
  #another containers
  logspout:
    image: gliderlabs/logspout
    command: syslog+tcp://logcollector:5000
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    depends_on:
      - logcollector
  logcollector:
    image: eapyl/syslog-collector
    volumes: 
      - $PWD/../logs:/log/logs
```

After that logs from all our containers are in logs folder! Very simple and easy!

Please find docker image [here](https://hub.docker.com/r/eapyl/syslog-collector/).

P.S. We started to write log message to stdout or stderr using [serilog](https://github.com/serilog/serilog-sinks-literate). So it was just small configuration change in all our projects to use Serilog.Literate instead of Serilog.RollingFile .

Thanks!

---
## Restore InfluxDB in Docker - 7 August, 2017
Tags: docker, influxdb, docker

It is not easy to restore influxdb in an official container. Unfortunately it is not possible directly. You need to restore db out of a container and mount restored db to the container.

```
# Restoring a backup requires that influxd is stopped (note that stopping the process kills the container).
docker stop "$CONTAINER_ID"

# Run the restore command in an ephemeral container.
# This affects the previously mounted volume mapped to /var/lib/influxdb.
docker run --rm \
--entrypoint /bin/bash \
-v "$INFLUXDIR":/var/lib/influxdb \
-v "$BACKUPDIR":/backups \
influxdb:1.3 \
-c "influxd restore -metadir /var/lib/influxdb/meta -datadir /var/lib/influxdb/data -database foo /backups/foo.backup"

# Start the container just like before, and get the new container ID.
CONTAINER_ID=$(docker run --rm \
--detach \
-v "$INFLUXDIR":/var/lib/influxdb \
-v "$BACKUPDIR":/backups \
-p 8086 \
influxdb:1.3
)
```

More info [here](https://gist.github.com/mark-rushakoff/36b4491f97b8781198da36752ecd949b).

Thanks.

---
## Personal mobile radio application using Ionic - 24 July, 2017
Tags: ionic, android, angular, ubuntu

Hi, there!

Finally I am tired because of all advertisements which we have in any simple radio mobile application. I just want to listen radio and that is all! Quite simple, hah?

So I decided to create a simple radio mobile application by myself. I thought about Xamarin as I have some experience in creating application using it. But I decided to use new framework for me and it is Ionic. I am very impressed how cool it is and how easy to create application using it. It took about 2 hours to install all dependencies (ionic, android studio, java), deploy and run a first version of my application on my android device. Quite fast!

I am using Ubuntu 16.04.

Please find the steps below:  
1. I have installed ionic framework using this [link](http://ionicframework.com/docs/intro/installation/) and create blank application: `ionic start radioOn blank`.
2. And that is all, to start our application need to run `ionic serve`.
3. As I want to listen an online radio, I need to create a provider to listen online stream: Html5Audio:

```typescript
import { Injectable, Output, EventEmitter } from '@angular/core';

@Injectable()
export class Html5Audio {
    @Output()
    ended = new EventEmitter();

    private pad2(number) { (number < 10 ? '0' : '') + number; }

    audioPlayer: any;
    time: string;
    @Output()
    isPlaying = false;
    readyStateInterval = null;
    url: string;

    public play(url: string) {
        if (this.audioPlayer) {
            this.stop();
        }
        this.url = url;
        this.audioPlayer = new Audio(this.url);
        this.isPlaying = true;
        this.audioPlayer.play();

        this.audioPlayer.addEventListener("timeupdate", () => {
            if (this.audioPlayer) {
                var s = this.audioPlayer.currentTime % 60;
                var m = (this.audioPlayer.currentTime / 60) % 60;
                var h = ((this.audioPlayer.currentTime / 60) / 60) % 60;
                if (this.isPlaying && this.audioPlayer.currentTime > 0) {
                    this.time = this.pad2(h) + ':' + this.pad2(m) + ':' + this.pad2(s);
                }
            }
        }, false);
        this.audioPlayer.addEventListener("error", (ex) => {
            console.error(ex);
        }, false);
        this.audioPlayer.addEventListener("canplay", () => {
            console.log('CAN PLAY');
        }, false);
        this.audioPlayer.addEventListener("waiting", () => {
            this.isPlaying = false;
        }, false);
        this.audioPlayer.addEventListener("playing", () => {
            this.isPlaying = true;
        }, false);
        this.audioPlayer.addEventListener("ended", () => {
            this.stop();
            this.ended.emit();
        }, false);
    }

    pause() {
        this.isPlaying = false;
        this.audioPlayer.pause();
    }

    stop() {
        this.isPlaying = false;
        this.audioPlayer.pause();
        this.audioPlayer = null;
    }
}
```
4. It has been imported to /src/app/app.module.
5. There is only one page with a list of stations (I have [only 3](https://github.com/eapyl/radioon/blob/master/src/pages/home/home.ts) which I listen). Click on a station - starting this station, and there is one button to stop music([link](https://github.com/eapyl/radioon/blob/master/src/pages/home/home.html)). That is all. Simple!
6. After that we need to prepare icon and splash image for application. I am using [game-icons](http://game-icons.net) to create an icon and [unspalsh](https://unsplash.com/) to find full size images.
7. Need to put created icon.png and splash.png to /resources/icon.png and /resources/splash.png and run the next command `ionic cordova resources` (you have to have an account in Ionic portal and it is free).
8. To [deploy](http://ionicframework.com/docs/intro/deploying/) our application need to run `ionic cordova build android --prod --release` and it will create *.apk file. We need to sign it using 'Sign Android APK' section from [here](http://ionicframework.com/docs/intro/deploying/).
9. That is all. We have signed apk which we can copy on our device and install.

Please find my project [here](https://github.com/eapyl/radioon). Signed apk file [here](https://mega.nz/#!edsTXT6L).

How it looks like:  
![image](./../images/radio-on-screenshot.png)

Cheers,

---
## Cake.Net. Useful scripts for AngularJS, dotnet core, docker application - 22 June, 2017
Tags: dotnet, angular, docker, cake.net, git

There is a great [cross-platform build automation tool](http://cakebuild.net/) - Cake.Net.

I started using it and created some useful scripts to Release and Publish new version of application.

I am using AngularJS as frontend framework, asp net core (dotnet core) as backend framework, git as source system and docker to deploy and run application on remote server.

So it is a small cake script to release a new version of application:

```cake
#addin "Cake.Docker"
#addin "Cake.FileHelpers"

// Release: ./build.sh -t Release "-packageVersion=x.x.x.x"
// Publish: ./build.sh -t Publish "-packageVersion=x.x.x.x" "-env=one"
// Delete release: ./build.sh -t Delete "-packageVersion=x.x.x.x"
var target = Argument<string>("target");
var version = Argument<string>("packageVersion");

public class Settings
{
    public string SshAddress;
    public string SshPort;
    public string HomePath;
}

Settings sets;

Task("SelectEnvironment")
    .Does(()=>{
        var env = Argument<string>("env");
        var environments = new Dictionary<string, Settings> {
            ["one"] = new Settings {
                SshAddress = "root@66.66.66.66",
                SshPort = "22",
                HomePath = "/root/Templates"
            },
            ["two"] = new Settings{
                SshAddress = "root@99.99.99.99",
                SshPort = "26",
                HomePath = "/home/Templates"
            }
        };
        sets = environments[env];
});

Task("Publish")
    .IsDependentOn("SelectEnvironment")
    .IsDependentOn("CheckAllCommitted")
    .IsDependentOn("CheckoutTag")
    .IsDependentOn("BuildAngular")
    .IsDependentOn("BuildDocker")
    .IsDependentOn("CheckoutBranch")
    .IsDependentOn("PushToGitLab")
    .IsDependentOn("PublishService")
    .Does(()=>
    {
        Information("Finished!");
    });

Task("Release")
    .IsDependentOn("SetVersion")
    .IsDependentOn("ReleaseNotes")
    .IsDependentOn("Commit")
    .IsDependentOn("Build")
    .IsDependentOn("PushTagToGit")
    .Does(()=>
    {
        Information("Finished!");
    });

//build angular
Task("Build Angular")
    .Does(() =>
    {
        StartProcess("ng", "build");
    });

// delete release (tag)
Task("Delete")
    .Does(()=>
    {
        StartProcess("git", $"tag --delete v{version}");
        StartProcess("git", $"push --delete origin v{version}");
    });

// add release notes to historys
Task("ReleaseNotes")
    .Does(()=> {
        IEnumerable<string> redirectedOutput;
        StartProcess("git", 
            new ProcessSettings {
                Arguments = $"describe --abbrev=0 --tags",
                RedirectStandardOutput = true
            },
            out redirectedOutput
        );
        var previousVersion = redirectedOutput.First();
        Information(previousVersion);
        IEnumerable<string> redirectedOutput2;
        StartProcess("git", 
            new ProcessSettings {
                Arguments = $"log --pretty=\"%s\" HEAD...{previousVersion}",
                RedirectStandardOutput = true
            },
            out redirectedOutput2
        );
        FileAppendLines("./src/HISTORY.txt", new string[]{$"------release v{version}-----"});
        FileAppendLines("./src/HISTORY.txt", redirectedOutput2.ToArray());
    });

// push docker image to gitlab
Task("PushToGitLab")
    .Does(()=>{
        StartProcess("docker", $"push registry.gitlab.com/counterra/service:{version}");
    });

// build project locally 
Task("Build")
    .Does(() =>
{
    DotNetCoreRestore("./src/counterra.csproj");
    CleanDirectory("./artifacts");
    var settings = new DotNetCorePublishSettings
    {
        Framework = "netcoreapp1.1",
        Configuration = "Release",
        OutputDirectory = "./artifacts/"
    };
    DotNetCorePublish("./src/counterra.csproj", settings);
});

// set new version in project file
Task("SetVersion")
    .Does(()=>
    {
        var file = File("./src/counterra.csproj");
        XmlPoke(file, "/Project/PropertyGroup/Version", version);
    });

// build docker
Task("BuildDocker")
    .Does(() =>
{
    var settings = new DockerBuildSettings {
        Tag = new []{ $"registry.gitlab.com/counterra/service:{version}" }
    };
    DockerBuild(settings, ".");
});

// go to remote server and replace docker there
Task("PublishService")
    .Does(() =>
{
    StartProcess("scp", $"-P {sets.SshPort} {sets.SshAddress}:{sets.HomePath}/server/docker-compose.yml ./artifacts/");
    ReplaceRegexInFiles("./artifacts/docker-compose.yml", @"registry.gitlab.com/counterra/service:\d+\.\d+\.\d+\.\d+", $"registry.gitlab.com/counterra/service:{version}");

    StartProcess("ssh", $"-p {sets.SshPort} {sets.SshAddress} cd {sets.HomePath}/server/; docker-compose stop service");
    StartProcess("ssh", $"-p {sets.SshPort} {sets.SshAddress} cd {sets.HomePath}/server/; docker-compose rm -f service");
    StartProcess("scp", $"-P {sets.SshPort} ./artifacts/docker-compose.yml {sets.SshAddress}:{sets.HomePath}/server/");
    StartProcess("ssh", $"-p {sets.SshPort} {sets.SshAddress} cd {sets.HomePath}/server/; docker-compose create service");
    StartProcess("ssh", $"-p {sets.SshPort} {sets.SshAddress} cd {sets.HomePath}/server/; docker-compose start service");
});

// push new tag
Task("PushTagToGit")
    .Does(() =>
{
    StartProcess("git", $"tag v{version}");
    StartProcess("git", "push --tags");
});

// checkout tag
Task("CheckoutTag")
    .Does(() =>
{
    StartProcess("git", $"checkout tags/v{version}");
});

// checkout master
Task("CheckoutBranch")
    .Does(() =>
{
    StartProcess("git", $"checkout master");
});

// commit all to branch and push it
Task("Commit")
    .Does(() =>
{
    StartProcess("git", $"add .");
    StartProcess("git", $"commit -m \"Release v{version}\" ");
    StartProcess("git", $"push");
});

// check that we haven't uncommitted files
Task("CheckAllCommitted")
    .Does(() =>
{
    IEnumerable<string> redirectedOutput;
    StartProcess("git", 
        new ProcessSettings {
            Arguments = "status",
            RedirectStandardOutput = true
        },
        out redirectedOutput
    );
    if (!redirectedOutput.LastOrDefault().Contains("nothing to commit"))
    {
        throw new Exception("Exists uncommitted changes");
    }
});

RunTarget(target);

```

It is a script to build released version and publish a docker container to a server:

```cake
// Release: ./build.sh -t Release "-packageVersion=x.x.x.x"
// Publish: ./build.sh -t Publish "-packageVersion=x.x.x.x" "-env=counterra"
// Delete release: ./build.sh -t Delete "-packageVersion=x.x.x.x"
var target = Argument<string>("target");
var version = Argument<string>("packageVersion");

public class Settings
{
    public string SshAddress;
    public string SshPort;
    public string HomePath;
}

Settings sets;

Task("SelectEnvironment")
    .Does(()=>{
        var env = Argument<string>("env");
        var environments = new Dictionary<string, Settings> {
            ["counterra"] = new Settings {
                SshAddress = "root@83.220.171.23",
                SshPort = "22",
                HomePath = "/root/Templates"
            },
            ["tj"] = new Settings{
                SshAddress = "vorsa@195.14.96.218",
                SshPort = "26",
                HomePath = "/home/vorsa"
            }
        };
        sets = environments[env];
});

Task("Publish")
    .IsDependentOn("SelectEnvironment")
    .IsDependentOn("CheckAllCommitted")
    .IsDependentOn("CheckoutTag")
    // build angular
    .IsDependentOn("Build Angular")
    .IsDependentOn("BuildDocker")
    .IsDependentOn("CheckoutBranch")
    .IsDependentOn("PushToGitLab")
    .IsDependentOn("PublishService")
    .Does(()=>
    {
        Information("Finished!");
    });

Task("Release")
    .IsDependentOn("SetVersion")
    .IsDependentOn("ReleaseNotes")
    .IsDependentOn("Commit")
    .IsDependentOn("Build")
    .IsDependentOn("PushTagToGit")
    .Does(()=>
    {
        Information("Finished!");
    });

Task("Publish")
    // check that current 'master' branch hasn't uncommitted changes
    .IsDependentOn("CheckAllCommitted")
    // checkout release tag
    .IsDependentOn("CheckoutTag")
    // build angular
    .IsDependentOn("Build Angular")
    // build release 
    .IsDependentOn("Build")
    // return 'master' to HEAD as we have published release in artifacts folder
    .IsDependentOn("CheckoutBranch")
    // create docker image using published release
    .IsDependentOn("BuildDocker")
    // copy image to remote server
    .IsDependentOn("ExportDocker")
    // import new image, replace running container
    .IsDependentOn("PublishService")
    .Does(()=>
    {
        Information("Finished!");
    });

Task("Build Angular")
    .Does(() =>
{
    StartProcess("ng", "build");
});

Task("Build")
    .Does(() =>
{
    DotNetCoreRestore($ "./src/{Settings.ProjectName}");
    CleanDirectory("./artifacts");
    var settings = new DotNetCorePublishSettings
    {
        Framework = "netcoreapp1.1",
        Configuration = "Release",
        OutputDirectory = "./artifacts/"
    };
    DotNetCorePublish($ "./src/{Settings.ProjectName}", settings);
    // clean up artifacts folder
    DeleteDirectory("./artifacts/e2e", recursive:true);
    DeleteDirectory("./artifacts/src", recursive:true);
    DeleteFiles("./artifacts/ts*.json");
});

Task("BuildDocker")
    .Does(() =>
{
    var settings = new DockerBuildSettings {
        Tag = new []{ $ "{Settings.ContainerName}:{version}" }
    };
    DockerBuild(settings, ".");
});

Task("ExportDocker")
    .Does(() =>
{
    var settings = new DockerSaveSettings {
        Output = $ "./artifacts/{Settings.ContainerName}.{version}.tar"
    };
    // save docker image to tar archive
    DockerSave(settings, new[] { $ "{Settings.ContainerName}:{version}"});
});

Task("PublishService")
    .Does(() =>
{
    // cope docker image to remote server
    StartProcess("scp", $ "-P {Settings.SshPort} ./artifacts/{Settings.ContainerName}.{version}.tar {Settings.SshAddress}:{Settings.HomePath}/");
    // load copied image to docker on remote server
    StartProcess("ssh", $ "-p {Settings.SshPort} {Settings.SshAddress} docker load < {Settings.HomePath}/{Settings.ContainerName}.{version}.tar");

    // copy docker-compose configuration from remote server to artifacts folder
    StartProcess("scp", $ "-P {Settings.SshPort} {Settings.SshAddress}:{Settings.HomePath}/docker-compose.yml ./artifacts/");
    // replace the version of docker image in docker-compose configuration
    ReplaceRegexInFiles("./artifacts/docker-compose.yml", $ "{Settings.ContainerName}:\\d+\\.\\d+\\.\\d+\\.\\d+", $ "{Settings.ContainerName}:{version}");

    // stop old docker container on server
    StartProcess("ssh", $ "-p {Settings.SshPort} {Settings.SshAddress} cd {Settings.HomePath}/; docker-compose stop {Settings.ContainerName}");
    // delete old container
    StartProcess("ssh", $ "-p {Settings.SshPort} {Settings.SshAddress} cd {Settings.HomePath}/; docker-compose rm -f {Settings.ContainerName}");
    // copy new docker-compose configuration to remote server
    StartProcess("scp", $ "-P {Settings.SshPort} ./artifacts/docker-compose.yml {Settings.SshAddress}:{Settings.HomePath}/");
    // recreate docker container, it will create a new version
    StartProcess("ssh", $ "-p {Settings.SshPort} {Settings.SshAddress} cd {Settings.HomePath}/; docker-compose create {Settings.ContainerName}");
    // start new docker container
    StartProcess("ssh", $ "-p {Settings.SshPort} {Settings.SshAddress} cd {Settings.HomePath}/; docker-compose start {Settings.ContainerName}");
});

Task("CheckoutTag")
    .Does(() =>
{
    StartProcess("git", $ "checkout tags/v{version}");
});

Task("CheckoutBranch")
    .Does(() =>
{
    StartProcess("git", $ "checkout master");
});

Task("CheckAllCommitted")
    .Does(() =>
{
    IEnumerable<string> redirectedOutput;
    var exitCodeWithArgument = StartProcess("git", new ProcessSettings{
    Arguments = "status",
    RedirectStandardOutput = true
    },
    out redirectedOutput
    );
    if (!redirectedOutput.LastOrDefault().Contains("nothing to commit"))
    {
        throw new Exception("Exists uncommitted changes");
    }
});

RunTarget(target);
```

I am running service using docker compose like:

```yml
version: '2'
services:
    ContainerName:
        image: ContainerName:x.x.x.x
```

Thanks.

---
## Cron service using F# on .NET Core - 28 May, 2017
Tags: dotnet, fsharp, cron

As continuation for [my previous post](https://eapyl.github.io/article/Daemon-cron-using-F-19-May-2017.html) I want to create a nancy service to run my cron jobs.

The source is [here](https://github.com/eapyl/fsharp-nancy-service).

A service to run jobs:
```fs
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

---
## Daemon cron using F# - 19 May, 2017
Tags: dotnet, fsharp

As continuation for [my previous post](https://eapyl.github.io/article/Cron-schedule-using-F-1-May-2017.html) I want to create a daemon which runs jobs using created cron code.

```fs
module Daemon =
    [<Literal>]
    let INTERVAL = 30000

    let internalRun interval (now: unit->DateTime) (jobs: seq<Job>) =
        // to dispose System.Threading.Timer properly
        let createDisposable f =
            {
                new IDisposable with
                    member x.Dispose() = f()
            }
        
        // event fot Timer
        let timerElapsed obj =
            let checkJob = () |> now |> Schedule.isTime
            jobs 
            |> Seq.map (fun x ->
                let schedule = Schedule.generate x.cron
                (schedule, x.action)
            ) 
            |> Seq.filter (fun (x, y) -> checkJob x)
            |> Seq.map (fun (x, y) -> y) 
            |> Async.Parallel 
            |> Async.RunSynchronously
            |> ignore
        
        // timer
        let localTimer = new Timer(timerElapsed, null, Timeout.Infinite, interval)
        // start timer
        localTimer.Change(0, interval) |> ignore
        // return timer as IDisposable 
        createDisposable (fun() -> localTimer.Dispose())

    // get DateTime
    let now = fun () -> DateTime.UtcNow
    // public method to call
    let run jobs = internalRun INTERVAL now jobs
```

How to use the daemon above:

```fs
type Job = { action: Async<unit>; cron: string }

let act id =
    async {
        printfn "Execution %A" id
    }
let jobs = [|1; 2|] |> Array.map (fun id ->
    {
        action = act id;
        cron = "* * * * *"
    })
let daemon = run jobs
```

Unfortunately I don't see the good way to wtite unit test for this code as there is hardcoded dependency to System.Threading.Timer.

Feel free to comment if you see the solution. Thanks in advance.

---
## Cron schedule using F# - 1 May, 2017
Tags: dotnet, fsharp

I decided to start learning fsharp and hope I will be able to use F# in my future small projects.

Currently I have a small service worked in Docker container. It is written using C# on dotnet core.
This service is for grabbing data from external services via HTTP and putting aggregated data to InfluxDB database. Internally the service is using cron to plan and run this process as we want to grab data periodically.

#### I started with rewriting cron scheduling.

So there is implementation in F# below:

Let's start with two helper objects: `String.split` and `TooMuchArgumentsException` exception.

```fs
type System.String with 
    static member split c (value: string) =
        value.Split c

exception TooMuchArgumentsException of int
```

`String.split` is just wrapper for `String.Split` method. Exception is needed to show when cron expression contains too much parts.

My internal cron supports the next template: 'minute hour dayOfMonth month dayOfWeek'. And the next type of cron expressions: 
1. '*' - wildcard;
2. '*/5' - every 5th, e.g. every five minutes;
3. '10-20/5' - range value, e.g. every from 10 till 20, e.g. every minute from 10 till 20. '/5' is optional and it works the same as previous one, so '10-20/5' for minutes means run at 10, 15 and 20;
4. '5' - one value only, e.g. only at 5th minute
5. '5,10,15,45' - list value, e.g. run at 5th, 10th, 15th and 45th minutes

```fs
open System
open System.Text.RegularExpressions

module Schedule =
    // regexp for */5
    [<Literal>]
    let DividePattern = @"(\*/\d+)"
    // regexp for range
    [<Literal>]
    let RangePattern = @"(\d+\-\d+(/\d+)?)"
    // regexp for wild char
    [<Literal>]
    let WildPattern = @"(\*)"
    // regexp for one value
    [<Literal>]
    let OneValuePattern = @"(\d)"
    // regexp for list value
    [<Literal>]
    let ListPattern = @"((\d+,)+\d+)"

    // internal record to parsed cron expression, so it contains minutes,
    // hours, days of month, months and day of week when we should run our jobs
    type ISchedueSet = 
        { 
            Minutes: int list;
            Hours: int list;
            DayOfMonth: int list;
            Months: int list;
            DayOfWeek: int list
        }
    
    // method to generate ISchedueSet record from cron expression
    let generate expression =
        // internal method to parse */5 
        let dividedArray (m:string) start max =
            let divisor = m |>  String.split [|'/'|] |> Array.skip 1 |> Array.head |> Int32.Parse
            [start .. max] |> List.filter (fun x -> x % divisor = 0)
        // internal method to parse range
        let rangeArray (m:string) =
            let split = m |> String.split [|'-'; '/'|] |> Array.map Int32.Parse
            match Array.length split with
                | 2 -> [split.[0] .. split.[1]]
                | 3 -> [split.[0] .. split.[1]] |> List.filter (fun x -> x % split.[2] = 0)
                | _ -> []
        // internal method to parse wild char
        let wildArray (m:string) start max =
            [start .. max]
        // internal method to parse single value
        let oneValue (m:string) =
            [m |> Int32.Parse]
        // internal method to parse list value
        let listArray (m:string) =
            m |> String.split [|','|] |> Array.map Int32.Parse |> Array.toList
        
        // we need to set minimum and maximum value for every part of date time
        let getStartAndMax i =
            match i with
            // for minutes
            | 0 -> (0, 59)
            // for hours
            | 1 -> (0, 23)
            // for days of month
            | 2 -> (1, 31)
            // for months
            | 3 -> (1, 12)
            // for day of week
            | 4 -> (0, 6)
            // throw an exception if don't know for what part of date time we need values
            | _ -> raise (TooMuchArgumentsException i)
        
        // active pattern to match regexp
        let (|MatchRegex|_|) pattern input =
            let m = Regex.Match(input, pattern)
            if m.Success then Some (m.ToString()) else None
        
        // parsing cron expression and create a array of lists which contains all possibles values
        // for every part of daytime
        let parts =
            expression 
            |> String.split [|' '|]
            |> Array.mapi (fun i x ->
                let (start, max) = getStartAndMax i 
                match x with
                    | MatchRegex DividePattern x -> dividedArray x start max
                    | MatchRegex RangePattern x -> rangeArray x
                    | MatchRegex WildPattern x -> wildArray x start max
                    | MatchRegex ListPattern x -> listArray x
                    | MatchRegex OneValuePattern x -> oneValue x
                    | _ -> []
            )
        // convert list of array to ISchedueSet
        { 
            Minutes = parts.[0];
            Hours = parts.[1];
            DayOfMonth = parts.[2];
            Months = parts.[3];
            DayOfWeek = parts.[4]
        }
    // method to check date time via generated ISchedueSet
    let isTime schedueSet (dateTime : DateTime) =
        List.exists ((=) dateTime.Minute) schedueSet.Minutes && 
        List.exists ((=) dateTime.Hour) schedueSet.Hours &&
        List.exists ((=) dateTime.Day) schedueSet.DayOfMonth &&
        List.exists ((=) dateTime.Month) schedueSet.Months &&
        List.exists ((=) (int dateTime.DayOfWeek)) schedueSet.DayOfWeek
```

So Schedule module contains two methods and one type: 
* ISchedueSet is a container for parsed cron expression;
* generate is to generate ISchedueSet record from cron expression. This record contains all possible values of minutes, hours, months, days of month, days of week for particular cron expression;
* isTime is to check if we need to run a job in passed date time 

Small sets of unit tests written using mstest:

```fs
namespace FsharpTest

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Cron
open Schedule

// minute hour dayOfMonth month dayOfWeek
[<TestClass>]
type ScheduleTests () =

    [<TestMethod>]
    member this.All () =
        let schedule = Schedule.generate "* * * * *"
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfMonth [1..31]);
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfWeek [0..6]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Hours [0..23]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Minutes [0..59]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Months [1..12]);

    [<TestMethod>]
    member this.Range () =
        let schedule = Schedule.generate "0-5 0-10/2 10-20/3 3-5 2-6/2"
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfMonth [12; 15; 18]);
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfWeek [2; 4; 6]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Hours [0; 2; 4; 6; 8; 10]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Minutes [0; 1; 2; 3; 4; 5]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Months [3; 4; 5]);

    [<TestMethod>]
    member this.OneTime () =
        let schedule = Schedule.generate "0 0 1 1 0"
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfMonth [1]);
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfWeek [0]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Hours [0]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Minutes [0]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Months [1]);

    [<TestMethod>]
    member this.Every () =
        let schedule = Schedule.generate "*/15 */3 */2 */5 *"
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfMonth [for i in [1 .. 31] do
                                                                if i%2 = 0 then
                                                                    yield i]
        );
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfWeek [0 .. 6]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Hours [0; 3; 6; 9; 12; 15; 18; 21]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Minutes [0; 15; 30; 45]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Months [5; 10]);

    [<TestMethod>]
    member this.OnlySome () =
        let schedule = Schedule.generate "1,2,3 5,6,7 28,29 1,2 5,6"
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfMonth [28;29]);
        Assert.IsTrue(List.forall2 ( = ) schedule.DayOfWeek [5;6]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Hours [5;6;7]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Minutes [1;2;3]);
        Assert.IsTrue(List.forall2 ( = ) schedule.Months [1;2]);

    [<TestMethod>]
    member this.IsTime () =
        let dateTime = DateTime(2000, 1, 1, 0, 0, 0)
        let schedule = { Minutes = [0]; Hours = [0]; DayOfWeek = [0 .. 6]; DayOfMonth = [1]; Months = [1]}
        Assert.IsTrue(Schedule.isTime schedule dateTime)
```

Quite simple implementation of cron schedule part of mentioned service.

Thanks.

---
## Backup InfluxDB in Docker - 15 April, 2017
Tags: influxdb, docker

If you want to backup your InfluxDB in Docker you can probably can start with this wonderful [article](https://devblog.digimondo.io/how-to-backup-influxdb-running-in-a-docker-container-615938dbab90?gi=78ccabcf77cf).

Later I will just describe how I am doing backup to local folder instead of S3 storage.

It is my docker-compose:

```dockerfile
    db:
        image: "influxdb:1.2"
        restart: always
        ports: 
            - "8086:8086"
        volumes:
            - /etc/influxdb:/var/lib/ #folder with db data
            - ./influxdb.conf:/etc/influxdb/influxdb.conf:ro #configuration for InfluxDB
    influxdb-backup:
        image: influxdb-backup:1.2
        container_name: influxdb-backup
        restart: always
        volumes:
            - /backup:/backup #folder with backups
        links:
            - db
        environment:
            INFLUX_HOST: db
            DATABASES: entity_db
```

So there are two containers: influxdb is the standard [InfluxDB](https://docs.influxdata.com/influxdb/v1.2/introduction/getting_started/) and influxdb-backup:1.2 (see dockerfile for this image below).

```dockerfile
FROM influxdb:1.2-alpine

# Backup the following databases, separator ":"
ENV DATABASES=entity_db
ENV INFLUX_HOST=influxdb

# Some tiny helpers 
RUN apk update && apk add ca-certificates && update-ca-certificates && apk add openssl
RUN apk add --no-cache bash py2-pip py-setuptools ca-certificates
RUN pip install python-magic

# Backup script
COPY backup.sh /bin/backup.sh
RUN chmod +x /bin/backup.sh

# Setup crontab
COPY cron.conf /var/spool/cron/crontabs/root

# Run Cron in foreground
CMD crond -l 0 -f
```

So we are running cron job to run our `backup.sh`. The configuration (cron.conf):
```bash
# do daily/weekly/monthly maintenance
# min	hour	day	month	weekday	command
0 0 * * * /bin/backup.sh
# run every day at 00:00:00
```

There is backup.sh with the next content:
```bash
#!/bin/bash
set -e

: ${INFLUX_HOST:?"INFLUX_HOST env variable is required"}

dir=/backup
min_dirs=16
#we are saving only last 14 backups (2 weeks)

if [ $(find "$dir" -maxdepth 1 -type d | wc -l) -ge $min_dirs ]
  then find "$dir" -maxdepth 1 | sort | head -n 2 | sort -r | head -n 1 | xargs rm -rf
fi

#all backups are in /backup folder
#every backup is in a folder with name which is date when a backup has been created
DATE=`date +%Y-%m-%d-%H-%M-%S`

echo 'Backup Influx metadata'
influxd backup -host $INFLUX_HOST:8088 /backup/$DATE

# Replace colons with spaces to create list.
for db in ${DATABASES//:/ }; do
  echo "Creating backup for $db"
  influxd backup -database $db -host $INFLUX_HOST:8088 /backup/$DATE
done
```

Please find an image [here](./../assets/inluxDbBackupDocker.zip).

Thank you!

---
## Giving full read+write permissions to a folder by all users and apps - 10 April, 2017
Tags: linux, ubuntu

Allows delete, write and edit all files in a particular folder:
```bash
chmod -R 777 <desired folder>
```

---
## Simple question to check understanding of multithreading - 5 March, 2017
Tags: interview, dotnet

How to do planning of two threads so none of them can leave a circle (X is global value and default value is 0)?

```csharp
while (X == 0)
{
    X = 1 - X;
}
```

---
## How to create Discord bot - 18 February, 2017
Tags: discord, javascript

That is a manual how to create simple ping-pong discord bot using javascript (nodejs).

* Install nodejs and npm
* Create a folder for your discord bot
* Run `npm init` in the folder
* Install [Discrod.js](https://discord.js.org/#/): `npm install --save discordjs`
* Create index.js in this folder:

```js
/*
  A ping pong bot, whenever you send "ping", it replies "pong".
*/

// import the discord.js module
const Discord = require('discord.js');

// create an instance of a Discord Client, and call it bot
const bot = new Discord.Client();

// the token of your bot - https://discordapp.com/developers/applications/me
const token = 'your bot token here';

// the ready event is vital, it means that your bot will only start reacting to information
// from Discord _after_ ready is emitted.
bot.on('ready', () => {
  console.log('I am ready!');
});

// create an event listener for messages
bot.on('message', message => {
  // if the message is "ping",
  if (message.content === 'ping') {
    // send "pong" to the same channel.
    message.channel.sendMessage('pong');
  }
});

// log our bot in
bot.login(token);
``` 
* Got to https://discordapp.com/developers/applications/me and create your bot
* Put all needed values, click "Create Application". On the next page scroll down until you see "Create a bot user", click that.
* After that you will be able to copy a token of your bot. Copy it and post it in created index.js
* Go to https://discordapp.com/oauth2/authorize?&client_id=YOUR_CLIENT_ID_HERE&scope=bot&permissions=0. You should replace YOUR_CLIENT_ID_HERE with Client ID (it should be in App Details on the web page where you got your token).
* Add your bot to the server.
* Run created bot by command `node index.js`
* You should be able to see a bot in Discord at your server. Write 'ping' to the bot. The answer should be 'pong'.

Well done! You created your Discord bot! Thank you.

Here is a simple rss bot for discord:

```js
const Discord = require('discord.js');
var Store = require("jfs"); // using jfs to save already posted rss news
var db = new Store("rssfeeds");
const client = new Discord.Client();

var currentNews = [];
var postedNews = [];
var interval;

function log(message) {
    console.log(new Date() + ": " + message);
}

/// load all posted rss news
db.all(function (err, objs) {
    if (err) log(err);
    for (var id in objs) {
        postedNews.push({ id: id, value: objs[id] });
        log("Restored posted news " + id);
    }
    loadFeeds();
});

client.on('ready', () => {
    log('I am ready!');
    var generalChannel = client.channels.get("25466045464298784169786");
    if (!interval) {
        interval = setInterval(() => {
            if (currentNews.length > 0) {
                var newsToPost = currentNews.shift();
                generalChannel.sendMessage(newsToPost.title + " - " + newsToPost.link);
                db.save(newsToPost, function (err, id) {
                    if (err) log(err);
                    log("Saved posted news " + id);
                    postedNews.push({ id: id, value: newsToPost });
                });

                log("Post " + newsToPost.link);
                log("Left in array - " + currentNews.length);
            }
        }, 60000 * 30);
    }
});

setInterval(() => {
    log("Updating news");
    loadFeeds();
}, 60000 * 60 * 24);

setInterval(() => {
    while (postedNews.length > 1000) {
        var oldNewsToDelete = postedNews.shift();
        log("deleting " + oldNewsToDelete.id)
        db.delete(oldNewsToDelete.id, function (err) {
            if (err) log(err);
        });
    }
}, 60000 * 60 * 24);

client.login('MjgyNDg5MjQ0MDEyNzczMzc2.C4nUkw.Na6H7ZVrXbMZbXv4Wt9p8cZaj2Q');

// rss.json should contain information about rss, like:
//{
//    "bbc": {
//        "description" : "bbc news",
//        "url" : "https://bbs.com/rss"
//    }
//}
var rssFeeds = require("./rss.json");

/// load feeds from Resources
function loadFeeds() {
    for (var feedName in rssFeeds) {
        rssfeed(rssFeeds[feedName].url);
    }
}

function rssfeed(url) {
    var FeedParser = require('feedparser');
    var feedparser = new FeedParser();
    var request = require('request');
    request(url).pipe(feedparser);
    feedparser.on('error', function (error) {
        log(error);
    });
    feedparser.on('readable', function () {
        var stream = this;
        var meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
        var item;

        while (item = stream.read()) {
            if (postedNews && postedNews.some(x => x.value.title == item.title)) continue;
            if (currentNews.some(x => x.title == item.title)) continue;
            log('Add news to current news array ' + item.link);
            currentNews.push({
                title: item.title,
                link: item.link,
                date: item.date
            });
            if (currentNews.length > 100) currentNews.shift();
        }
    });
}
```

Links:
* [Creating a discord bot & getting a token](https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token)
* [Discord.js](https://discord.js.org/#/)
* [Ping-Pong example](https://discord.js.org/#/docs/main/stable/examples/ping)
* [Discord Developers](https://discordapp.com/developers/applications/me)
* [Discord](https://discordapp.com/)

---
## Nancy sample application: NetCore + Serilog - 12 February, 2017
Tags: netcore, nancy, serilog, fakeiteasy

Hi there, 

Just trying to create a simple service running on server. This service should report his status by HTTP and should have a nice log mechanism. So I finished my investigation to use [NetCore](https://www.microsoft.com/net/core) + [Nancy](http://nancyfx.org/) + [Serilog](https://serilog.net/) + [Newtonsoft.Json](http://www.newtonsoft.com/json) + [FakeItEasy](https://github.com/FakeItEasy/FakeItEasy) + [App Metrics](https://github.com/alhardy/AppMetrics).

Please find the source code of sample application [here](https://github.com/eapyl/nancy-netcore-sample).

Want to mention that I had to install the next lib:

```bash
sudo apt-get install libunwind8
```

to actually run my service on remote server - Ubuntu 16.04 x64. There was the next error: `Failed to load libcoreclr.so libunwind.so.8 cannot open shared object file”` without this library.

Also project.json is set up to build self-contained application:

```bash
dotnet publish -c Release -r ubuntu.16.04-x64 -o packages/ubuntu
```

Thanks.

---
## Update pip application in Ubuntu - 21 January, 2017
Tags: pip, ubuntu, streamlink

To update application installed by pip, e.g. [streamlink](https://github.com/streamlink/streamlink):

```bash
sudo pip install -U streamlink
```

---
## Understanding Docker - 21 January, 2017
Tags: docker

### [What is Docker?](https://www.docker.com/what-docker)

Docker is the world's leading software containerization platform

> Note: Docker is licensed under the open source [Apache 2.0](https://inteist.com/how-to-use-apache-2-0-in-commercial-products-explained-in-simple-terms/) license.

Docker containers wrap a piece of software in a complete filesystem that contains everything needed to run: code, runtime, system tools, system libraries – anything that can be installed on a server. This guarantees that the software will always run the same, regardless of its environment.

![image1](./../images/WhatIsDocker_1_kernal-2_1.png)

1. Lightweight

Containers running on a single machine share the same operating system kernel; they start instantly and use less RAM. Images are constructed from layered filesystems and share common files, making disk usage and image downloads much more efficient.

2. Open and supported by community

Docker containers are based on open standards, enabling containers to run on all major Linux distributions and on Microsoft Windows - and on top of any infrastructure. In addition, because Docker's [partnering](http://www.zdnet.com/article/what-is-docker-and-why-is-it-so-darn-popular/) with the other container powers, including Canonical, Google, Red Hat, and Parallels, on its key open-source component [runc](https://github.com/opencontainers/runc), it's brought standardization to containers. More info is [here](https://www.opencontainers.org/).

3. Secure by default

Containers isolate applications from one another and the underlying infrastructure, while providing an added layer of protection for the application.

### Containers and virtual machines
Containers and virtual machines have similar resource isolation and allocation benefits - but a different architectural approach allows containers to be more portable and efficient. Docker is built on top of [LXC](https://en.wikipedia.org/wiki/LXC). Like with any container technology, as far as the program is concerned, it has its own file system, storage, CPU, RAM, and so on. The [key difference](http://www.zdnet.com/article/what-is-docker-and-why-is-it-so-darn-popular/) between containers and VMs is that while the hypervisor abstracts an entire device, containers just abstract the operating system kernel. It means that you are able to run only containers which are supported by host operating system, i.e. there are containers for Windows OS and containers for Linux.

* Virtual Machines

![image](./../images/WhatIsDocker_2_VMs_0-2_2.png)

Virtual machines include the application, the necessary binaries and libraries, and an entire guest operating system - all of which can amount to tens of GBs.

* Containers

![image](./../images/WhatIsDocker_3_Containers_2_0.png)

Containers include the application and all of its dependencies - but share the kernel with other containers, running as isolated processes in user space on the host operating system. Docker containers are not tied to any specific infrastructure: they run on any computer, on any infrastructure, and in any cloud.

### Additional benefits
Docker allows you to dynamically change your application - from adding new capabilities and scaling services, to quickly changing problem areas.

* Shorter delivery time

On average, Docker users ship 7X more software after deploying Docker in their environment. More frequent software updates provide added value to customers.

* Quickly scale

Docker containers spin up and down in seconds, making it easy to scale application services to satisfy peak customer demand, and then reduce running containers when demand ebbs. And Docker containers are easy to deploy in a cloud.

* Easily remediate issue

Docker makes it easy to identify issues, isolate the problem container, quickly roll back to make the necessary changes, and then push the updated container into production. Isolation between containers makes these changes less disruptive than in traditional software models.

In summary: docker allows to deploy, use and support containers on production easier and safer than previous approaches. On the other hand, developers can use Docker to pack, ship, and run any application as a lightweight, portable, self sufficient LXC container that can run virtually anywhere.

### [Docker toolset](https://docs.docker.com/engine/understanding-docker/#/what-is-the-docker-platform)

Docker provides tooling and a platform to manage the lifecycle of your containers:

* Encapsulate your applications (and supporting components) into Docker containers
* Distribute and ship those containers to your teams for further development and testing
* Deploy those applications to your production environment, whether it is in a local data center or the Cloud

### [Docker Engine](https://docs.docker.com/engine/understanding-docker/#/what-is-docker-engine)
Docker Engine is a client-server application with these major components:

* A server which is a type of long-running program called a daemon process.
* A REST API which specifies interfaces that programs can use to talk to the daemon and instruct it what to do.
* A command line interface (CLI) client.

![image](./../images/engine-components-flow.png)

### [What can I use Docker for?](https://docs.docker.com/engine/understanding-docker/#/what-can-i-use-docker-for)

Docker can streamline the development lifecycle by allowing developers to work in standardized environments using local containers which provide your applications and services. You can also integrate Docker into your continuous integration and continuous deployment (CI/CD) workflow.

Docker’s portability and lightweight nature also make it easy to dynamically manage workloads, scaling up or tearing down applications and services as business needs dictate, in near real time.

Docker is lightweight and fast. It provides a viable, cost-effective alternative to hypervisor-based virtual machines, allowing you to use more of your compute capacity to achieve your business goals.

### [Docker’s architecture](https://docs.docker.com/engine/understanding-docker/#/what-is-dockers-architecture)

Docker uses a client-server architecture. The Docker client and daemon communicate using a REST API, over UNIX sockets or a network interface. One client can even communicate with multiple unrelated daemons.

![image](./../images/architecture.svg)

### [Inside Docker](https://docs.docker.com/engine/understanding-docker/#/inside-docker)

* Docker images

A Docker image is a read-only template with instructions for creating a Docker container. For example, an image might contain an [Nano Server](https://technet.microsoft.com/en-us/windows-server-docs/get-started/getting-started-with-nano-server) operating system with dotnet core and your web application installed. You can build or update images from scratch or download and use images created by others. A docker image is described in text file called a Dockerfile, which has a simple, well-defined syntax.

#### Docker images are the build component of Docker.

* Docker containers

A Docker container is a runnable instance of a Docker image. You can run, start, stop, move, or delete a container using Docker API or CLI commands. When you run a container, you can provide configuration metadata such as networking information or environment variables. Each container is an isolated and secure application platform, but can be given access to resources running in a different host or container, as well as persistent storage or databases.

#### Docker containers are the run component of Docker.

* Docker registries

A docker registry is a library of images. A registry can be public or private, and can be on the same server as the Docker daemon or Docker client, or on a totally separate server.

#### Docker registries are the distribution component of Docker.

* Docker services

A Docker service allows a swarm of Docker nodes to work together, running a defined number of instances of a replica task, which is itself a Docker image. You can specify the number of concurrent replica tasks to run, and the swarm manager ensures that the load is spread evenly across the worker nodes. To the consumer, the Docker service appears to be a single application.

#### Docker services are the scalability component of Docker.

### [Docker image work](https://docs.docker.com/engine/understanding-docker/#/how-does-a-docker-image-work)

Docker images are read-only templates from which Docker containers are instantiated. Each image consists of a series of layers. Docker uses union file systems to combine these layers into a single image. Union file systems allow files and directories of separate file systems, known as branches, to be transparently overlaid, forming a single coherent file system.

These layers are one of the reasons Docker is so lightweight. When you change a Docker image, such as when you update an application to a new version, a new layer is built and replaces only the layer it updates. The other layers remain intact. To distribute the update, you only need to transfer the updated layer. Layering speeds up distribution of Docker images. Docker determines which layers need to be updated at runtime.

An image is defined in a Dockerfile. Every image starts from a base image, such as ubuntu (Note: [Docker Hub](https://hub.docker.com/) is a public registry and stores images). The base image is defined using the FROM keyword in the dockerfile. There are a set of [intructions](https://docs.docker.com/engine/reference/builder/#/parser-directives) after this world usually. Each instruction creates a new layer in the image:
* Specify the base image ([FROM](https://docs.docker.com/engine/reference/builder/#/from))
* Specify the maintainer ([LABEL](https://docs.docker.com/engine/reference/builder/#/label))
* Run a command ([RUN](https://docs.docker.com/engine/reference/builder/#/run))
* Add a file or directory ([ADD](https://docs.docker.com/engine/reference/builder/#/add))
* Create an environment variable ([ENV](https://docs.docker.com/engine/reference/builder/#/env))
* What process to run when launching a container from this image ([CMD](https://docs.docker.com/engine/reference/builder/#/cmd))

Docker reads this Dockerfile when you request a build of an image, executes the instructions, and returns the image.

### [Docker registry work](https://docs.docker.com/engine/understanding-docker/#/how-does-a-docker-registry-work)

A Docker registry stores Docker images. After you build a Docker image, you can push it to a public registry such as Docker Hub or to a private registry running behind your firewall. You can also search for existing images and pull them from the registry to a host.

### [Container work](https://docs.docker.com/engine/understanding-docker/#/how-does-a-container-work)

A container uses the host machine’s Linux/Windows kernel, and consists of any extra files you add when the image is created, along with metadata associated with the container at creation or when the container is started. Each container is built from an image. The image defines the container’s contents, which process to run when the container is launched, and a variety of other configuration details. The Docker image is read-only. When Docker runs a container from an image, it adds a read-write layer on top of the image in which your application runs.

#### What happens when you run a container?

When you use the docker run CLI command or the equivalent API, the Docker Engine client instructs the Docker daemon to run a container. For example:

```bash
$ docker run -i -t ubuntu /bin/bash
```

When you run this command, Docker Engine does the following:

* **Pulls the ubuntu image**: Docker Engine checks for the presence of the ubuntu image. If the image already exists locally, Docker Engine uses it for the new container. Otherwise, then Docker Engine pulls it from Docker Hub.
* **Creates a new container**: Docker uses the image to create a container.
* **Allocates a filesystem and mounts a read-write layer**: The container is created in the file system and a read-write layer is added to the image.
* **Allocates a network / bridge interface**: Creates a network interface that allows the Docker container to talk to the local host.
* **Sets up an IP address**: Finds and attaches an available IP address from a pool.
* **Executes a process that you specify**: Executes the /bin/bash executable.
* **Captures and provides application output**: Connects and logs standard input, outputs and errors for you to see how your application is running, because you requested interactive mode.

You can manage and interact with it, use the services and applications it provides, and eventually stop and remove it.

### [The underlying technology](https://docs.docker.com/engine/understanding-docker/#/the-underlying-technology)

Docker is written in Go programming language.

#### [Namespaces](https://www.toptal.com/linux/separation-anxiety-isolating-your-system-with-linux-namespaces)
Docker uses a technology called namespaces to provide the isolated workspace called the container. When you run a container, Docker creates a set of namespaces for that container.

These namespaces provide a layer of isolation. Each aspect of a container runs in a separate namespace and its access is limited to that namespace.

Docker Engine uses namespaces such as the following on Linux:

* The `pid` namespace: Process isolation (PID: Process ID).
* The `net` namespace: Managing network interfaces (NET: Networking).
* The `ipc` namespace: Managing access to IPC resources (IPC: InterProcess Communication).
* The `mnt` namespace: Managing filesystem mount points (MNT: Mount).
* The `uts` namespace: Isolating kernel and version identifiers. (UTS: Unix Timesharing System).

#### [Control groups](https://en.wikipedia.org/wiki/Cgroups)
Docker Engine on Linux also relies on another technology called *control groups* (`cgroups`). A cgroup limits an application to a specific set of resources. Control groups allow Docker Engine to share available hardware resources to containers and optionally enforce limits and constraints. For example, you can limit the memory available to a specific container.

#### [Union file systems](https://en.wikipedia.org/wiki/UnionFS)
Union file systems, or UnionFS, are file systems that operate by creating layers, making them very lightweight and fast. Docker Engine uses UnionFS to provide the building blocks for containers.

#### [Container format](https://blog.docker.com/2015/07/open-container-format-progress-report/)
Docker Engine combines the namespaces, control groups, and UnionFS into a wrapper called a container format.

Resourses:
* [What is Docker?](https://www.docker.com/what-docker)
* [What is Docker and why is it so darn popular?](http://www.zdnet.com/article/what-is-docker-and-why-is-it-so-darn-popular/)
* [Понимая Docker](https://habrahabr.ru/post/253877/)
* [Docker Overview](https://docs.docker.com/engine/understanding-docker/)
* [Docker libcontainer unifies Linux container powers](http://www.zdnet.com/article/docker-libcontainer-unifies-linux-container-powers/)
* [ASP.NET Community Standup - January 17th, 2017 - Messing with Docker](https://www.youtube.com/watch?v=4nviEODZlsA&list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF&index=0)
* [Working with Windows Containers and Docker: The Basics](https://www.simple-talk.com/sysadmin/virtualization/working-windows-containers-docker-basics/)
* [Understanding Docker presentation](https://docs.google.com/presentation/d/1M-b0BGA57bczBUg3er4rQsBta0U1RQ5lahrY1cEhUew/edit?usp=sharing)

---
## Machine Learning links - 14 January, 2017
Tags: machine learning

This info is from [Machine Learning recourses](https://www.coursera.org/learn/machine-learning)
* [Week 1](./../assets/MachineLearning/MachineLearning1.html)
* [Week 2](./../assets/MachineLearning/MachineLearning2.html)
* [Week 3](./../assets/MachineLearning/MachineLearning3.html)
* [Week 4](./../assets/MachineLearning/MachineLearning4.html)
* [Week 5](./../assets/MachineLearning/MachineLearning5.html)
* [Week 6](./../assets/MachineLearning/MachineLearning6.html)
* [Week 7](./../assets/MachineLearning/MachineLearning7.html)
* [Week 8](./../assets/MachineLearning/MachineLearning8.html)
* [Week 9](./../assets/MachineLearning/MachineLearning9.html)
* [Week 10](./../assets/MachineLearning/MachineLearning10.html)
* [Links](./../assets/MachineLearning/UsefulResources.html)

---
## English minute - 12 January, 2017
Tags: english

* gook
> A derrogatory term used for the purpose of describing a korean.
* chink
> A racist term used to describe the Chinese.
* Honky Cracker
> A word used to describe a piggy ugly ass racist white muthafucking person.
> woman: KKK muthafucker told us to get the fuck out of here!
> man: Oh shit that honky cracker is muthafucker.
* wet backer
> A racial term for someone who swims in order to get to another country. Most commonly used towards Mexicans who swim to America, but may also refer to the Vietnamese who swim to Hong Kong during the Vietnam War to escape from Ho Chi Minh.
> The wet backer has some crazy arms from all that swimming.
* Spik
> Actually "Spik" is derived from the Mexicans themselves. The ones who can't speak very good english would say "no spika da english". So Americans began calling them Spiks.
* wop
> An epithet used for those of Italian descent. WOP stands for WithOut Papers. Many Italian immigrants had no papers to identify themselves and were branded as WOPs.
* Mick
> Derogatory word for Irish people.
* limey
> a British person, term comes from sailors who came to the New World preventing scurvy from sucking limes. Term indigenous to North America
> "That guy over there is a limey, lives in London, and came over here just for a short vacation".

---
## English minute - 9 January, 2017
Tags: english

### Crime stuff

* proximity
> The state, quality, sense, or fact of being near or next; closeness: "Swift's major writings have a proximity and a relevance that is splendidly invigorating" (M.D. Aeschliman).
* robbery
> The act or an instance of unlawfully taking the property of another by the use of violence or intimidation. "He committed dozens of armed robberies."
* burglary
> The act of entering another's premises without authorization in order to commit a crime, such as theft. An instance of this: There were 10 burglaries in the area last month.
* fraud
>  A deception practiced in order to induce another to give up possession of property or surrender a right. A piece of trickery; a trick.
* get away with something
>  escape without any penalty
* caught
> arrested
* aftermath
> after-effect
* sinister
> Suggesting or threatening harm or evil: a sinister smile.
* seeping
> To pass slowly through small openings or pores; ooze: Water is seeping into the basement.
* obsessed
> Can't stop thinking about.
* inning
> 3 strike outs in a row
> OMFG, I can't believe he did a whole inning.
---
## Clustering - 27 December, 2016
Tags: machine learning

Unsupervised learning is contrasted from supervised learning because it uses an unlabeled training set rather than a labeled one.

### K-Means Algorithm

1. Randomly initialize two points in the dataset called the cluster centroids.
2. Cluster assignment: assign all examples into one of two groups based on which cluster centroid the example is closest to.
3. Move centroid: compute the averages for all the points inside each of the two cluster centroid groups, then move the cluster centroid points to those averages.
4. Re-run (2) and (3) until we have found our clusters.

```
Randomly initialize K cluster centroids mu(1), mu(2), ..., mu(K)
Repeat:
   for i = 1 to m:
      c(i):= index (from 1 to K) of cluster centroid closest to x(i)
   for k = 1 to K:
      mu(k):= average (mean) of points assigned to cluster k
```

The first for-loop is the 'Cluster Assignment' step. We make a vector c where c(i) represents the centroid assigned to example x(i).

`\[c^{(i)} = argmin_k\ ||x^{(i)} - \mu_k||^2\]`

`\[||x^{(i)} - \mu_k|| = ||\quad\sqrt{(x_1^i - \mu_{1(k)})^2 + (x_2^i - \mu_{2(k)})^2 + (x_3^i - \mu_{3(k)})^2 + ...}\quad||\]`

The second for-loop is the 'Move Centroid' step where we move each centroid to the average of its group.

`\[\mu_k = \dfrac{1}{n}[x^{(k_1)} + x^{(k_2)} + \dots + x^{(k_n)}] \in \mathbb{R}^n\]`

After a number of iterations the algorithm will converge, where new iterations do not affect the clusters.

### Optimization Objective

Using these variables we can define our cost function:

`\[J(c^{(i)},\dots,c^{(m)},\mu_1,\dots,\mu_K) = \dfrac{1}{m}\sum_{i=1}^m ||x^{(i)} - \mu_{c^{(i)}}||^2\]`

With k-means, it is not possible for the cost function to sometimes increase. It should always descend.

### Random Initialization

1. Have K<m. That is, make sure the number of your clusters is less than the number of your training examples.
2. Randomly pick K training examples. (Not mentioned in the lecture, but also be sure the selected examples are unique).
3. Set μ1,…,μK equal to these K examples.

### Choosing the Number of Clusters

Choosing K can be quite arbitrary and ambiguous.

A way to choose K is to observe how well k-means performs on a downstream purpose. In other words, you choose K that proves to be most useful for some goal you're trying to achieve from using these clusters.

### Dimensionality Reduction

Motivation I: Data Compression

1. We may want to reduce the dimension of our features if we have a lot of redundant data.
2. To do this, we find two highly correlated features, plot them, and make a new line that seems to describe both features accurately. We place all the new features on this single line.

Doing dimensionality reduction will reduce the total data we have to store in computer memory and will speed up our learning algorithm.

Motivation II: Visualization

It is not easy to visualize data that is more than three dimensions. We can reduce the dimensions of our data to 3 or less in order to plot it.

### Principal Component Analysis Problem Formulation

The most popular dimensionality reduction algorithm is Principal Component Analysis (PCA)

Problem formulation

Given two features, x1 and x2, we want to find a single line that effectively describes both features at once. We then map our old features onto this new line to get a new single feature.

The goal of PCA is to reduce the average of all the distances of every feature to the projection line. This is the projection error.

**PCA is not linear regression**

* In linear regression, we are minimizing the squared error from every point to our predictor line. These are vertical distances.
* In PCA, we are minimizing the shortest distance, or shortest orthogonal distances, to our data points.

### Principal Component Analysis Algorithm

* Given training set: x(1),x(2),…,x(m)
* Preprocess (feature scaling/mean normalization):

`\[\mu_j = \dfrac{1}{m}\sum^m_{i=1}x_j^{(i)}\]`

* Replace each `\(x_j^{(i)}\)` with `\(x_j^{(i)} - \mu_j\)`
* If different features on different scales (e.g., x1 = size of house, x2 = number of bedrooms), scale features to have comparable range of values.

1. Compute "covariance matrix"

`\[\Sigma = \dfrac{1}{m}\sum^m_{i=1}(x^{(i)})(x^{(i)})^T\]`

2. Compute "eigenvectors" of covariance matrix Σ

```
[U,S,V] = svd(Sigma);
```

3. Take the first k columns of the U matrix and compute z

Summarize:

```
Sigma = (1/m) * X' * X; % compute the covariance matrix
[U,S,V] = svd(Sigma);   % compute our projected directions
Ureduce = U(:,1:k);     % take the first k directions
Z = X * Ureduce;        % compute the projected data points
```

### Reconstruction from Compressed Representation

`\[x_{approx}^{(1)} = U_{reduce} \cdot z^{(1)}\]`

### Choosing the Number of Principal Components

Algorithm for choosing k

1. Try PCA with k=1,2,…
2. Compute `\(U_{reduce}, z, x\)`
3. Check the formula given above that 99% of the variance is retained. If not, go to step one and increase k.

### Advice for Applying PCA

* Compressions
* Reduce space of data
* Speed up algorithm
* Visualization of data

**Bad use of PCA**: trying to prevent overfitting. 

Don't assume you need to do PCA. **Try your full machine learning algorithm without PCA first**. Then use PCA if you find that you need it.

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Optimization Objective - 27 December, 2016
Tags: machine learning

The Support Vector Machine (SVM) is yet another type of supervised machine learning algorithm. It is sometimes cleaner and more powerful.

### Cost function

`\(\text{cost}_1(z)\)` and `\(\text{cost}_0(z)\)` (respectively, note that `\(\text{cost}_1(z)\)` is the cost for classifying when y=1, and `\(\text{cost}_0(z)\)` is the cost for classifying when y=0)

![image](../images/Svm_hing.png)
![image](../images/Svm_hinge_negative_class.png)

`\[J(\theta) = C\sum_{i=1}^m y^{(i)} \ \text{cost}_1(\theta^Tx^{(i)}) + (1 - y^{(i)}) \ \text{cost}_0(\theta^Tx^{(i)}) + \dfrac{1}{2}\sum_{j=1}^n \Theta^2_j\]`

Note that the hypothesis of the Support Vector Machine is not interpreted as the probability of y being 1 or 0 (as it is for the hypothesis of logistic regression). Instead, it outputs either 1 or 0. (In technical terms, it is a discriminant function.)

`\[h_\theta(x) =\begin{cases}    1 & \text{if} \ \Theta^Tx \geq 0 \\    0 & \text{otherwise}\end{cases}\]`

### Large Margin Intuition

A useful way to think about Support Vector Machines is to think of them as Large Margin Classifiers.

* If y=1, we want `\(\Theta^Tx \geq 1\)` (not just ≥0)
* If y=0, we want `\(\Theta^Tx \leq -1\)` (not just <0)

In SVMs, the decision boundary has the special property that it is as far away as possible from both the positive and the negative examples.

This large margin is only achieved when C is very large. Data is linearly separable when a straight line can separate the positive and negative examples. If we have outlier examples that we don't want to affect the decision boundary, then we can reduce C. Increasing and decreasing C is similar to respectively decreasing and increasing λ, and can simplify our decision boundary.

### Kernels

Kernels allow us to make complex, non-linear classifiers using Support Vector Machines.

`\[f_i = similarity(x, l^{(i)}) = \exp(-\dfrac{||x - l^{(i)}||^2}{2\sigma^2})\]`

This "similarity" function is called a Gaussian Kernel. It is a specific example of a kernel.

If `\(x \approx l^{(i)}\)`, then `\(f_i = \exp(-\dfrac{\approx 0^2}{2\sigma^2}) \approx 1\)`
If x is far from `\(l^{(i)}\)`, then `\(f_i = \exp(-\dfrac{(large\ number)^2}{2\sigma^2}) \approx 0\)`

One way to get the landmarks is to put them in the exact same locations as all the training examples. This gives us m landmarks, with one landmark per training example.

Using kernels to generate f(i) is not exclusive to SVMs and may also be applied to logistic regression. However, because of computational optimizations on SVMs, kernels combined with SVMs is much faster than with other algorithms, so kernels are almost always found combined only with SVMs.

### Choosing SVM Parameters

* If C is large, then we get higher variance/lower bias
* If C is small, then we get lower variance/higher bias

* With a large `\(σ^2\)`, the features fi vary more smoothly, causing higher bias and lower variance.
* With a small `\(σ^2\)`, the features fi vary less smoothly, causing lower bias and higher variance.

### Using An SVM

* Choice of parameter C
* Choice of kernel (similarity function)
* No kernel ("linear" kernel) -- gives standard linear classifier
* Choose when n is large and when m is small
* Gaussian Kernel (above) -- need to choose σ2
* Choose when n is small and m is large

### Logistic Regression vs. SVMs

* If n is large (relative to m), then use logistic regression, or SVM without a kernel (the "linear kernel")
* If n is small and m is intermediate, then use SVM with a Gaussian Kernel
* If n is small and m is large, then manually create/add more features, then use logistic regression or SVM without a kernel.
In the first case, we don't have enough examples to need a complicated polynomial hypothesis. In the second example, we have enough examples that we may need a complex non-linear hypothesis. In the last case, we want to increase our features so that logistic regression becomes applicable.

**Note**: a neural network is likely to work well for any of these situations, but may be slower to train.

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Applying machine learning - 27 December, 2016
Tags: machine learning

Errors in your predictions can be troubleshooted by:

* Getting more training examples
* Trying smaller sets of features
* Trying additional features
* Trying polynomial features
* Increasing or decreasing λ

It is good to use test error factor to understand the correctness of our algorithm:

`\[\text{Test Error} = \dfrac{1}{m_{test}} \sum^{m_{test}}_{i=1} err(h_\Theta(x^{(i)}_{test}), y^{(i)}_{test})\]`

and this factor is calculated using completely separate training set (test set).

Using **Cross Validation Set** our test set will give us an accurate, non-optimistic error.

One example way to break down our dataset into the three sets is:

* Training set: 60%
* Cross validation set: 20%
* Test set: 20%
We can now calculate three separate error values for the three different sets.

With the Validation Set (note: this method presumes we do not also use the CV set for regularization)

1. Optimize the parameters in Θ using the training set for each polynomial degree.
2. Find the polynomial degree d with the least error using the cross validation set.
3. Estimate the generalization error using the test set with `\(J_{test}(\Theta^{(d)})\)`, (d = theta from polynomial with lower error).

### Diagnosing Bias vs. Variance

High bias is underfitting and high variance is overfitting. 

![image](../images/features-and-polynom-degree.png)

## Regularization and Bias/Variance

* Large λ: High bias (underfitting)
* Small λ: High variance (overfitting)

![image](../images/features-and-polynom-degree-fix2.png)

In order to choose the model and the regularization λ, we need:

1. Create a list of lambda (i.e. λ∈{0,0.01,0.02,0.04,0.08,0.16,0.32,0.64,1.28,2.56,5.12,10.24});
2. Select a lambda to compute;
3. Create a model set like degree of the polynomial or others;
4. Select a model to learn Θ;
5. Learn the parameter Θ for the model selected, using `\(J_{train}(\Theta)\)` with λ selected (this will learn Θ for the next step);
6. Compute the train error using the learned Θ (computed with λ ) on the `\(J_{train}(\Theta)\)` without regularization or λ = 0;
7. Compute the cross validation error using the learned Θ (computed with λ) on the `\(J_{CV}(\Theta)\)` without regularization or λ = 0;
8. Do this for the entire model set and lambdas, then select the best combo that produces the lowest error on the cross validation set;
9. Now if you need visualize to help you understand your decision, you can plot to the figure like above with: (λ x Cost J`\(J_{train}(\Theta)\)` and (λ x Cost `\(J_{CV}(\Theta)\)`);
10. Now using the best combo Θ and λ, apply it on `\(J_{test}(\Theta)\)` to see if it has a good generalization of the problem.
11. To help decide the best polynomial degree and λ to use, we can diagnose with the learning curves.

### Learning Curves

If a learning algorithm is suffering from high variance, getting more training data is likely to help.
If a learning algorithm is suffering from high bias, getting more training data will not (by itself) help much.

![image](../images/Learning1.png)
![image](../images/Learning2.png)

### Deciding What to Do Next Revisited

Decision process can be broken down as follows:

1. Getting more training examples - Fixes high variance
2. Trying smaller sets of features - Fixes high variance
3. Adding features - Fixes high bias
4. Adding polynomial features - Fixes high bias
5. Decreasing λ - Fixes high bias
6. Increasing λ - Fixes high variance

### Diagnosing Neural Networks

* A neural network with fewer parameters is prone to underfitting. It is also computationally cheaper.
* A large neural network with more parameters is prone to overfitting. It is also computationally expensive. In this case you can use regularization (increase λ) to address the overfitting.

Using a single hidden layer is a good starting default. You can train your neural network on a number of hidden layers using your cross validation set.

## Error Analysis

The recommended approach to solving machine learning problems is:

* Start with a simple algorithm, implement it quickly, and test it early.
* Plot learning curves to decide if more data, more features, etc. will help
* Error analysis: manually examine the errors on examples in the cross validation set and try to spot a trend.

It's important to get error results as a single, numerical value. Otherwise it is difficult to assess your algorithm's performance.

### Error Metrics for Skewed Classes

Precision:

`\[\dfrac{\text{True Positives}}{\text{Total number of predicted positives}}
= \dfrac{\text{True Positives}}{\text{True Positives}+\text{False positives}}\]`

Recall:

`\[\dfrac{\text{True Positives}}{\text{Total number of actual positives}}= \dfrac{\text{True Positives}}{\text{True Positives}+\text{False negatives}}\]`

### Trading Off Precision and Recall

A better way is to compute the F Score (or F1 score):

`\[\text{F Score} = 2\dfrac{PR}{P + R}\]`

In order for the F Score to be large, both precision and recall must be large. We want to train precision and recall on the cross validation set so as not to bias our test set.

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Discord Belarus chat By - 22 December, 2016
Tags: chat, by, minsk, belarus, discord

Приглашаю всех присоединиться к белорусскому серверу в Discrod - **чату**, который позволит пообщаться и познакомиться!

Возможно, у многих есть желание пообщаться с людьми, которые живут с тобой в одном городе и стране. Такого места нет для тех, кто живет в Беларуси, или хочет общаться с людьми, которым небезразлична жизнь в Беларуси, особенно после закрытия [chat.by](http://chat.by). Наверное, единственного и широко известного чата для Беларуси.

Для того, чтобы избежать создания отдельного сайта, который будет собирать информацию о Вас и ваши сообщения, был создан сервер в отличном сервисе - [Discord](https://discordapp.com/). _Присоединяйтесь, знакомьтесь и общайтесь с интересными людьми!_ Переходите по [этой ссылке](https://discord.gg/Pt2ERRv) и регистрируйтесь!

На данный момент этот сервер не очень известен и популярен, но, надеюсь, общими усилиями мы сделаем его приятным и интересным местом для нас всех! Заранее спасибо!

С уважением,  
Discord Chat By

---
## InfluxDB can't be started as a service - 21 December, 2016
Tags: influxdb, ubuntu

I was not able to run influxdb as a service after [installing](https://docs.influxdata.com/influxdb/v1.1/introduction/installation/) influxdb on my local ubuntu:

Command `sudo service influxd status` showed:

```
● influxdb.service - InfluxDB is an open-source, distributed, time series database
   Loaded: loaded (/lib/systemd/system/influxdb.service; enabled; vendor preset: enabled)
   Active: inactive (dead) (Result: exit-code) since Fri 2016-12-23 15:05:34 CET; 3min 47s ago
     Docs: https://docs.influxdata.com/influxdb/
  Process: 13938 ExecStart=/usr/bin/influxd -config /etc/influxdb/influxdb.conf ${INFLUXD_OPTS} (code=exited, status=1/FAILURE)
 Main PID: 13938 (code=exited, status=1/FAILURE)

Dec 23 15:05:34 fes-U36SG systemd[1]: influxdb.service: Main process exited, code=exited, status=1/FAILURE
Dec 23 15:05:34 fes-U36SG systemd[1]: influxdb.service: Unit entered failed state.
Dec 23 15:05:34 fes-U36SG systemd[1]: influxdb.service: Failed with result 'exit-code'.
Dec 23 15:05:34 fes-U36SG systemd[1]: influxdb.service: Service hold-off time over, scheduling restart.
Dec 23 15:05:34 fes-U36SG systemd[1]: Stopped InfluxDB is an open-source, distributed, time series database.
Dec 23 15:05:34 fes-U36SG systemd[1]: influxdb.service: Start request repeated too quickly.
Dec 23 15:05:34 fes-U36SG systemd[1]: Failed to start InfluxDB is an open-source, distributed, time series database.
```

But the simple command `influxd` worked well.

I found that there is the next code in `/lib/systemd/system/influxdb.service`:

```
# If you modify this, please also make sure to edit init.sh

[Unit]
Description=InfluxDB is an open-source, distributed, time series database
Documentation=https://docs.influxdata.com/influxdb/
After=network-online.target

[Service]
User=influxdb
Group=influxdb
LimitNOFILE=65536
EnvironmentFile=-/etc/default/influxdb
ExecStart=/usr/bin/influxd -config /etc/influxdb/influxdb.conf ${INFLUXD_OPTS}
KillMode=control-group
Restart=on-failure

[Install]
WantedBy=multi-user.target
Alias=influxd.service
```

All is working well after I commended the next two lines:

```
[Service]
# User=influxdb
# Group=influxdb
```

Thanks!

---
## Add dotnet command to alias of bash - 19 December, 2016
Tags: bash, ubuntu, dotnet

### Dotnet commands
I am using sample application created by `dotnet run`.

To run dotnet core application on Ubuntu with **compilation**:

```
dotnet run -p {pathToFolderWithProjectJson} -- {arguments}
```

To run compiled application:

```
dotnet {pathToCompiledDll} {arguments}
```
where `pathToCompiledDll` is a path to dll (it is in `/bin/Debug/netcoreapp1.1/` be default).

### Bash commands

To add dotnet alias (shortcut) to bash set of commands:

```
nano ~/.bash_aliases
```

And you need to add the next line to this file (bash_aliases):

```
alias helloWorld="dotnet run -p ~/projects/helloWorld -- ~/Peter"
```
where 
* ~/projects/helloWorld is path to the project;
* ~/Peter is an argument passed to the application;

Now you are able to run `helloWorld` in bash terminal after restarting it. You should see *Hello World*.

Links:
* [How do I create shortcut commands in the Ubuntu terminal?](http://stackoverflow.com/questions/5658781/how-do-i-create-shortcut-commands-in-the-ubuntu-terminal)
* [dotnet run](https://docs.microsoft.com/en-us/dotnet/articles/core/tools/dotnet-run)
* [How to create a permanent “alias”?](http://askubuntu.com/questions/1414/how-to-create-a-permanent-alias#5278)

---
## Simple question to check understanding of recursion in .NET - 19 December, 2016
Tags: C#, interview, dotnet

There is the next code:

```csharp
public static void Main(string[] args) {
    Console.WriteLine(Test());
}

public static int Test(int index = 0) {
    if (index > 10) return 0;
    try {
        for (var i=0; i< 10; i++) {
            index++;
        }
    }
    catch {
        Console.Write("Error");
    }
    for (var j = 1; j < 10; j++)
        index += Test(index);
    return index;
}

```

Is it working? What is the output?

---
## Implementing async.queue using rxjs - 18 December, 2016
Tags: rxjs, javascript, async.js

There is an example of queue using rxjs:

```javascript
Rx.Observable.from(['foo', 'bar', 'baz', 'bay', 'bax', 'bar', 'cat'])
    .do(x => console.log((new Date).toLocaleTimeString() + " " + x))
    // grouping by 2 
    .bufferCount(2)
    // concat received results
    .concatMap((data) => {
        return Rx.Observable.defer(() => {
            // assuming long operation here, e.g. downloading,
            // we can use merge here if we want to do operation per item
            return Rx.Observable.create((observer)=>{
                setTimeout(function () {
                    observer.next(data);
                    observer.complete();
                }, 2000);
            })
        });
    }) 
    .subscribe(
        result => console.log((new Date).toLocaleTimeString() + " finished " + result),
        error => console.error(error),
        () => console.log('done')
    );

// Console ouput
// rxjs.html:9 Console was cleared
// rxjs.html:11 9:56:36 PM foo
// rxjs.html:11 9:56:36 PM bar
// rxjs.html:11 9:56:36 PM baz
// rxjs.html:11 9:56:36 PM bay
// rxjs.html:11 9:56:36 PM bax
// rxjs.html:11 9:56:36 PM bar
// rxjs.html:11 9:56:36 PM cat
// rxjs.html:28 9:56:38 PM finished foo,bar
// rxjs.html:28 9:56:40 PM finished baz,bay
// rxjs.html:28 9:56:42 PM finished bax,bar
// rxjs.html:28 9:56:44 PM finished cat
// rxjs.html:30 done
```
Used links:
* [Question about promise chains/queues in Rx](https://github.com/Reactive-Extensions/RxJS/issues/613)
* [Defer](http://reactivex.io/documentation/operators/defer.html)
* [RxJS](https://github.com/ReactiveX/rxjs)

---
## Neural Networks: Learning - 11 December, 2016
Tags: machine learning

### Cost Function

a) L= total number of layers in the network

b) `\(s_l\)` = number of units (not counting bias unit) in layer l

c) K= number of output units/classes

`\[\begin{gather*}\large J(\Theta) = - \frac{1}{m} \sum_{i=1}^m \sum_{k=1}^K \left[y^{(i)}_k \log ((h_\Theta (x^{(i)}))_k) + (1 - y^{(i)}_k)\log (1 - (h_\Theta(x^{(i)}))_k)\right] + \frac{\lambda}{2m}\sum_{l=1}^{L-1} \sum_{i=1}^{s_l} \sum_{j=1}^{s_{l+1}} ( \Theta_{j,i}^{(l)})^2\end{gather*}\]`

### Backpropagation Algorithm

"Backpropagation" is neural-network terminology for minimizing our cost function, just like what we were doing with gradient descent in logistic and linear regression.

In back propagation we're going to compute for every node:

`\(\delta_j^{(l)}\)` - = "error" of node j in layer l

`\(a_j^{(l)}\)` is activation node j in layer l.

For the last layer, we can compute the vector of delta values with:

`\[\delta^{(L)} = a^{(L)} - y\]`

To get the delta values of the layers before the last layer, we can use an equation that steps us back from right to left:

`\[\delta^{(l)} = ((\Theta^{(l)})^T \delta^{(l+1)})\ .*\ g'(z^{(l)})\]`

`\[g'(z^{(l)}) = a^{(l)}\ .*\ (1 - a^{(l)})\]`

### Backpropagation algorithm

Given training set `\(\lbrace (x^{(1)}, y^{(1)}) \cdots (x^{(m)}, y^{(m)})\rbrace\)`;

* Set `\(\Delta^{(l)}_{i,j}\)`= 0 for all (l,i,j)

For training example t =1 to m:

* Set `\(a^{(1)} := x^{(t)}\)`
* Perform forward propagation to compute `\(a^{(l)}\)` for l=2,3,…,L
* Using `\(y^{(t)}\)`, compute `\(\delta^{(L)} = a^{(L)} - y^{(t)}\)`
* Compute `\(\delta^{(L-1)}, \delta^{(L-2)},\dots,\delta^{(2)}\)` using `\(\delta^{(l)} = ((\Theta^{(l)})^T \delta^{(l+1)})\ .*\ a^{(l)}\ .*\ (1 - a^{(l)})\)`
* `\(\Delta^{(l)}_{i,j} := \Delta^{(l)}_{i,j} + a_j^{(l)} \delta_i^{(l+1)}\)` or with vectorization, `\(\Delta^{(l)} := \Delta^{(l)} + \delta^{(l+1)}(a^{(l)})^T\)`
* `\(D^{(l)}_{i,j} := \dfrac{1}{m}\left(\Delta^{(l)}_{i,j} + \lambda\Theta^{(l)}_{i,j}\right)\)` If j≠0 NOTE: Typo in lecture slide omits outside parentheses. This version * is correct.
* `\(D^{(l)}_{i,j} := \dfrac{1}{m}\Delta^{(l)}_{i,j}\)` If j=0

### Gradient Checking

Gradient checking will assure that our backpropagation works as intended.

`\[\dfrac{\partial}{\partial\Theta}J(\Theta) \approx \dfrac{J(\Theta + \epsilon) - J(\Theta - \epsilon)}{2\epsilon}\]`

Once you've verified once that your backpropagation algorithm is correct, then you don't need to compute gradApprox again. The code to compute gradApprox is very slow.

### Random Initialization

Initializing all theta weights to zero does not work with neural networks. When we backpropagate, all nodes will update to the same value repeatedly.

### Summary

First, pick a network architecture; choose the layout of your neural network, including how many hidden units in each layer and how many layers total.

* Number of input units = dimension of features `\(x^{(i)}\)`
* Number of output units = number of classes
* Number of hidden units per layer = usually more the better (must balance with cost of computation as it increases with more hidden units)
* Defaults: 1 hidden layer. If more than 1 hidden layer, then the same number of units in every hidden * layer.

**Training a Neural Network**

1. Randomly initialize the weights
2. Implement forward propagation to get `\(h_\theta(x^{(i)})\)`
3. Implement the cost function
4. Implement backpropagation to compute partial derivatives
5. Use gradient checking to confirm that your backpropagation works. Then disable gradient checking.
6. Use gradient descent or a built-in optimization function to minimize the cost function with the weights in theta.

When we perform forward and back propagation, we loop on every training example:

```
for i = 1:m,
   Perform forward propagation and backpropagation using example (x(i),y(i))
   (Get activations a(l) and delta terms d(l) for l = 2,...,L
```

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Neural Networks - 6 December, 2016
Tags: machine learning

Neural networks are limited imitations of how our own brains work. They've had a big recent resurgence because of advances in computer hardware.

In neural networks, we use the same logistic function as in classification: `\(\frac{1}{1 + e^{-\theta^Tx}}\)`. In neural networks however we sometimes call it a sigmoid (logistic) activation function.

Visually, a simplistic representation looks like:

`\[\begin{bmatrix}x_0 \newline x_1 \newline x_2 \newline x_3\end{bmatrix}\rightarrow\begin{bmatrix}a_1^{(2)} \newline a_2^{(2)} \newline a_3^{(2)} \newline \end{bmatrix}\rightarrow h_\theta(x)\]`

The values for each of the "activation" nodes is obtained as follows:

`\[\begin{align*}
a_1^{(2)} = g(\Theta_{10}^{(1)}x_0 + \Theta_{11}^{(1)}x_1 + \Theta_{12}^{(1)}x_2 + \Theta_{13}^{(1)}x_3) \newline
a_2^{(2)} = g(\Theta_{20}^{(1)}x_0 + \Theta_{21}^{(1)}x_1 + \Theta_{22}^{(1)}x_2 + \Theta_{23}^{(1)}x_3) \newline
a_3^{(2)} = g(\Theta_{30}^{(1)}x_0 + \Theta_{31}^{(1)}x_1 + \Theta_{32}^{(1)}x_2 + \Theta_{33}^{(1)}x_3) \newline
h_\Theta(x) = a_1^{(3)} = g(\Theta_{10}^{(2)}a_0^{(2)} + \Theta_{11}^{(2)}a_1^{(2)} + \Theta_{12}^{(2)}a_2^{(2)} + \Theta_{13}^{(2)}a_3^{(2)}) \newline
\end{align*}\]`

Each layer gets its own matrix of weights, `\(\Theta^{(j)}\)`.

The dimensions of these matrices of weights is determined as follows:

`\[\text{If network has $s_j$ units in layer $j$ and $s_{j+1}$ units in layer $j+1$, then $\Theta^{(j)}$ will be of dimension $s_{j+1} \times (s_j + 1)$.}\]`

### Multiclass Classification

`\[\begin{align*}\begin{bmatrix}x_0 \newline x_1 \newline x_2 \newline\cdots \newline x_n\end{bmatrix} \rightarrow\begin{bmatrix}a_0^{(2)} \newline a_1^{(2)} \newline a_2^{(2)} \newline\cdots\end{bmatrix} \rightarrow\begin{bmatrix}a_0^{(3)} \newline a_1^{(3)} \newline a_2^{(3)} \newline\cdots\end{bmatrix} \rightarrow \cdots \rightarrow\begin{bmatrix}h_\Theta(x)_1 \newline h_\Theta(x)_2 \newline h_\Theta(x)_3 \newline h_\Theta(x)_4 \newline\end{bmatrix} \rightarrow\end{align*}\]`

We can define our set of resulting classes as y:

![Neur_multi_res](./../images/multi_neur_res.png)

Our final value of our hypothesis for a set of inputs will be one of the elements in y.

---
## Logistic regression - 5 December, 2016
Tags: octave, machine learning

### Binary Classification

The range of values is between 0 or 1.

Hypothesis Representation:

`\[\begin{align*}& h_\theta (x) =  g ( \theta^T x ) \newline \newline& z = \theta^T x \newline& g(z) = \dfrac{1}{1 + e^{-z}}\end{align*}\]`

![Logistic_hyp](./../images/logisticfunction.png)

### Decision Boundary

`\[\begin{align*}& h_\theta(x) \geq 0.5 \rightarrow y = 1 \newline& h_\theta(x) < 0.5 \rightarrow y = 0 \newline\end{align*}\]`

The decision boundary is the line that separates the area where y = 0 and where y = 1. It is created by our hypothesis function.

### Cost Function

`\[J(\theta) = - \frac{1}{m} \displaystyle \sum_{i=1}^m [y^{(i)}\log (h_\theta (x^{(i)})) + (1 - y^{(i)})\log (1 - h_\theta(x^{(i)}))]\]`

### Gradient Descent

`\[\begin{align*}
& Repeat \; \lbrace \newline
& \; \theta_j := \theta_j - \frac{\alpha}{m} \sum_{i=1}^m (h_\theta(x^{(i)}) - y^{(i)}) x_j^{(i)} \newline & \rbrace
\end{align*}\]`

### Regularized Linear Regression

`\[\begin{align*}
& \text{Repeat}\ \lbrace \newline
& \ \ \ \ \theta_0 := \theta_0 - \alpha\ \frac{1}{m}\ \sum_{i=1}^m (h_\theta(x^{(i)}) - y^{(i)})x_0^{(i)} \newline
& \ \ \ \ \theta_j := \theta_j - \alpha\ \left[ \left( \frac{1}{m}\ \sum_{i=1}^m (h_\theta(x^{(i)}) - y^{(i)})x_j^{(i)} \right) + \frac{\lambda}{m}\theta_j \right] &\ \ \ \ \ \ \ \ \ \ j \in \lbrace 1,2...n\rbrace\newline
& \rbrace
\end{align*}\]`

### Regularized Logistic Regression

`\[J(\theta) = - \frac{1}{m} \sum_{i=1}^m \large[ y^{(i)}\ \log (h_\theta (x^{(i)})) + (1 - y^{(i)})\ \log (1 - h_\theta(x^{(i)}))\large] + \frac{\lambda}{2m}\sum_{j=1}^n \theta_j^2\]`

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Octave Tutorial - 26 November, 2016
Tags: octave

Information is from this wonderful [course](https://www.coursera.org/learn/machine-learning/).

### Basic operations

```Octave
%% Change Octave prompt  
PS1('>> ');
%% Change working directory in windows example:
cd 'c:/path/to/desired/directory name'
%% Note that it uses normal slashes and does not use escape characters for the empty spaces.

%% elementary operations
5+6
3-2
5*8
1/2
2^6
1 == 2 % false
1 ~= 2 % true.  note, not "!="
1 && 0
1 || 0
xor(1,0)


%% variable assignment
a = 3; % semicolon suppresses output
b = 'hi';
c = 3>=1;

% Displaying them:
a = pi
disp(a)
disp(sprintf('2 decimals: %0.2f', a))
disp(sprintf('6 decimals: %0.6f', a))
format long
a
format short
a


%%  vectors and matrices
A = [1 2; 3 4; 5 6]


v = [1 2 3]
v = [1; 2; 3]
v = 1:0.1:2   % from 1 to 2, with stepsize of 0.1. Useful for plot axes
v = 1:6       % from 1 to 6, assumes stepsize of 1 (row vector)

C = 2*ones(2,3) % same as C = [2 2 2; 2 2 2]
w = ones(1,3)   % 1x3 vector of ones
w = zeros(1,3)
w = rand(1,3) % drawn from a uniform distribution 
w = randn(1,3)% drawn from a normal distribution (mean=0, var=1)
w = -6 + sqrt(10)*(randn(1,10000));  % (mean = -6, var = 10) - note: add the semicolon
hist(w)    % plot histogram using 10 bins (default)
hist(w,50) % plot histogram using 50 bins
% note: if hist() crashes, try "graphics_toolkit('gnu_plot')" 

I = eye(4)   % 4x4 identity matrix

% help function
help eye
help rand
help help
```

### Moving Data Around

```Octave
%% dimensions
sz = size(A) % 1x2 matrix: [(number of rows) (number of columns)]
size(A,1) % number of rows
size(A,2) % number of cols
length(v) % size of longest dimension


%% loading data
pwd   % show current directory (current path)
cd 'C:\Users\ang\Octave files'  % change directory 
ls    % list files in current directory 
load q1y.dat   % alternatively, load('q1y.dat')
load q1x.dat
who   % list variables in workspace
whos  % list variables in workspace (detailed view) 
clear q1y      % clear command without any args clears all vars
v = q1x(1:10); % first 10 elements of q1x (counts down the columns)
save hello.mat v;  % save variable v into file hello.mat
save hello.txt v -ascii; % save as ascii
% fopen, fread, fprintf, fscanf also work  [[not needed in class]]

%% indexing
A(3,2)  % indexing is (row,col)
A(2,:)  % get the 2nd row. 
        % ":" means every element along that dimension
A(:,2)  % get the 2nd col
A([1 3],:) % print all  the elements of rows 1 and 3

A(:,2) = [10; 11; 12]     % change second column
A = [A, [100; 101; 102]]; % append column vec
A(:) % Select all elements as a column vector.

% Putting data together 
A = [1 2; 3 4; 5 6]
B = [11 12; 13 14; 15 16] % same dims as A
C = [A B]  % concatenating A and B matrices side by side
C = [A, B] % concatenating A and B matrices side by side
C = [A; B] % Concatenating A and B top and bottom
```

### Computing on Data

```Octave
%% initialize variables
A = [1 2;3 4;5 6]
B = [11 12;13 14;15 16]
C = [1 1;2 2]
v = [1;2;3]

%% matrix operations
A * C  % matrix multiplication
A .* B % element-wise multiplication
% A .* C  or A * B gives error - wrong dimensions
A .^ 2 % element-wise square of each element in A
1./v   % element-wise reciprocal
log(v)  % functions like this operate element-wise on vecs or matrices 
exp(v)
abs(v)

-v  % -1*v

v + ones(length(v), 1)  
% v + 1  % same

A'  % matrix transpose

%% misc useful functions

% max  (or min)
a = [1 15 2 0.5]
val = max(a)
[val,ind] = max(a) % val -  maximum element of the vector a and index - index value where maximum occur
val = max(A) % if A is matrix, returns max from each column

% compare values in a matrix & find
a < 3 % checks which values in a are less than 3
find(a < 3) % gives location of elements less than 3
A = magic(3) % generates a magic matrix - not much used in ML algorithms
[r,c] = find(A>=7)  % row, column indices for values matching comparison

% sum, prod
sum(a)
prod(a)
floor(a) % or ceil(a)
max(rand(3),rand(3))
max(A,[],1) -  maximum along columns(defaults to columns - max(A,[]))
max(A,[],2) - maximum along rows
A = magic(9)
sum(A,1)
sum(A,2)
sum(sum( A .* eye(9) ))
sum(sum( A .* flipud(eye(9)) ))


% Matrix inverse (pseudo-inverse)
pinv(A)        % inv(A'*A)*A'
```

### Plotting Data

```Octave
%% plotting
t = [0:0.01:0.98];
y1 = sin(2*pi*4*t); 
plot(t,y1);
y2 = cos(2*pi*4*t);
hold on;  % "hold off" to turn off
plot(t,y2,'r');
xlabel('time');
ylabel('value');
legend('sin','cos');
title('my plot');
print -dpng 'myPlot.png'
close;           % or,  "close all" to close all figs
figure(1); plot(t, y1);
figure(2); plot(t, y2);
figure(2), clf;  % can specify the figure number
subplot(1,2,1);  % Divide plot into 1x2 grid, access 1st element
plot(t,y1);
subplot(1,2,2);  % Divide plot into 1x2 grid, access 2nd element
plot(t,y2);
axis([0.5 1 -1 1]);  % change axis scale

%% display a matrix (or image) 
figure;
imagesc(magic(15)), colorbar, colormap gray;
% comma-chaining function calls.  
a=1,b=2,c=3
a=1;b=2;c=3;
```

### Control statements: for, while, if statements

```Octave
v = zeros(10,1);
for i=1:10, 
    v(i) = 2^i;
end;
% Can also use "break" and "continue" inside for and while loops to control execution.

i = 1;
while i <= 5,
  v(i) = 100; 
  i = i+1;
end

i = 1;
while true, 
  v(i) = 999; 
  i = i+1;
  if i == 6,
    break;
  end;
end

if v(1)==1,
  disp('The value is one!');
elseif v(1)==2,
  disp('The value is two!');
else
  disp('The value is not one or two!');
end
```

### Functions

To create a function, type the function code in a text editor (e.g. gedit or notepad), and save the file as "functionName.m"

```Octave
function y = squareThisNumber(x)

y = x^2;
```

```Octave
function [y1, y2] = squareandCubeThisNo(x)
y1 = x^2
y2 = x^3
```

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Linear Regression with Multiple Variables - 24 November, 2016
Tags: machine learning

Hypothesis function looks like:

`\[h_\theta (x) = \theta_0 + \theta_1 x_1 + \theta_2 x_2 + \theta_3 x_3 + \cdots + \theta_n x_n\]`

Vectorized version:

`\[h_\theta(X) = X \theta\]`

Vectorized version of cost function:

`\[J(\theta) = \dfrac {1}{2m} (X\theta - \vec{y})^{T} (X\theta - \vec{y})\]`

Gradient descent:

`\[\begin{align*}& \text{repeat until convergence:} \; \lbrace \newline \; & \theta_j := \theta_j - \alpha \frac{1}{m} \sum\limits_{i=1}^{m} (h_\theta(x^{(i)}) - y^{(i)}) \cdot x_j^{(i)} \;  & \text{for j := 0..n}\newline \rbrace\end{align*}\]`

Vectorized version:

`\[theta := \theta - \frac{\alpha}{m} X^{T} (X\theta - \vec{y})\]`

### Feature Normalization

We can speed up gradient descent by having each of our input values in roughly the same range. This is because θ will descend quickly on small ranges and slowly on large ranges, and so will oscillate inefficiently down to the optimum when the variables are very uneven.

Two techniques to help with this are feature scaling and mean normalization. Feature scaling involves dividing the input values by the range (i.e. the maximum value minus the minimum value) of the input variable, resulting in a new range of just 1. Mean normalization involves subtracting the average value for an input variable from the values for that input variable, resulting in a new average value for the input variable of just zero. To implement both of these techniques, adjust your input values as shown in this formula:

`\[x_i := \dfrac{x_i - \mu_i}{s_i}\]`

Where `\(μ_i\)` is the average of all the values for feature (i) and `\(s_i\)` is the range of values (max - min), or `\(s_i\)` is the standard deviation.

### Normal Equation

The "Normal Equation" is a method of finding the optimum theta without iteration.

`\[\theta = (X^T X)^{-1}X^T y\]`

Basically it means that we are solving the equation to find minimum. Additional info is [here](http://eli.thegreenplace.net/2014/derivation-of-the-normal-equation-for-linear-regression).

More info:
[https://www.coursera.org/learn/machine-learning](https://www.coursera.org/learn/machine-learning)

---
## Use WebSQL and IndexedDB in Typescript - 25 November, 2016
Tags: webSQL, indexedDb, typescript

More information about [IndexedDb](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API) or [WebSQL](https://www.w3.org/TR/webdatabase/).

Let's define base interfaces for our task:

```javascript
export interface IItem {
    id: string;
    value: string;
}

export interface IStorage<T extends IItem> {
    // Initial method to create storage
    init(name: string): Observable<IStorage<T>>;

    // Get the value by unique key
    get(key: string): Observable<T>;

    // Clear/remove all data in the storage
    clear(): Observable<T>;

    // Put specific value into the storage
    put(value: T): Observable<T>;

    // Get all values using the set of keys
    getDenseBatch(keys: string[]): Observable<T>;

    // Get all values from the storage
    all(): Observable<T>;
}
```

Here I am using [rxjs](http://reactivex.io/) to handle results. IItem is an interface for items which we are saving, IStorage is an interface for a specific storage.

### In Memory implementation

A short example how to implement mentioned interface using in-memory array:

```javascript
export class MemoryStorage<T extends IItem> implements IStorage<T> {
    private storage: { [key: string]: T } = {};

    init(name: string): Observable<MemoryStorage<T>> {
        return Observable.of(this);
    }

    get(key: string): Observable<T> {
        return Observable.of(this.storage[key]);
    }

    clear(): Observable<T> {
        this.storage = {};
        return Observable.empty<T>();
    }

    put(value: T): Observable<T> {
        if (!value.id) {
            value.id = Math.random().toString(36).substring(7);
        }
        this.storage[value.id] = value;
        return Observable.of(value);
    }

    getDenseBatch(keys: string[]): Observable<T> {
        return Observable.from(keys.map(x => this.storage[x]));
    }

    all(): Observable<T> {
        return Observable.from(Object.keys(this.storage).map(x => this.storage[x]));
    }
}
```

Simple implementation of IItem:

```javascript
class TestKeyValue implements IItem {
  public id: string;
  public value: string;
}
```

Unit tests for MemoryStorage:

```javascript
describe('MemoryStorage: Class', () => {
  let key1 = 'key1', key2 = 'key2';
  let value1 = 'value1', value2 = 'value2';

  function init(): MemoryStorage<TestKeyValue> {
    let storage = new MemoryStorage<TestKeyValue>();
    storage.init('test');
    return storage;
  }

  it('should create empty storage', async(() => {
    let storage = init();
    storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeTruthy());
  }));

  it('should save one item', async(() => {
    let storage = init();
    storage.put({ id: key1, value: value1 });
    storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeFalsy());
  }));

  it('should save/get one item', async(() => {
    let storage = init();
    let item = { id: key1, value: value1 };
    storage.put(item);
    storage.get(key1).subscribe(value => expect(value).toEqual(item));
  }));

  it('should save/get two items', async(() => {
    let storage = init();
    let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
    storage.put(items[0]);
    storage.put(items[1]);
    let i = 0;
    storage.getDenseBatch([key1, key2]).subscribe(value => expect(value).toEqual(items[i++]));
  }));

  it('should clear saved items', async(() => {
    let storage = init();
    let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
    storage.put(items[0]);
    storage.put(items[1]);

    storage.clear();
    storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeTruthy());
  }));
});
```

### WebSQL implementation

Current implementation just just for objects where key (string) is unique string value, value (string) is a payload.

```javascript
export class WebSQLStorage<T extends IItem> implements IStorage<T> {
    private db: Database;
    private databaseName: string = 'TripNoteDB';
    private name: string;

    constructor() {
        this.db = window.openDatabase(this.databaseName, '1.0', `Store information`, 40 * 1024 * 1024);
    }

    init(name: string): Observable<WebSQLStorage<T>> {
        this.name = name;
        return Observable.create((observer: Observer<WebSQLStorage<T>>) => {
            this.db.transaction(
                (tx) => tx.executeSql(`CREATE TABLE IF NOT EXISTS ${name} (key unique, value string)`,
                    [],
                    (t, results) => {
                        observer.next(this);
                        observer.complete();
                    },
                    (t, message) => {
                        observer.error(message.message.toString());
                        return true;
                    })
            );
        });
    }

    get(key: string): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.db.transaction((tx) => {
                tx.executeSql(`SELECT * FROM ${this.name} WHERE key='${key}'`, [],
                    (t, results) => {
                        let len = results.rows.length;
                        if (len === 0) {
                            observer.next(undefined);
                        } else if (len === 1) {
                            observer.next(results.rows.item(0));
                        } else {
                            observer.error('There should be no more than one entry');
                        }
                        observer.complete();
                    },
                    (t, message) => {
                        observer.error(message.message.toString());
                        return true;
                    });
            });
        });
    }

    clear() {
        return Observable.create((observer: Observer<T>) => {
            this.db.transaction((tx) => {
                tx.executeSql(`DELETE FROM ${this.name}`, [], (t, r) => observer.complete(), (t, e) => {
                    observer.error(e.message.toString());
                    return true;
                });
            });
        });
    }

    all(): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.db.transaction((tx) => {
                tx.executeSql(`SELECT * FROM ${this.name}`,
                    [],
                    (t, results) => {
                        for (let i = 0; i < results.rows.length; i++) {
                            observer.next(results.rows.item(i));
                        }
                        observer.complete();
                    },
                    (t, message) => {
                        observer.error(message.message.toString());
                        return true;
                    });
            });
        });
    }

    put(value: T): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.db.transaction((tx) => {
                tx.executeSql(`INSERT OR REPLACE INTO ${this.name} VALUES (?, ?)`, [value.id, value.value],
                    () => {
                        observer.next(value);
                        observer.complete();
                    },
                    (t, e) => {
                        observer.error(e.message.toString());
                        return true;
                    });
            });
        });
    }

    getDenseBatch(keys: string[]): Observable<T> {
        if (keys.length === 0) {
            return Observable.empty<T>();
        };

        return Observable.create((observer: Observer<T[]>) => {
            this.db.transaction((tx) => {
                let key = keys.map(x => '\'' + x + '\'').join(',');
                tx.executeSql(`SELECT * FROM ${this.name} WHERE key IN (${key})`,
                    [],
                    (t, results) => {
                        for (let i = 0; i < results.rows.length; i++) {
                            observer.next(results.rows.item(i));
                        }
                        observer.complete();
                    },
                    (t, e) => {
                        observer.error(e.message.toString());
                        return true;
                    });
            });
        });
    }
}
```

```javascript
describe('WebSQLStorage: Class', () => {
  let key1 = 'key1', key2 = 'key2';
  let value1 = 'value1', value2 = 'value2';

  it('should create empty storage', async(() => {
    let storage = new WebSQLStorage<TestKeyValue>();
    storage.init('test1').subscribe(() => {
      storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeTruthy());
    });
  }));

  it('should save one item ', async(() => {
    let storage = new WebSQLStorage<TestKeyValue>();
    storage.init('test2').subscribe(() => {
      storage.put({ id: key1, value: value1 }).subscribe(() => {
        storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeFalsy());
      });
    });
  }));

  it('should save/get one item', async(() => {
    let storage = new WebSQLStorage<TestKeyValue>();
    storage.init('test3').subscribe(() => {
      let item = { id: key1, value: value1 };
      storage.put(item).subscribe(() => {
        storage.get(key1).subscribe(value => {
          expect(value.value).toEqual(item.value);
        });

      });
    });
  }));

  it('should save/get two items', async(() => {
    let storage = new WebSQLStorage<TestKeyValue>();
    storage.init('test4').subscribe(() => {
      let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
      storage.put(items[0])
      .subscribe(() => storage.put(items[1])
        .subscribe(() => {
          let i = 0;
          storage.getDenseBatch([key1, key2])
            .subscribe(value => expect(value.value).toEqual(items[i++].value));
        }));
    });
  }));

  it('should clear saved items', async(() => {
    let storage = new WebSQLStorage<TestKeyValue>();
    storage.init('test5').subscribe(() => {
      let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
      storage.put(items[0])
        .zip(() => storage.put(items[1]))
        .subscribe(() => storage.clear()
        .subscribe(() => {
          storage.all().isEmpty().subscribe(isAny => expect(isAny).toBeTruthy());
        }));
    });
  }));
});
```

### IndexedDB implementation

How to use IndexedDB is [here](https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API/Using_IndexedDB).
There are very useful [tricks](https://www.codeproject.com/articles/744986/how-to-do-some-magic-with-indexeddb).

```javascript
export class IndexedDBStorage<T extends IItem> implements IStorage<T> {
    private databaseName: string = 'TripNoteDB';
    private name: string;

    private getDb(version?: number, storeName?: string): Observable<IDBDatabase> {
        return Observable.create((observer: Observer<number>) => {
            let req = version && version > 0 ? window.indexedDB.open(this.databaseName, version)
                : window.indexedDB.open(this.databaseName);
            req.onsuccess = (e) => {
                let db = (<any>event.target).result;
                observer.next(db);
                db.close();
                observer.complete();
            };
            req.onupgradeneeded = (e) => {
                let db = (<any>e.target).result;
                if (storeName && !db.objectStoreNames.contains(storeName)) {
                    db.createObjectStore(storeName, { keyPath: 'id' });
                    let transaction = (<any>e.target).transaction;
                    transaction.oncomplete = (event) => {
                        observer.next(db);
                        db.close();
                        observer.complete();
                    };
                };
            };
            req.onblocked = (event) => observer.error('IndexedDB is blocked');
            req.onerror = (e) => observer.error(e.error);
        });
    }

    private getVersionOfDb(name: string): Observable<number> {
        return this.getDb().map(db => {
            if (!db.objectStoreNames.contains(this.name)) {
                return db.version + 1;
            } else {
                return db.version;
            }
        });
    }

    init(name: string): Observable<IndexedDBStorage<T>> {
        this.name = name;
        return Observable.create((observer: Observer<IndexedDBStorage<T>>) => {
            this.getVersionOfDb(name).subscribe((version) => {
                this.getDb(version, name).subscribe(db => {
                    observer.next(this);
                    observer.complete();
                });
            });
        });
    }

    all(): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.getDb().subscribe(db => {
                let req = db.transaction(this.name, 'readwrite').objectStore(this.name)
                    .openCursor();
                req.onsuccess = (e) => {
                    let res = (<any>event.target).result;
                    if (res) {
                        observer.next(res.value);
                        res.continue();
                    }
                    observer.complete();
                };
                req.onerror = (e) => observer.error(e.error);
            });
        });
    }

    get(key: string): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.getDb().subscribe(db => {
                let req = db.transaction(this.name).objectStore(this.name).get(key);
                req.onerror = (e) => observer.error(e.error);
                req.onsuccess = (e) => {
                    observer.next(req.result);
                    observer.complete();
                };
            });
        });
    }

    clear(): Observable<IStorage<T>> {
        return Observable.create((observer: Observer<IStorage<T>>) => {
            this.getDb().subscribe(db => {
                let req = db.transaction(this.name, 'readwrite').objectStore(this.name).clear();
                req.onerror = (e) => observer.error(e.error);
                req.onsuccess = (e) => {
                    observer.next(this);
                    observer.complete();
                };
            });
        });
    }

    put(value: T): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.getDb().subscribe(db => {
                let req = db.transaction(this.name, 'readwrite').objectStore(this.name).put(value);
                req.onerror = (e) => {
                    observer.error(e.error);
                };
                req.onsuccess = (e) => {
                    observer.next(value);
                    observer.complete();
                };
            });
        });
    }

    getDenseBatch(keys: string[]): Observable<T> {
        return Observable.create((observer: Observer<T>) => {
            this.getDb().subscribe(db => {
                let set = keys.sort();
                let i = 0;
                let req = db.transaction(this.name).objectStore(this.name)
                    .openCursor();
                req.onsuccess = (e) => {
                    let cursor = (<any>event.target).result;
                    if (!cursor) { observer.complete(); return; }
                    let key = cursor.key;
                    while (key > set[i]) {
                        // The cursor has passed beyond this key. Check next.
                        ++i;
                        if (i === set.length) {
                            // There is no next. Stop searching.
                            observer.complete();
                            return;
                        }
                    }
                    if (key === set[i]) {
                        // The current cursor value should be included and we should continue
                        // a single step in case next item has the same key or possibly our
                        // next key in set.
                        observer.next(cursor.value);
                        cursor.continue();
                    } else {
                        // cursor.key not yet at set[i]. Forward cursor to the next key to hunt for.
                        cursor.continue(set[i]);
                    }
                };
                req.onerror = (e) => observer.error(e.error);
            });
        });
    }
}
```

Unit tests for indexedDB:

```javascript
describe('IndexedDBStorage: Class', () => {
  let key1 = 'key1', key2 = 'key2';
  let value1 = 'value1', value2 = 'value2';

  it('should create empty storage', (done) => {
    let storage = new IndexedDBStorage<TestKeyValue>();
    storage.init('test1').subscribe(() => {
      storage.all().isEmpty().subscribe(isAny => {
        expect(isAny).toBeTruthy();
        done();
      });
    });
  });

  it('should save one item ', (done) => {
    let storage = new IndexedDBStorage<TestKeyValue>();
    storage.init('test2').subscribe(() => {
      storage.put({ id: key1, value: value1 }).subscribe(() => {
        storage.all().isEmpty().subscribe(isAny => {
          expect(isAny).toBeFalsy();
          done();
        });
      });
    });
  });

  it('should save/get one item', (done) => {
    let storage = new IndexedDBStorage<TestKeyValue>();
    storage.init('test3').subscribe(() => {
      let item = { id: key1, value: value1 };
      storage.put(item).subscribe(() => {
        storage.get(key1).subscribe(value => {
          expect(value).toEqual(item);
          done();
        });
      });
    });
  });

  it('should save/get two items', (done) => {
    let storage = new IndexedDBStorage<TestKeyValue>();
    storage.init('test4').subscribe(() => {
      let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
      let item1 = storage.put(items[0])
        .merge(storage.put(items[1])).last()
        .subscribe(y => {
          storage.getDenseBatch([key1, key2]).toArray().subscribe(x => {
            expect(x[0]).toEqual(items[0]);
            expect(x[1]).toEqual(items[1]);
            done();
          });
        });
      });
  });

  it('should clear saved items', (done) => {
    let storage = new IndexedDBStorage<TestKeyValue>();
    storage.init('test5').subscribe(() => {
      let items = [{ id: key1, value: value1 }, { id: key2, value: value2 }];
      storage.put(items[0])
        .merge(storage.put(items[1])).last()
        .subscribe(x => storage.clear()
          .subscribe(y => storage.all().isEmpty().subscribe(isAny => {
              expect(isAny).toBeTruthy();
              done();
            })));
    });
  });
});
```

---
## Machine Learning Init. Linear Regression. Gradient Descent - 14 November, 2016
Tags: machine learning

The information is from this [course](https://www.coursera.org/learn/machine-learning/).

[Machine learning](https://en.wikipedia.org/wiki/Machine_learning) is the subfield of computer science that "gives computers the ability to learn without being explicitly programmed" (Arthur Samuel, 1959).

In general, any machine learning problem can be assigned to one of two broad classifications:
* supervised learning ("regression" and "classification")
* unsupervised learning

In supervised learning, we are given a data set and already know what our correct output should look like, having the idea that there is a relationship between the input and the output.

Unsupervised learning, on the other hand, allows us to approach problems with little or no idea what our results should look like. We can derive structure from data where we don't necessarily know the effect of the variables.

### [Linear Regression](https://en.wikipedia.org/wiki/Linear_regression) with One Variable

In statistics, linear regression is an approach for modeling the relationship between a scalar dependent variable y and one or more explanatory variables (or independent variables) denoted X. The case of one explanatory variable is called simple linear regression. For more than one explanatory variable, the process is called multiple linear regression.

![Linear regression](https://upload.wikimedia.org/wikipedia/commons/thumb/3/3a/Linear_regression.svg/438px-Linear_regression.svg.png)

 Hypothesis function has the general form:
`\[\hat{y} = h_\theta(x) = \theta_0 + \theta_1 x\]`

### Cost Function

We can measure the accuracy of our hypothesis function by using a cost function. This takes an average (actually a fancier version of an average) of all the results of the hypothesis with inputs from x's compared to the actual output y's.

`\[J(\theta_0, \theta_1) = \dfrac {1}{2m} \displaystyle \sum _{i=1}^m \left ( \hat{y}_{i}- y_{i} \right)^2  = \dfrac {1}{2m} \displaystyle \sum _{i=1}^m \left (h_\theta (x_{i}) - y_{i} \right)^2\]`

If we try to think of it in visual terms, our training data set is scattered on the x-y plane. We are trying to make straight line (defined by `\(h_\theta(x)\)`) which passes through this scattered set of data. Our objective is to get the best possible line. The best possible line will be such so that the average squared vertical distances of the scattered points from the line will be the least. In the best case, the line should pass through all the points of our training data set. In such a case the value of `\(J(\theta_0, (\theta_1\)`) will be 0.

### Gradient Descent

There is hypothesis function and there are a set of {x, y} values, so we need to find `\(\theta_0\)` and `\(\theta_1\)`.

The gradient descent algorithm is:

`\[\theta_j := \theta_j - \alpha \frac{\partial}{\partial \theta_j} J(\theta_0, \theta_1)\]`

### Gradient Descent for Linear Regression

`\[\begin{align*}
  \text{repeat until convergence: } \lbrace & \newline 
  \theta_0 := & \theta_0 - \alpha \frac{1}{m} \sum\limits_{i=1}^{m}(h_\theta(x_{i}) - y_{i}) \newline
  \theta_1 := & \theta_1 - \alpha \frac{1}{m} \sum\limits_{i=1}^{m}\left((h_\theta(x_{i}) - y_{i}) x_{i}\right) \newline
  \rbrace&
  \end{align*}\]`

---
## Xamarin Dev Days in Warsaw - 24 September, 2016
Tags: xamarin, dev days

Hi All, it is my overview of Xamarin Dev Days - short summary. Hope it will be useful to create an  impression what you can expect from this event.

My stream technology is server-side .NET or ASP.NET MVC. My current project is on .NET platform, the main technology is Windows Workflow Foundation and I am not using Xamarin platform. But I had a small experience in Xamarin that I got during working on my previous project.

Some time ago I had a chance to visit Xamarin Dev Days event in Warsaw. It was one day event in Warsaw Microsoft office. There were two part: the first one is generic information about Xamarin ecosystem and Azure Mobile Apps. The second one is hands on lab. So [this presentation](https://docs.google.com/presentation/d/1pbsv4otZvU88ABx4QSgyznxpqolMQ279QsjWh3WvPjQ/edit?usp=sharing) has been created to summarize and do a short recap mentioned event.

### Agenda:
* Introduction to Xamarin
* Xamarin.Forms
* Azure Mobile Apps
* Sample application
* Resources

![Xamarin Platform](./../images/xam_dev_days_platform.png "Xamarin Platform")

The first thing that you should keep in mind when you are talking about Xamarin is that you are able to write application not using Visual Studio on Windows only, but also [Xamarin Studio](https://www.xamarin.com/studio) on Mac. As I understand, usually people are using Visual Studio to develop Android and Phone applications and Xamarin Studio on Mac to develop IOS application. But, of course, the team is using one code base. The reason of this approach, that it is much more quicker to build IOS application on MAC directly, without remote building using Visual Studio on Windows.

The second thing is that Xamarin is not only about developing cross-platform applications using Visual Studio or Xamarin Studio, but Xamarin also provides possibility to test you application on various number of devices using [Xamarin Cloud](https://www.xamarin.com/test-cloud). As you can imagine, it is very expensive to buy all set of mobile devices for testing, so this cloud allows you to run your application on thousand of real devices in the cloud, analyze detailed test reports with results, screenshots, and performance metrics. Also it allows you to measure performance of your application. Sounds very cool, but there was a question from Xamarin Dev Days presenter about if there is someone who is using this cloud and nobody answered.

The third thing is about building and continuous integration. Actually it is not about Xamarin but about [TFS](https://www.visualstudio.com/tfs/)(Team Foundation Server). You are able to install it on your private server or it is possible to use [Visual Studio Online](https://www.visualstudio.com/) service from Microsoft. It is free for small teams. It provides opportunity to work with your code (git), use agile board to organize your work, set up your continuous integration project (pure version of team city), create test cases. I heard that they added Wiki also.

The last one is about distributing and monitoring. I can’t say a lot about distributing as I have never put any application to stores. As for monitoring, there is Xamarin Insight. It is the same approach as [Visual Studio Application Insights](https://www.visualstudio.com/en-us/docs/insights/application-insights). It is an extensible analytics service that helps you understand the performance and usage of your live mobile application. It's designed for developers, to help you continuously improve the performance and usability of your app. It [allows](http://www.joesauve.com/xamarin-insights-after-only-5-minutes-its-already-saving-my-arse/):

* see user sessions in real time
* see which users are being affected by which errors
* see stacktraces for each exception
* see device stats for each exception (operating system, app version, network status, device orientation, jalbreak status, and bluetooth status)
* see advanced reporting and filtering of aggregate exception statistics
* setup webhooks for triggering actions on certain Insights events
* integrate with third-party services (Campire, Github, HipChat, Jira, PivotalTracker, and Visual Studio Online)

### Approaches:
* Separate solutions for each platform (Android, IOS, Windows)
    * Many projects, many languages, many teams
* One universal solution (JS + HTML + CSS) - Cordova
    * Slow performance, limited native API
* Xamarin approach (shared code + platform specific UI)
    * Good performance, almost all native API
* ReactNative and NativeScript
    * Only Android and IOS (UWP in future)

Here I want to show the main ways to build mobile application.
    
1. Of course, firstly it is native apps. It is clear. Swift, object-C for IOS, java is for Android, C# is  for Windows Phone. It means you should have and support many projects and many teams. It is a good option if you are planning to build complex and big mobile application. The best scenario is if this application has only mobile version.
2. Universal solution. You are able to use Cordova and build you application using JavaScript. Personally I really like this approach as you are able to build almost any type of application using Javascript now. To execute javascript on server - NodeJs. For desktop application there is Electron framework. Cordova is to create mobile applications. The problem here is performance. The resulting applications are hybrid, meaning that they are neither truly native mobile application (because all layout rendering is done via Web views instead of the platform's native UI framework) nor purely Web-based (because they are not just Web apps, but are packaged as apps for distribution and have access to native device APIs). [[link](https://en.wikipedia.org/wiki/Apache_Cordova)]
3. And Xamarin. It looks like win-win strategy if you already have web or desktop application written on .NET. You are able to share code, get native performance (almost, depends how you are creating application, Xamarin.Forms, for example, can create non the best implementation), access to all native API (almost). If there is a new version of OS, need to wait implementation in Xamarin up to one month.
4. [ReactNative](https://www.reactnative.com/) and [NativeScript](https://www.nativescript.org/) created and supported by Facebook and Telerik. ReactNative hasn’t final version still, but NativeScript has version 2.0. They are the most young libraries in the list. JavaScript is the language to write a code. But unlike Cordova transform Javascript elements to native UI elements. Support Android and IOS now. Microsoft is working to add UWP here (NativeScript). Looks like the most perspective platforms. You are able to use [Angular2 + Typescript + NativeScript or ReactJs + ReactNative](https://www.quora.com/What-are-the-key-difference-between-ReactNative-and-NativeScript/answer/Valentin-Stoychev) to write mobile applications and share code also with your web version of application. Probably it is the best frameworks if your application is web first.

### [Xamarin features](https://www.xamarin.com/download-it)

* Produce ARM binary for Apple store
* Produce APK for Android
* Possibility to use only one IDE (Visual Studio)
* [Android Hyper-V]("https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/debug-on-emulator/visual-studio-android-emulator/) and [IOS Remote emulators](https://developer.xamarin.com/guides/cross-platform/windows/ios-simulator/)
* Designers for IOS, Android and Windows Phone in Visual Studio
* Xamarin Studio for Mac
* [MVVM pattern](https://msdn.microsoft.com/en-us/library/hh848246.aspx) (XAML)

Here I put the main interesting features of Xamarin:

1. Xamarin [allows](https://www.xamarin.com/download-it#ios) to ship native app bundles on the App Store. Ahead-of-Time (AOT) compiler compiles Xamarin.iOS apps directly to native ARM assembly code, meaning your app is a native platform binary.
2. As for Android, Xamarin.Android uses just-in-time compilation for sophisticated runtime optimization of your app’s performance, meaning your app is a native Android APK.
3. Also I talked that we are able to use only one IDE to develop applications in theory. 
4. Microsoft Visual Studio 2015 includes an Android emulator that you can use as a target for debugging your Xamarin.Android app: Visual Studio Emulator for Android. This emulator uses the Hyper-V capabilities of your development computer, resulting in faster launch and execution times than the default emulator that comes with the Android SDK. Also it is possible to debug IOS in Windows using remote simulator.
5. Visual Studio supports also visual designers to build UI. It works almost the same like it works for WPF.
6. Also there is [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/).
7. The Model-View-ViewModel (MVVM) architectural pattern was invented with XAML in mind. The pattern enforces a separation of the XAML user interface (the View) from the underlying data (the Model) through a class that serves as an intermediary between the View and the Model (the ViewModel). The View and the ViewModel are often connected through data bindings defined in the XAML file. The BindingContext for the View is usually an instance of the ViewModel. [link](https://developer.xamarin.com/guides/xamarin-forms/xaml/xaml-basics/data_bindings_to_mvvm/)

### [Sharing code](https://developer.xamarin.com/guides/cross-platform/application_fundamentals/building_cross_platform_applications/sharing_code_options/)
* Portable class library
* Shared projects
* Upcoming [.NET Standard 2.0](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/)

Currently there are two ways to write shared code in Xamarin:

1. Shared project. Unlike most other project types, a Shared Project has no 'output' assembly. During compilation, the files are treated as part of the referencing project and compiled into that DLL. If you wish to share your code as a DLL then Portable Class Libraries are a better solution. Shared code can be branched based on the platform using compiler directives (eg. using #if \__ANDROID__ , as discussed in the Building Cross Platform Applications document).
2. Portable library. Only a subset of the .NET framework is available to use, determined by the profile selected (see the Introduction to PCL for more info).
Upcoming .net standard 2.0 will support Xamarin. Basically Microsoft introduced a new [.NET Standard Library](https://docs.microsoft.com/en-us/dotnet/articles/standard/library). The .NET Standard Library is a formal specification of .NET APIs that are intended to be available on all .NET runtimes. The motivation behind the Standard Library is establishing greater uniformity in the .NET ecosystem.

### [Xamarin plugins](https://developer.xamarin.com/guides/xamarin-forms/platform-features/plugins/) (nuget)
* Battery
* Connectivity
* Geolocation
* Media
* Settings
* Text to speech
* ...

Another one nice feature of Xamarin is Xamarin [plugins](https://github.com/xamarin/XamarinComponents) that can be downloaded using nuget. These libraries allow you to use functionality that adds cross-platform functionality or abstracts platform specific functionality to a common API, like battery, geolocation, media and so on.

You are able to find the whole list of plugins [here](https://components.xamarin.com/?category=plugins).

### [Xamarin.Forms](https://developer.xamarin.com/guides/xamarin-forms/)
* Shared UI 
* [Pages, layouts, controls](https://developer.xamarin.com/guides/xamarin-forms/controls/)
* [Two-ways data binding](https://blog.xamarin.com/introduction-to-data-binding/)
* [Navigation](https://developer.xamarin.com/guides/xamarin-forms/user-interface/navigation/)
* [Animation](https://blog.xamarin.com/creating-animations-with-xamarin-forms/)
* [Dependency service](https://developer.xamarin.com/guides/xamarin-forms/dependency-service/), [messaging center](https://developer.xamarin.com/guides/xamarin-forms/messaging-center/)
* [Xamarin.Forms 2.0](https://developer.xamarin.com/releases/xamarin-forms/xamarin-forms-2.0/2.0.0/) - Performance
* [Themes](https://developer.xamarin.com/guides/xamarin-forms/themes/)
* [Data pages](https://developer.xamarin.com/guides/xamarin-forms/datapages/)
* [Native embedding](https://developer.xamarin.com/guides/xamarin-forms/user-interface/layouts/add-platform-controls/)

Use the Xamarin.Forms API provides a way to quickly build native apps for iOS, Android and Windows completely in C#. Xamarin.Forms is included with Visual Studio.

1. Xamarin allows sharing not only code between platforms, but also UI. During compilation created abstract XAML is transformed to platform specific. For example, there is the next XAML element: Entry, what is TextBox in Windows Phone. So we have an platform-specific equivalent for every control from Xamarin.Form.
2. There are three main groups in Xamarin.Form:
    * A Xamarin.Forms.Page represents a View Controller in iOS or a Page in Windows Phone. On Android each page takes up the screen like an Activity, but Xamarin.Forms Pages are not Activities.
    * The Layout class in Xamarin.Forms is a specialized subtype of View, which acts as a container for other Layouts or Views. It typically contains logic to set the position and size of child elements in Xamarin.Forms applications.
    * Xamarin.Forms uses the word View to refer to visual objects such as buttons, labels or text entry boxes - which may be more commonly known as controls of widgets.
3. Data binding connects two objects, called the source and the target. The source object provides the data. The target object, which must be a bindable property, will consume (and often display) data from the source object.
4. Xamarin.Forms provides a number of different page navigation experiences, depending upon the Page type being used: Tabbed Page, Hierarchical Navigation, CarouselPage and so on.
5. Xamarin.Forms includes its own animation infrastructure that allows for easy creation of simple animations, while also being versatile enough to create complex animations. The Xamarin.Forms animation classes target different properties of visual elements, with a typical animation progressively changing a property from one value to another over a period of time.
6. Xamarin.Forms allows developers to define behavior in platform-specific projects. DependencyService then finds the right platform implementation, allowing shared code to access the native functionality.
7. Xamarin.Forms MessagingCenter enables view models and other components to communicate with without having to know anything about each other besides a simple Message contract.
8. Native Embedding. Platform-specific controls can be directly added to a Xamarin.Forms layout. It is possible to add platform-specific controls to a Xamarin.Forms layout, and how to override the layout of custom controls in order to correct their measurement API usage.

### [Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-value-prop/)

![Azure Mobile](./../images/azure_mob_structure.png)

* Stable backend
* Offline sync
* Data storage (tables, sql)
* Authentication (Microsoft, Google, Facebook, Twitter)
* Push notifications

Azure App Service is a fully managed Platform as a Service (PaaS) that brings a rich set of capabilities to web, mobile and integration scenarios.

1. Build offline-ready apps with data sync.
2. Push Notifications allows you to engage your clients with instant push notifications on any device.
3. Authorization using different approaches
4. Save data.

### Take a look
* [App Service Helpers](https://github.com/MikeCodesDotNet/App-Service-Helpers)
* [Contoso Moments](https://github.com/azure-appservice-samples/ContosoMoments)

Want to mention two nice libraries/ applications:

1. App Service Helpers (ASH) makes it as easy as possible to add data storage and authentication to your mobile app with Microsoft's Azure App Service Platform. ASH was built with the mobile developer in mind, and requires no previous experience with backends as a service (BaaS). This lib was developed as a supplemental library to Microsoft's Azure Client SDK. Rather than replacing this library, ASH extends it by lowering the barrier to entry for developers who wish to build cloud-connected mobile apps in C#.
2. It is a good example how to use Azure Mobile services. Contoso Moments is a photo sharing application that demonstrates the following features of Azure App Service:
    * App Service authentication/authorization
    * Continuous Integration and deployment
    * Mobile app server SDK
    * Mobile offline sync client SDK
    * Mobile file sync SDK
    * Mobile push notifications

### Sample application

I have created a small sample application. The source code is available [here](./../assets/WakeUp.zip). It is simple carousel application that shows information about morning exercises.  

### References:
There are a list of references and interesting information about Xamarin. Thank you for reading. [Please feel free to ask](mailto:gromkaktus@gmail.com) any questions, I will try to answer.

* [Github dev-days-labs](https://github.com/xamarin/dev-days-labs)
* [Xamarin Dev Days](https://www.xamarin.com/dev-days)
* [Xamarin Warsaw Mobile Developers Group](http://www.meetup.com/warsawmobiledevelopers/)
* [Xamarin Training Series](https://confluence.infusion.com/pages/viewpage.action?pageId=28582107)
* [Xamarin bootcamp training](https://confluence.infusion.com/display/innovationpractices/Xamarin+Bootcamp+Training)
* [Xamarin Dev page](https://developer.xamarin.com/)
* [Xamarin Dev days - presentation](https://docs.google.com/presentation/d/1pbsv4otZvU88ABx4QSgyznxpqolMQ279QsjWh3WvPjQ/edit?usp=sharing)

---
## GitVersion + TC - 04 February, 2016
Tags: git

GitVersion is the utility to set up version of deployed assemblies using information from Git.

There is an easy way to set up it on your TC build server. Go to MetaRunner and download MR_GitVersion3.xml file. You should put this file to your build server. The path is C:\ProgramData\JetBrains\TeamCity\config\projects\{Project}\pluginData\metaRunners. The first part of this part you can find in Global settings of Team City. Also you have to restart your server after this.

So you will be able to set up GitVersion build step which just gran information about your version from Git tags and put it to your assemblies.

TC to build only commits with tags (RC-*). VCS Root set up:

![Image One](./../images/git_version_step3.PNG)
![Image Two](./../images/git_version_step3_2.PNG)

Trigger:

![Image Three](./../images/git_version_step3_3.PNG)

---
## Notes from 'Introduction to Linux' - 21 May, 2015
Tags: linux

[course link](https://www.edx.org/course/introduction-linux-linuxfoundationx-lfs101x-0)

Linux [Filesystem](http://www.tldp.org/LDP/sag/html/filesystems.html)

* Conventional disk filesystems: ext2, ext3, ext4, XFS, Btrfs, JFS,NTFS, etc.
* Flash storage filesystems: ubifs, JFFS2, YAFFS, etc.
* Database filesystems
* Special purpose filesystems: procfs, sysfs, tmpfs, debugfs, etc.

[Partitions](https://en.wikipedia.org/wiki/Disk_partitioning)

1. Windows
    * Partition: Disk1 
    * Filesystem type: NTFS/FAT32
    * Mounting Parameters: DriveLetter 
    * Base Folder of OS: C drive
2. Linux
    * Partition: /dev/sda1
    * Filesystem type: EXT3/EXT4/XFS
    * Mounting Parameters: MountPoint
    * Base Folder of OS: /

[The Filesystem Hierarchy Standard](https://en.wikipedia.org/wiki/Filesystem_Hierarchy_Standard)

![File System](./../images/linux_foundation_filesystem.jpg)

The Boot Process

![Boot Process](./../images/linux_foundation_boot_process.jpg)

Choosing a [Linux Distribution](https://en.wikipedia.org/wiki/Linux_distribution)

![Choose](./../images/linux_foundation_choose.jpg)

---
## Task: Show information about types in tooltip - 01 November, 2014
Tags: javascript

The value of a point is the sum of values of types ('one', 'two').
[Link](http://jsfiddle.net/yo4L215v/) - 26 December, 2014

[Additional info](http://api.highcharts.com/highcharts#tooltip.formatter)

![image](./../images/highchart_add_info.png)

The code:
```javascript
$(function () {
    $('#container').highcharts({
        chart: {
            type: 'column'
        },
 
        tooltip: {
            formatter: function () {
                var typeInf = '';
                var types = this.point.Types;
                $.each(types, function (index) {
                    typeInf += '<b>' + types[index][0] + '</b> ' + types[index][1] + '<br/>';
                });
 
                return 'The value for <b>' + this.x +
                    '</b> is <b>' + this.y + '</b><br/>' + typeInf;
            }
        },
 
        xAxis: {
            categories: ['Green', 'Pink']
        },
 
        series: [{
            data: [{
                name: 'Point 1',
                Types: [["one", 1], ["two", 1]],
                color: '#00FF00',
                y: 2
            }, {
                name: 'Point 2',
                Types: [["one", 2], ["two", 3]],
                color: '#FF00FF',
                y: 5
            }]
        }]
    });
});
```

---
## WinRT Checksum for large files - 29 September, 2014
Tags: winRT, .net

There is a good [question](http://stackoverflow.com/questions/13534334/how-to-compute-hash-md5-or-sha-of-a-large-file-with-c-sharp-in-windows-store-a) about this.

Also there is a code. I have used MD5 algorithm for my purposes.
```csharp
  public async Task<string> GetFileChecksumAsync(string fileName)
  {
   HashAlgorithmProvider alg = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
   IStorageFile stream = await openFile(fileName);
   using (var inputStream = await stream.OpenReadAsync())
   {
    Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer(BUFFER_SIZE);
    var hash = alg.CreateHash();

    while (true)
    {
     await inputStream.ReadAsync(buffer, BUFFER_SIZE, InputStreamOptions.None);
     if (buffer.Length > 0)
      hash.Append(buffer);
     else
      break;
    }

    return CryptographicBuffer.EncodeToHexString(hash.GetValueAndReset()).ToUpper();
   }
  }
```

---
## FuelUX Tree additional behavior - 14 July, 2014
Tags: javascript

Add "Show All" to the FuelUX Tree like this.

![example](./../images/fuelux-tree.gif)

There is the [tree plugin](https://exacttarget.github.io/fuelux/#tree). You can add the next code after initialization of the tree:
```javascript
scope.find('#MyTree').on('selected', function (event, data) {
    if (data.target.additionalParameters.id == 0) {
        scope.find('#MyTree').find('.tree-item').removeClass('tree-selected').find('i').removeClass('icon-ok').addClass('tree-dot');
        scope.find('#MyTree').find('.tree-item:eq(1)').addClass('tree-selected').find('i').removeClass('tree-dot').addClass('icon-ok');
        scope.find('#MyTree').find('.tree-folder-header> i.icon-ok').remove();
    }
    else {
        scope.find('#MyTree').find('.tree-item:eq(1)').removeClass('tree-selected').find('i').removeClass('icon-ok').addClass('tree-dot');
        if (data.target.additionalParameters.type == 'anyOther') {
            data.element.closest('.tree-folder-content').find('.tree-item:gt(0)').removeClass('tree-selected').find('i').removeClass('icon-ok').addClass('tree-dot');
        }
    }
    
});
```

---
## Fixed header or column for HTML table using JQuery - 07 July, 2014
Tags: javascript

I have created one more plugin for HTML table. It fixes the head of a table on the page. [Please see it](https://bitbucket.org/upyl/fixedheader). The main feature is supporting of overflow parent element.

Some images of plugin:

![example1](./../images/fixed_header1.png)

And another plugin https://bitbucket.org/upyl/fixedcolumn to fix column of table.

![example2](./../images/fixed_header2.png)

---
## Light version of Monodruid - 04 June, 2014
Tags: .net, xamarin

I have created the light version of [MonoDroid Unit Testing framework](https://bitbucket.org/mayastudios/monodroid-unittest/wiki/Home).

Please [look](https://bitbucket.org/upyl/mono-unitetsting-light) and use if you need.

---
## Bootstrap + Recaptcha. Css problem - 07 June, 2013
Tags: bootstrap, javascript

If you use bootstrap and recaptcha, you may face a problem like wrong padding|margin:

You should add next css style to yours:

```css
body{ line-height:1}
```

And all will be OK ! Enjoy!

---
## Bootstrap datepicker Week mode view - 28 July, 2013
Tags: javascript, Bootstrap

I have updated bootstrap datepicker to new view mode: week.

The source code is [here](./../code/bootstrap-datepicker.js)

---
## ASP.NET MVC and Html.Hidden - 01 April, 2013
Tags: .net, asp.net mvc

There is interesting bug connected with Html.Hidden.
There are two model:
```csharp
public class  Model1{
public int ID{get;set;}
public Model2 Model2Model{get;set;}
}
public class Model2{
public int ID{get;set;}
}
```
Page:
```html
<html>
...
<body>
@Html.Partial("partial"
<body>
</html>
```

---
## SMO Scripter. Create script of DB - 25 June, 2013

It is possible to create script of MSSQL using SMO: ([link](http://pastebin.com/AQkprTS7#))

```csharp
private static void Main(string[] args)
{
    var arguments = args.Select(x => x.ToLower()).ToList();
    if (arguments.Count == 0 || arguments.Contains("-help"))
    {
        Console.WriteLine("-d - Database Name");
        Console.WriteLine("-i - Output Sql File Name");
        Console.WriteLine("-s - Server Instance");
        Console.WriteLine("-u - User Name");
        Console.WriteLine("-p - Password");
        Console.WriteLine("It should be 'tables.txt' file in folder with names of tables to script; if it does not exist the application scripts all tables with prefix 'GMP_'");
        Console.ReadKey();
    }
    else if (arguments.Count > 1 && arguments.Contains("-d") && args.Contains("-i"))
    {
        if (arguments.Count <= arguments.IndexOf("-d") + 1)
        {
            throw new ArgumentException("Database Name");
        }
        if (arguments.Count <= argumSMO Scripter. Create script of DBents.IndexOf("-i") + 1)
        {
            throw new ArgumentException("Output Sql File Name");
        }
        var dbName = arguments[arguments.IndexOf("-d") + 1];
        var outputFileName = arguments[arguments.IndexOf("-i") + 1];
        var srv = new Server();
        if (arguments.Contains("-s") && args.Contains("-u") && arguments.Contains("-p"))
        {
            if (arguments.Count <= arguments.IndexOf("-s") + 1)
            {
                throw new ArgumentException("Server Instance");
            }
            if (arguments.Count <= arguments.IndexOf("-u") + 1)
            {
                throw new ArgumentException("User Name");
            }
            if (arguments.Count <= arguments.IndexOf("-p") + 1)
            {
                throw new ArgumentException("Password");
            }
            var connection = new ServerConnection(arguments[arguments.IndexOf("-s") + 1], arguments[arguments.IndexOf("-u") + 1], arguments[arguments.IndexOf("-p") + 1]);
            srv = new Server(connection);
        }
        // read names of tables
        var tablesFromFile = new List<string>();
        if (File.Exists("tables.txt"))
        {
            using (var file = File.OpenText("tables.txt"))
            {
                while (file.Peek() > 0)
                {
                    tablesFromFile.Add(file.ReadLine());
                }
            }
        }
        Database db = srv.Databases[dbName];
        var dropKeys = new Scripter(srv) {Options = {ScriptDrops = true, IncludeIfNotExists = true, DriForeignKeys = true}};
        var listOfScripts = new List<Scripter>
                            {
                                new Scripter(srv) {Options = {ScriptDrops = true, IncludeIfNotExists =true, DriAllKeys = false}},
                                new Scripter(srv) {Options = {ScriptDrops = false, ScriptSchema = true, WithDependencies = false, DriIndexes = true, DriClustered = true, IncludeIfNotExists = true, DriAllKeys = false}},
                                new Scripter(srv) {Options = {ScriptDrops = false, ScriptSchema = true, DriDefaults = false, DriIndexes = false, DriPrimaryKey = false, DriClustered = false, Default = false, DriAll =false, DriForeignKeys = true, IncludeIfNotExists = true, DriAllKeys = false}},
                                new Scripter(srv) {Options = {DriIndexes = true, Default = true, DriDefaults = true, DriClustered = false, IncludeIfNotExists = true, DriAll = true, DriAllConstraints = true, DriAllKeys = true, SchemaQualify = true, SchemaQualifyForeignKeysReferences = true, NoCollation = true}}
                            };
        using (var file = File.CreateText(outputFileName))
        {
            foreach (Table tb in db.Tables)
            {
                if ((tablesFromFile.Count > 0 && tablesFromFile.Contains(tb.Name) || (tablesFromFile.Count== 0 && tb.Name.StartsWith("GMP_"))))
                {
                    if (tb.IsSystemObject == false)
                    {
                        foreach (ForeignKey foreignKey in tb.ForeignKeys)
                        {
                            System.Collections.Specialized.StringCollection scd = dropKeys.Script(new[] {foreignKey.Urn });
                            foreach (string st in scd)
                            {
                                file.WriteLine(st);
                                file.WriteLine("GO");
                            }
                        }
                        file.WriteLine();
                    }
                }
            }
            foreach (var script in listOfScripts)
            {
                foreach (Table tb in db.Tables)
                {
                    if ((tablesFromFile.Count > 0 && tablesFromFile.Contains(tb.Name) ||(tablesFromFile.Count == 0 && tb.Name.StartsWith("GMP_"))))
                    {
                        if (tb.IsSystemObject == false)
                        {
                            System.Collections.Specialized.StringCollection scd = script.Script(new[] {tb.Urn });
                            foreach (string st in scd)
                            {
                                file.WriteLine(st);
                                file.WriteLine("GO");
                            }
                            file.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
```

---
## TFS Exclude binding from solution - 24 April, 2013
Tags: TFS

It is not easy command to unbind solution/project of TFS.

But there is manual actions to do that:
1. Remove all *.vssscc and etc(Source Control files near your solution and projects file);
2. Remove all nodes in solution and project file with tag <scc />

---
## Structure of presentation - 10 April, 2013

[link](http://burba.pro/presentation_structure/)

![image](./../images/presentation_structure.png)

---
## Persistence.js insert empty values in web sql - 04 April, 2013
Tags: javascript

I am starting to work with persistence.js library and open the problem to me: it saves empty data to web sql

![example1](./../images/persistance1.png)

After investigating the problem i have found this question  and just want to clear and reproduce it in my notes. Thanks guys from this question :)
The problem connected with this js file: persistence.jquery. If it used, we should rewrite code such as:

![example2](./../images/persistance2.png)

Thanks.

---
## Useful LINQ extensions - 26 April, 2013
Tags: .net, linq

After reading a lot of articles about this theme, I start to use the next [extensions](http://pastebin.com/As5as2pE)

```csharp
public static class Linq
{
    public static IEnumerable<T> Except<T>(this IEnumerable<T> source, IEnumerable<T> target, Func<T, T, bool> func)
    {
        return source.Except(target, new LambdaComparer<T>(func));
    }

    public static TResult With<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator)
        where TInput : class
    {
        if (o == null) return default(TResult);
        return evaluator(o);
    }

    public static TResult Return<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator, TResult failure_value) where TInput : class
    {
        if (o == null) return failure_value;
        return evaluator(o);
    }

    public static bool Check<TInput>(this TInput o, Func<TInput, bool> evaluator) where TInput : class
    {
        if (o == null) return false;
        return evaluator(o);
    }

    public static TInput If<TInput>(this TInput o, Func<TInput, bool> evaluator) where TInput : class
    {
        if (o == null) return null;
        return evaluator(o) ? o : null;
    }

    public static TInput Unless<TInput>(this TInput o, Func<TInput, bool> evaluator) where TInput : class
    {
        if (o == null) return null;
        return evaluator(o) ? null : o;
    }

    public static TInput Do<TInput>(this TInput o, Action<TInput> action) where TInput : class
    {
        if (o == null) return null;
        action(o);
        return o;
    }

    public static List<TInput> Delete<TInput>(this List<TInput> o, Func<TInput, bool> evaluator) where TInput : class
    {
        var listToDelete = o.Where(evaluator).ToList();
        foreach (var input in listToDelete)
        {
            o.Remove(input);
        }
        return o;
    }
}


public class LambdaComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _lambdaComparer;
    private readonly Func<T, int> _lambdaHash;

    public LambdaComparer(Func<T, T, bool> lambdaComparer) :
        this(lambdaComparer, o => 0)
    {
    }

    public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
    {
        if (lambdaComparer == null)
            throw new ArgumentNullException("lambdaComparer");
        if (lambdaHash == null)
            throw new ArgumentNullException("lambdaHash");

        _lambdaComparer = lambdaComparer;
        _lambdaHash = lambdaHash;
    }

    public bool Equals(T x, T y)
    {
        return _lambdaComparer(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _lambdaHash(obj);
    }
}
```

---
## Log2Console with IISExpress and log4net - 25 April, 2013

Log2Console with IISExpress and log4net
I have found that log4net and log2console don't work correctly (using IISExpress) with each other after trying using default configuration from [http://log2console.codeplex.com/wikipage?title=ClientConfiguration](http://log2console.codeplex.com/wikipage?title=ClientConfiguration.).

But i have found new configuration for log4net and log2console ([log4net](http://logging.apache.org/log4net/release/config-examples.html#udpappender)) and

![example1](./../images/log2net1.png)

Seems work fine:

![example2](./../images/log2net2.png)

---
## jQuery / Twitter Bootstrap List Tree Plugin Editable version - 20 April, 2013
Tags: javascript, bootstrap, jquery

JQuery / Twitter Bootstrap List Tree Plugin is a great plugin, but it does not allow to edit and sort tree elements. I have created an editable version of plugin: The view of tree:

![example](./../images/bootstrap_tree.png)

You can find the source code [there](http://pastebin.com/BTA4nL1c) or [here](./../code/tree.js).

Also see it [below](http://jsfiddle.net/QD8Hs/1060/).

---
## TFS Delete Team Project - 30 January, 2013

It is not so easy to find the steps to delete project from TFS.

So, according to [http://stackoverflow.com/questions/13635889/delete-team-project-from-free-team-foundation-service](http://stackoverflow.com/questions/13635889/delete-team-project-from-free-team-foundation-service).

You can use the following command from the "Developer Command Prompt":

```
TfsDeleteProject /collection:https://mytfs.visualstudio.com/DefaultCollection MyProject
```

Thank you

---
## Avoid "Ambiguous invocation" for extension methods - 14 September, 2012
Tags: asp.net mvc

I m using ASP.NET MVC. And work with my view, where I have the View 

![example](./../images/invocation.png)

It gives me the exception

```
The call is ambiguous between the following methods or properties: 'GMP.MvcWebSite.StringExtensions.TrimOrEmpty(string)' and 'System.StringHelper.TrimOrEmpty(string)'
```

So i just rebuild my view as

![example2](./../images/invocation2.png)

---
## API Facebook на С# - 28 September, 2012
Tags: facebook, api

I know there are a lot of information about working with Facebook API, but... I just think more information is better then less.

I need to post different types of context to Facebook pages (user create his own page, give us the name of page). OK, let's go!

I m using ASP.NET MVC, you know...
Link to the project.

All events has been shown on the main page.

1. Create our application in facebook.

![example](./../images/facebook1.png)

You see there is secret key and application key. We need them to working with facebook through our application.

2. Authorization (OAuth 2.0).

![example](./../images/facebook2.png)

* Ask user to permission.
* User will be redirected to facebook access page.
* User give us needed permission.
* User has been redirected back to our web site with special code.

There is the step 4 (we get the code from redirecting response).

```csharp
public void GetAccessToken()
{
    if (HttpContext.Current.Request.Params.AllKeys.Contains("code"))
    {
        code = HttpContext.Current.Request.Params["code"];
        //get the short-lived user access_toke
        string request = string.Format(_tokenEndpoint, _applicationId, _redirectTo, _applicationSecret, code);
        var webClient = new WebClient();
        string response = webClient.DownloadString(request);
        string[] pairResponse = response.Split('&');
        accessToken = pairResponse[0].Split('=')[1];
        //get the long-lived user access_toke
        request = string.Format(_exchangeAccessToken, _applicationId, _applicationSecret, accessToken);
        webClient = new WebClient();
        response = webClient.DownloadString(request);
        if (!accessToken.Equals(response.Split('=')[1]))
        {
            throw new AccessViolationException();
        }
        GetUserInformation();
    }
    else if (HttpContext.Current.Request.Params.AllKeys.Contains("error"))
    {
        error = HttpContext.Current.Request.Params["error"];
        throw new AccessViolationException(error);
    }
    throw new HttpException();
}
```

That is my facebook controller

```csharp
public ActionResult Index()
{
    if (!Client.IsAuthorizated)
    {
        return Redirect(Client.UriToAuth);
    }
    return View(new FacebookModel {Name = ""});
}

public ActionResult Authorizate()
{
    Client.GetAccessToken();
    return RedirectToAction("Index");
}
```

After getting code we need to get the id of the page:

```csharp
private void GetUserInformation()
{
    string request = "https://graph.facebook.com/me?access_token=" + accessToken;
    var webClient = new WebClient();
    string response = webClient.DownloadString(request);
    user = JObject.Parse(response);
    GetPagesInformation();
}

private void GetPagesInformation()
{
    string request = "https://graph.facebook.com/" + user.SelectToken("id") + "/accounts?access_token=" +
                        accessToken;
    var webClient = new WebClient();
    string response = webClient.DownloadString(request);
    userPages = JObject.Parse(response);
    page = userPages.SelectToken("data").First(x => x.SelectToken("name").ToString().Equals(_pageName));
}
```

3. OK, let's start to posting something. Here is my configuration:

```csharp
private static readonly Dictionary<string, string> Config = new Dictionary<string, string>
{
    {"AuthorizationEndpoint", "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope=manage_pages,create_event,publish_stream"},
    {"TokenEndpoint", "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}"},
    {"ApplicationId", "387222228001291"},
    {"ApplicationSecret", "3a177e2231e2966733771775b42"},
    {"RedirectTo", "http://localhost:4769/Facebook/Authorizate"},
    {"PageName", "TesterMyRest Community"}
};
```

Posting video.

```csharp
public string CreateVideo(MemoryStream imageMemoryStream, string title, string fileName)
{
    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
    var uploadRequest =
        (HttpWebRequest)
        WebRequest.Create("https://graph.facebook.com/" + page.SelectToken("id") + "/videos?access_token=" +
                            page.SelectToken("access_token"));
    uploadRequest.ServicePoint.Expect100Continue = false;
    uploadRequest.Method = "POST";
    uploadRequest.UserAgent = "Mozilla/4.0 (compatible; Windows NT)";
    uploadRequest.ContentType = "multipart/form-data; boundary=" + boundary;
    uploadRequest.KeepAlive = false;

    var sb = new StringBuilder();

    const string formdataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";
    sb.AppendFormat(formdataTemplate, boundary, "title", HttpContext.Current.Server.HtmlEncode(title));

    const string headerTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
    sb.AppendFormat(headerTemplate, boundary, "source", fileName, @"application/octet-stream");

    string formString = sb.ToString();
    byte[] formBytes = Encoding.UTF8.GetBytes(formString);
    byte[] trailingBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

    long imageLength = imageMemoryStream.Length;
    long contentLength = formBytes.Length + imageLength + trailingBytes.Length;
    uploadRequest.ContentLength = contentLength;

    uploadRequest.AllowWriteStreamBuffering = false;
    Stream strmOut = uploadRequest.GetRequestStream();

    strmOut.Write(formBytes, 0, formBytes.Length);

    var buffer = new Byte[checked((uint) Math.Min(4096, (int) imageLength))];
    int bytesRead;
    imageMemoryStream.Seek(0, SeekOrigin.Begin);
    while ((bytesRead = imageMemoryStream.Read(buffer, 0, buffer.Length)) != 0)
    {
        strmOut.Write(buffer, 0, bytesRead);
    }

    strmOut.Write(trailingBytes, 0, trailingBytes.Length);

    strmOut.Close();

    var wresp = uploadRequest.GetResponse() as HttpWebResponse;
    Encoding enc = Encoding.UTF8;
    if (wresp != null)
    {
        var stream = wresp.GetResponseStream();
        if (stream != null)
        {
            var loResponseStream = new StreamReader(stream, enc);
            return "https://graph.facebook.com/" + loResponseStream.ReadToEnd();
        }
    }
    return string.Empty;
}
```

4. Change the information about the page.

WebHelper class:
```csharp
public static class WebWorker
{
    private static void AddPostParameter(Dictionary<string, string> values, StringBuilder postBody)
    {
        foreach (string key in values.Keys)
        {
            if (postBody.Length > 0)
            {
                postBody.Append("&");
            }
            postBody.Append(string.Format("{0}={1}", key, values[key]));
        }
    }

    public static JObject DownloadJson(string requestUrl)
    {
        var webClient = new WebClient();
        string response = webClient.DownloadString(requestUrl);
        return JObject.Parse(response);
    }

    public static string UploadString(string requstUrl, Dictionary<string, string> values)
    {
        var webClient = new WebClient();
        var postBody = new StringBuilder();
        AddPostParameter(values, postBody);
        return webClient.UploadString(requstUrl, postBody.ToString());
    }
}
```
```csharp
private string CreateStatus(Dictionary<string, string> values)
{
    string request = "https://graph.facebook.com/" + page.SelectToken("id") + "/feed?access_token=" +
                        page.SelectToken("access_token");
    return WebWorker.UploadString(request, values);
}
```

---
## ASP.NET MVC Bundle Minification - Not Found 404 - 06 September, 2012
Tags: asp.net mvc

If you try to add Bundle from Web.Optimization library to your existing project and your Web.config file

![example](./../images/bundle1.png)

so runAllManagedModulesForAllRequests="false".

You start to get 404- Not found response to your bundle requests.

![example](./../images/bundle2.png)

Just enable BundleModule and all will be OK ! ;)

---
## VS 2010 Attach to process w3wp.exe - 20 July, 2012

Need to create the macros:

```
Public Module AttachToProcess
Public Function AttachToProcess(ByVal ProcessName As String) As Boolean
Dim proc As EnvDTE.Process
Dim attached As Boolean
For Each proc In DTE.Debugger.LocalProcesses
If (Right(proc.Name, Len(ProcessName)) = ProcessName) Then
proc.Attach()
attached = True
End If
Next

Return attached
End Function

Sub AttachToW3WP()
If Not AttachToProcess("w3wp.exe") Then
System.Windows.Forms.MessageBox.Show("Cannot attach to process")
End If
End Sub
End Module
```

[Link](http://habrahabr.ru/post/131937/)

---
## PowerShell, Replace physical path of web sites in IIS7 - 17 July, 2012
Tags: powershell

```PowerShell
param([String]$numb)

[Void][Reflection.Assembly]::LoadWithPartialName("Microsoft.Web.Administration")

$siteName = "graph.vrpinc.com"
##$serverIP = "your ip address"
$newPath = "D:\Projects\gmp"+$numb+"\GMP.WebSite"

$serverManager = New-Object Microsoft.Web.Administration.ServerManager
## $serverManager = [Microsoft.Web.Administration.ServerManager]::OpenRemote($serverIP)
$site = $serverManager.Sites | where { $_.Name -eq $siteName }
$rootApp = $site.Applications | where { $_.Path -eq "/" }
$rootVdir = $rootApp.VirtualDirectories | where { $_.Path -eq "/" }
$rootVdir.PhysicalPath = $newPath
$serverManager.CommitChanges()

$siteName = "gmp3.vrpinc.com"
$newPath = "D:\Projects\gmp"+$numb+"\GMP.MvcWebSite"

$serverManager = New-Object Microsoft.Web.Administration.ServerManager
## $serverManager = [Microsoft.Web.Administration.ServerManager]::OpenRemote($serverIP)
$site = $serverManager.Sites | where { $_.Name -eq $siteName }
$rootApp = $site.Applications | where { $_.Path -eq "/" }
$rootVdir = $rootApp.VirtualDirectories | where { $_.Path -eq "/" }
$rootVdir.PhysicalPath = $newPath
$serverManager.CommitChanges()

$siteName = "GMPServices"
$newPath = "D:\Projects\gmp"+$numb+"\GMP.Services"

$serverManager = New-Object Microsoft.Web.Administration.ServerManager
## $serverManager = [Microsoft.Web.Administration.ServerManager]::OpenRemote($serverIP)
$site = $serverManager.Sites | where { $_.Name -eq $siteName }
$rootApp = $site.Applications | where { $_.Path -eq "/" }
$rootVdir = $rootApp.VirtualDirectories | where { $_.Path -eq "/" }
$rootVdir.PhysicalPath = $newPath
$serverManager.CommitChanges()
```

Bat file to call PowerShell file

```
@echo off

set /p delBuild=Enter the number of gmp project?
powershell -noprofile Set-ExecutionPolicy Unrestricted
powershell .\setUpSite.ps1 -numb %delBuild%
```

One variable to set current version of projects!
Thanks!

---
## MVC indefinitely loads the page and call the controller cyclical - 31 January, 2012
Tags: asp.net mvc

I have found that the page load indefinitely. And the reason was I add @Html.RenderAction to my default layout.

So the solution of the problem to add the next code to rendered view:

```
@{
      Layout = null;
}
```
 
As I understand, MVC try to load the next without this code:

Layout -> MyView -> Layout -> MyView -> ....

With code above:

Layout -> MyView { -> null}

So it's happens.

---
## 0x80004005: Failed to Execute URL - 12 December, 2011

The error was connected with GET requests. ApplicationPool was set in Classic mode.
There is HttpModule, which throws this type of exceptions:

```
System.Web.HttpException (0x80004005): Failed to Execute URL.
at System.Web.Hosting.ISAPIWorkerRequestInProcForIIS6.BeginExecuteUrl(String url, String method, String childHeaders, Boolean sendHeaders, Boolean addUserIndo, IntPtr token, String name, String authType, Byte[] entity, AsyncCallback cb, Object state)
at System.Web.HttpResponse.BeginExecuteUrlForEntireResponse(String pathOverride, NameValueCollection requestHeaders, AsyncCallback cb, Object state)
at System.Web.DefaultHttpHandler.BeginProcessRequest(HttpContext context, AsyncCallback callback, Object state)
at System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute()
at System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean& completedSynchronously)
```

contained the next source code in event context.BeginRequest:

```
app.Context.RewritePath(app.Context.Request.Path);
```

The solve of the problem is:

```
app.Context.RewritePath(app.Context.Request.FilePath, app.Context.Request.PathInfo, string.Empty);
```

---
## PowerShell How to ... ? - 9 December, 2011
Tags: powershell

1. How to set folder's permission?

```powershell
 #set owner and principals for %SystemRoot%\TEMP
 #http://channel9.msdn.com/Forums/Coffeehouse/Powershell-subinacl-ownership-of-directories
 Write-Host -ForegroundColor green "Set owner and principals for %SystemRoot%\TEMP"
 $pathToSystemRoot = get-content env:systemroot
 $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
 $Principal = new-object security.principal.ntaccount $currentUser
 $path = Join-Path $pathToSystemRoot \temp
 
 $code = @"
using System;
using System.Runtime.InteropServices;

namespace WSG.Utils
{
public class PermissionsSetter
{

  [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
  internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
  ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);


  [DllImport("kernel32.dll", ExactSpelling = true)]
  internal static extern IntPtr GetCurrentProcess();


  [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
  internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
  phtok);


  [DllImport("advapi32.dll", SetLastError = true)]
  internal static extern bool LookupPrivilegeValue(string host, string name,
  ref long pluid);


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct TokPriv1Luid
  {
   public int Count;
   public long Luid;
   public int Attr;
  }


  internal const int SE_PRIVILEGE_DISABLED = 0x00000000;
  internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
  internal const int TOKEN_QUERY = 0x00000008;
  internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;


  public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
  public const string SE_AUDIT_NAME = "SeAuditPrivilege";
  public const string SE_BACKUP_NAME = "SeBackupPrivilege";
  public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
  public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
  public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
  public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
  public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
  public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
  public const string SE_DEBUG_NAME = "SeDebugPrivilege";
  public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
  public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
  public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
  public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
  public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
  public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
  public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
  public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
  public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
  public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
  public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
  public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
  public const string SE_RESTORE_NAME = "SeRestorePrivilege";
  public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
  public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
  public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
  public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
  public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
  public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
  public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
  public const string SE_TCB_NAME = "SeTcbPrivilege";
  public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
  public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
  public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
  public const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";       


  public static bool AddPrivilege(string privilege)
  {
   try
   {
    bool retVal;
    TokPriv1Luid tp;
    IntPtr hproc = GetCurrentProcess();
    IntPtr htok = IntPtr.Zero;
    retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
    tp.Count = 1;
    tp.Luid = 0;
    tp.Attr = SE_PRIVILEGE_ENABLED;
    retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);
    retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
    return retVal;
   }
   catch (Exception ex)
   {
    throw ex;
   }


  }
  public static bool RemovePrivilege(string privilege)
  {
   try
   {
    bool retVal;
    TokPriv1Luid tp;
    IntPtr hproc = GetCurrentProcess();
    IntPtr htok = IntPtr.Zero;
    retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
    tp.Count = 1;
    tp.Luid = 0;
    tp.Attr = SE_PRIVILEGE_DISABLED;
    retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);
    retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
    return retVal;
   }
   catch (Exception ex)
   {
    throw ex;
   }


  }
}
}
"@
 
 add-type $code
 
 $acl = Get-Acl $Path
 $acl.psbase.SetOwner($principal)
 $Ar = New-Object  system.security.accesscontrol.filesystemaccessrule("IIS_IUSRS","FullControl", "Allow")

 ## Check if Access already exists.
 #see http://cyrusbuilt.net/wordpress/?p=158
 if ($acl.Access | Where { $_.IdentityReference -eq $Principal}) {
  $accessModification = New-Object System.Security.AccessControl.AccessControlModification
  $accessModification.value__ = 2
  $modification = $false
  $acl.ModifyAccessRule($accessModification, $Ar, [ref]$modification) | Out-Null
 } else {
  $acl.AddAccessRule($Ar)
 }

 [void][WSG.Utils.PermissionsSetter]::AddPrivilege([WSG.Utils.PermissionsSetter]::SE_RESTORE_NAME)
 set-acl -Path $Path -AclObject $acl
 [void][WSG.Utils.PermissionsSetter]::RemovePrivilege([WSG.Utils.PermissionsSetter]::SE_RESTORE_NAME)
```

2. How to register Asp.Net & WCF in IIS?

```powershell
 $pathToFramework = "$env:windir\Microsoft.NET\Framework"
 if (test-path "$env:windir\Microsoft.NET\Framework64")
 {
  $pathToFramework = "$env:windir\Microsoft.NET\Framework64"
 }
 
 #start aspnet_regiis and ServiceModelReg
 $aspNet2 = Test-Path "$pathToFramework\v2.0.50727\aspnet_regiis.exe" -pathType leaf
 if (($aspNet2 -eq $true) -and ($aspNet2Reg -eq $false))
 {
  Write-Host -ForegroundColor green "`r`nInstall aspnet_regiis.exe v2.0.50727"
  & "$pathToFramework\v2.0.50727\aspnet_regiis.exe" -i -enable
 }

 $ServModReg3 = Test-Path "$pathToFramework\v3.0\Windows Communication Foundation\ServiceModelReg.exe" -pathType leaf
 if ($ServModReg3 -eq $true)
 {
  Write-Host -ForegroundColor green "`r`nInstall ServiceModelReg.exe v3.0"
  & "$pathToFramework\v3.0\Windows Communication Foundation\ServiceModelReg.exe" -iru
 }
  
 $ServModReg4 = Test-Path "$pathToFramework\v4.0.30319\ServiceModelReg.exe" -pathType leaf
 if ($ServModReg4 -eq $true)
 {
  Write-Host -ForegroundColor green "`r`nInstall ServiceModelReg.exe v4.0.30319"
  & "$pathToFramework\v4.0.30319\ServiceModelReg.exe" -ia -q -nologo
 }

 $AspNetRegIis4 = Test-Path "$pathToFramework\v4.0.30319\aspnet_regiis.exe" -pathType leaf
 if (($AspNetRegIis4 -eq $true) -and ($aspNet4Reg -eq $false))
 {
  Write-Host -ForegroundColor green "`r`nInstall aspnet_regiis.exe v4.0.30319"
  & "$pathToFramework\v4.0.30319\aspnet_regiis.exe" -ir -enable
 }
```

3. How to enable windows features?

```powershell
 #check the windows features
 $features = @(("IIS-ASPNET", "unknown"), ("IIS-HttpCompressionDynamic", "unknown"), ("IIS-ManagementScriptingTools", "unknown"), ("IIS-IIS6ManagementCompatibility", "unknown"), ("IIS-Metabase", "unknown"), ("IIS-WMICompatibility", "unknown"), ("IIS-LegacyScripts", "unknown"), ("IIS-LegacySnapIn", "unknown"))
 
 $dismPath = "$env:windir\System32\Dism.exe"
 if(test-path "$env:windir\Sysnative\Dism.exe")
 {
  $dismPath = "$env:windir\Sysnative\Dism.exe"
 }
 
 Write-Host -ForegroundColor green "`r`nGet windows features"
 $res = & "$dismPath" /online /Get-Features
 #take feature's states
 $writeNextStr = $false
 for ($i = 0; $i -lt $res.Count; $i++)
 {
  $str = $res[$i]
  foreach ($feature in $features)
  {
   if ($str.Contains($feature[0]))
   {
    $feature[1] = $res[$i+1]
    break
   }
  }
 }
 
 #show results
 Write-Host -ForegroundColor green "`r`nPlease see the states of features`r`n"
 foreach($feature in $features)
 {
  Write-Host -ForegroundColor yellow "$feature"
 }
 Write-Host -ForegroundColor green "`r`n"
 
 #enable features
 $needToRestart = $false
 Write-Host -ForegroundColor green "Started to enable all features`r`n"
 foreach($feature in $features)
 {
  if ($feature[1] -ne "State : Enabled")
  {
   $needToRestart  = $true
   $temp = $feature[0]
   Write-Host -ForegroundColor green "Try to enable $temp"
   & "$dismPath" /online /Enable-Feature /FeatureName:$temp /NoRestart
  }
 }
```

4. How to avoid exception "The OS handle’s position is not what FileStream expected"?

```powershell
#this code is for exception such as The OS handle’s position is not what FileStream expected
#see http://www.leeholmes.com/blog/2008/07/30/workaround-the-os-handles-position-is-not-what-filestream-expected/
$bindingFlags = [Reflection.BindingFlags] “Instance,NonPublic,GetField”
$objectRef = $host.GetType().GetField(“externalHostRef”, $bindingFlags).GetValue($host)
$bindingFlags = [Reflection.BindingFlags] “Instance,NonPublic,GetProperty”
$consoleHost = $objectRef.GetType().GetProperty(“Value”, $bindingFlags).GetValue($objectRef, @())
[void] $consoleHost.GetType().GetProperty(“IsStandardOutputRedirected”, $bindingFlags).GetValue($consoleHost, @())
$bindingFlags = [Reflection.BindingFlags] “Instance,NonPublic,GetField”
$field = $consoleHost.GetType().GetField(“standardOutputWriter”, $bindingFlags)
$field.SetValue($consoleHost, [Console]::Out)
$field2 = $consoleHost.GetType().GetField(“standardErrorWriter”, $bindingFlags)
$field2.SetValue($consoleHost, [Console]::Out)
```

5. How to load module?

```powershell
Import-Module WebAdministration
```

6. How to load another script file?

```powershell
#load external functions
. (Join-Path $curFolder \Functions\DevSetupFunctions.ps1)
```

---
## Style Cop / I don't like SA 1201 - 8 December, 2011

[StyleCop]() is a good application to take your source code in good state. There are a lot of rules and most of all are very useful, but...

Unfortunately, the rule SA1201 is not so good for me. So i just want to create the similar rule, but a little else.

You can see the rule that changes the order of document's element to the next:

```xml
<element name="File" order="0"></element>
<element name="Root" order="1"></element>
<element name="ExternAliasDirective" order="2"></element>
<element name="UsingDirective" order="3"></element>
<element name="AssemblyAttribute" order="4"></element>
<element name="Namespace" order="5"></element>
<element name="Field" order="6"></element>
<element name="Constructor" order="10"></element>
<element name="Destructor" order="11"></element>
<element name="Delegate" order="8"></element>
<element name="Event" order="9"></element>
<element name="Enum" order="13"></element>
<element name="Interface" order="14"></element>
<element name="Property" order="7"></element>
<element name="Accessor" order="15"></element>
<element name="Indexer" order="16"></element>
<element name="Method" order="12"></element>
<element name="Struct" order="17"></element>
<element name="EnumItem" order="18"></element>
<element name="ConstructorInitializer" order="19"></element>
<element name="EmptyElement" order="20"></element>
```

It saves in configuration file, so it is possible to change it in any time. 
Further the source code of my rule. You can see the original source code of SA1201 using the [DotPeek](http://www.jetbrains.com/decompiler/) for [example](./../code/code.cs).

Methods AnalyzeDocument и DoAnalysis are the most important, so i suggest to start the learning code from them. The project in VS2010 is StyleCopOrder.Thanks.

---
## Fiddler Zip extension - 8 December, 2011

Extension for [Fiddler](http://www.fiddler2.com/Fiddler/dev/).

![example](./../images/zip_fiddler.jpg)

The source code: [FiddlerZip](https://drive.google.com/file/d/0BwVmorgjT-W1NDFlZDZkMDItMWMxMS00NzU2LTg3NDUtYTYzOWVhOGMyMzRj/view).

The main code:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Windows.Forms;
 
// It is important !!! see the current version of fiddler
[assembly: Fiddler.RequiredVersion("2.3.4.4")]
 
namespace FiddlerZip
{
    /// <summary>
    /// Zip extension
    /// </summary>
    public class Zip : IFiddlerExtension 
    {
        /// <summary>
        /// page for extesnion
        /// </summary>
        private TabPage oPage;
        /// <summary>
        /// Control that presented extension
        /// </summary>
        private ZipControl oAppConsole;
        #region IFiddlerExtension Members
 
        public void OnBeforeUnload()
        {
            
        }
 
        public void OnLoad()
        {
            // create tab with name "Zip"
            oPage = new TabPage("Zip");
            // it is possible to add icon to your tab
            oPage.ImageIndex = (int)Fiddler.SessionIcons.FilterBrowsers;
            oAppConsole = new ZipControl();
            // add control to tab
            oPage.Controls.Add(oAppConsole);
            oAppConsole.Dock = DockStyle.Fill;
            FiddlerApplication.UI.tabsViews.TabPages.Add(oPage);
        }
 
        #endregion
    }
}
```

---
## IIS 7 HttpModule Logger - 6 December, 2011

Простой модуль-лог для IIS 7 (Classic mode)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net;
using ReutersKnowledge.Web.Services.Util;
using System.Collections.Specialized;
using System.Reflection;
 
namespace IISWsgLogger
{
    public class IISLoggerModule : IHttpModule
    {
        private static string fileName = "D:\\Log.txt";
 
        private static ConcurrentQueue<string> logRecords = new ConcurrentQueue<string>();
 
        private static object syncTask = new object();
        private static Task taskLog;
 
        public void Init(HttpApplication context)
        {
            if (taskLog == null)
            {
                lock (syncTask)
                {
                    if (taskLog == null)
                    {
                        taskLog = Task.Factory.StartNew(() => StartLog(), TaskCreationOptions.LongRunning);
                    }
                }
            }
            context.BeginRequest += new EventHandler(OnPreRequestHandlerExecute);
            context.EndRequest += new EventHandler(OnPostReleaseRequestState);
        }
 
        private void StartLog()
        {
            var t = File.AppendText(fileName);
            t.WriteLine("date time wsg-guid s-port cs-method reuters-uuid cs(Host) cs-uri-stem cs-uri-query sc-status sc-substatus cs(User-Agent) cs(Cookie) TimeTakenMS sc-bytes");
            int waitMil = 1000;
            string res = null;
            try
            {
                while (true)
                {
                    while (logRecords.TryDequeue(out res))
                    {
                        t.WriteLine(res);
                    }
                    Thread.Sleep(waitMil);
                }
            }
            finally
            {
                t.Close();
            }
 
        }
 
        public void Dispose()
        {
        }
 
        public void OnPreRequestHandlerExecute(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            var guid = Guid.NewGuid().ToString();
            app.Context.RewritePath(app.Context.Request.FilePath, app.Context.Request.PathInfo, "guid=" + guid);
        }
 
        public void OnPostReleaseRequestState(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            var time = DateTime.Now;
            var timeStamp = app.Context.Timestamp;
            var port = app.Context.Request.Url.Port;
            var typeOfRequest = app.Context.Request.RequestType;
            var Guid = app.Context.Request.QueryString[0];
            var host = app.Context.Request.Url.Host;
            var rawUrl = app.Context.Request.RawUrl;
            var contentRequestLength = app.Context.Request.ContentLength;
            var status = app.Context.Response.StatusCode;
            var agent = app.Context.Request.UserAgent;
            var cookies = string.Empty;
            foreach (var cookie in app.Context.Request.Cookies)
            {
                cookies += cookies;
            }
 
            logRecords.Enqueue(string.Format("{0} {1} {3} {4} {5} {6} {7} {8} {9} {10}", time, guid, port, typeOfRequest, host, rawUrl, status, agent, timeStamp, contentRequestLength));
        }
 
    }
}
```

web.config :

```xml
<?xml version="1.0"?>
<configuration>
 <configSections>
  ...
 </configSections>
 <system.web>
  ...
   <httpModules>
   ...
  </httpModules>
 </system.web>
 <system.webServer>
  <modules>
   <add name="IISLoggerModule" type="IISLogger.IISLoggerModule, IISLogger.IISLoggerNamespace.IISLogger"/>
```

---
## WCF/ Response without root element - 21 June, 2011

Write WCF web-service that accepts SOAP 1.2 and SOAP 1.1 requests. Service should expose 1 operation (let it be "GetResponse_1") with the following structure (just an example, please combine schema for the service):

```xml
<GetResponse_Request_1>
    <element1>text1</element1><!-- at least 1 "element1" -->
    <element1>text2</element1>
    <element1>text3</element1>
    <element2>textn</element2><!-- at least 1 "element2" -->
    <element2>textn+1</element2>
    <element2>textn+2</element2>     
</GetResponse_Request_1>
```

Response should contain "planed" element1 and element2 sets, just comma separated values:

```xml
<GetResponse_Response_1>
    <element1_plane>text1, text2, text3</element1>
    <element2_plane>textn, textn+1, textn+2</element2>
</GetResponse_Response_1>
```
---
## Create line chart use .net 2.0 - 06 May, 2011

See please [ChatBuilder](https://drive.google.com/file/d/0BwVmorgjT-W1YzVhNmNmYzktOWM3Yy00OGUyLWExNzYtNTZkOGU0NmVkZTdi/view). There are 4 files of simple source code.

---
## Use LogParser - 5 May, 2011
Tags: .net

1. Download LogParser 2.2.
2. C:\>tlbimp LogParser.dll /out:Interop.MSUtil.dll
3. Include Interop.MSUtil.dll in a project.
4.

```csharp
using LogQuery = Interop.MSUtil.LogQueryClassClass;
using IISW3CInputFormat = Interop.MSUtil.COMIISW3CInputContextClassClass;
using CsvOutputFormat = Interop.MSUtil.COMCSVOutputContextClassClass;
using LogRecordSet = Interop.MSUtil.ILogRecordset;

LogQuery oLogQuery = new LogQuery();

// Instantiate the Event Log Input Format object
IISW3CInputFormat oW3CInputFormat = new IISW3CInputFormat();

string query = String.Format(_queryPostOverall, _pathToLog, "200", _date);
var oRecordSet = oLogQuery.Execute(query, oW3CInputFormat);
for (; !oRecordSet.atEnd(); oRecordSet.moveNext())
{
   double count = Convert.ToDouble(oRecordSet.getRecord().getValue("count1"));
   _postCountOverall.overall += count;
}
oRecordSet.close();
```

---
## Use ZadGraph - 5 May, 2011

Library can be downloaded. License is LPGL 2.1.

```csharp
if (dates.Count > 0 && dates.Count == values.Count)
{
    double max = FindMax(values);
    if (!Directory.Exists(_imageFolderName))
    {
        Directory.CreateDirectory(_imageFolderName);
    }
    GraphPane myPane = new GraphPane();

    // Set the titles and axis labels
    myPane.Title.Text = "Statistic of errors";
    myPane.XAxis.Title.Text = "Day";
    myPane.YAxis.Title.Text = "Percent";
    // Make up some data points based on the Sine function
    PointPairList list = new PointPairList();
    for (int i = 0; i < dates.Count; i++)
    {
        double x = dates[i].Date.ToOADate();
        double y = values[i];
        list.Add(x, y);
    }
    myPane.XAxis.CrossAuto = true;
    myPane.XAxis.Type = AxisType.Date;
    myPane.XAxis.Scale.MinorStep = 1;
    myPane.XAxis.Scale.MajorStep = 1;
    myPane.XAxis.Scale.Format = "dd.MM.yy";
    myPane.XAxis.MajorTic.IsBetweenLabels = true;
    myPane.XAxis.Scale.Min = dates[0].ToOADate();
    myPane.XAxis.Scale.Max = dates[dates.Count - 1].ToOADate();
    myPane.XAxis.AxisGap = 3f;

    myPane.YAxis.Scale.Max = max + 4;
    myPane.YAxis.Scale.MinorStep = myPane.YAxis.Scale.Max/10;
    myPane.YAxis.MajorGrid.IsVisible = true;
    myPane.YAxis.Type = AxisType.Linear;
    LineItem myCurve2 = myPane.AddCurve("Overall",
            list, Color.Blue, SymbolType.Diamond);
    myPane.YAxis.Scale.MajorStep = myPane.YAxis.Scale.Max / 5;

    myPane.Legend.Position = ZedGraph.LegendPos.Bottom;
    int width = 800;
    if (27 * dates.Count > width)
        width = 27 * dates.Count;
    myPane.GetImage(width, 600, 800f).Save(Path.Combine(_imageFolderName, _imageFileName), System.Drawing.Imaging.ImageFormat.Jpeg);
}
```

---
## Sniffer of TCP packets - 10 February, 2011
Tags: .net

Поставновка задачи: Необходимо создать сниффер, который позволяет получить инфрмацию, которая хранится внутри tcp-пакета (например, мы снифферим загрузку html-страниц). Следует отметить, что tcp-пакеты могут приходить на машину назначение беспорядочно. Таким образом класс Sniffer, используя библиотеку WinPcap упорядочивает все пакеты.

Проект на VS2008 [SharkTrace](https://docs.google.com/open?id=0BwVmorgjT-W1OWVmY2M1MzktNGRhNS00MjEwLWIyOTktMWMzODI3YmM3Mzc3).

Важно! Библиотека SharpPcap уже давно имеет новый интерфейс.

Далее в тексте, под соединением я понимаю соответствие адресов и портов на клиенте и сервере. 

Для работы необходима открытая библиотека SharpPcap, она предоставляет удобный интерфейс для работы с приложением WinPcap ([Sharppcap](http://sourceforge.net/projects/sharppcap/)).

Класс соединения используется для хранения информации об адресе клиента и сервера, о портах, а так же об текущем ожидаемом tcp-пакете.

Больше о флагах tcp-пакета можно посмотреть [http://www.firewall.cx.](http://www.firewall.cx/) 

```csharp
///<summary>
///class for connection
///</summary>
internal class Connection
{
    public long ClientAddress; // client initiating the connection
    public int ClientPort;
    public long HostAddress; // host receiving the connection
    public int HostPort;
    public long ClientSyn; // starting syn sent from client
    public long HostSyn; // starting syn sent from host;
    public long NextClientSeq; // this must be in SequenceNumber field of TCP packet if it is from client
    public long NextHostSeq; // this must be in SequenceNumber field of TCP packet if it is from host
    public bool HostClosed;
    public bool ClientClosed;
    public long TimeIdentifier;
    public bool ThreeWayCompleted = false; // three way connection is completed
 
    // Fragments , used when we get newer packets that expected.
    // so we need to wait for expected before adding them.
    public SortedDictionary<long, TcpPacket> HostFragments = new SortedDictionary<long, TcpPacket>();
    public SortedDictionary<long, TcpPacket> ClientFragments = new SortedDictionary<long, TcpPacket>();
 
    // returns client ip:client port as a string
    public string GetClientAddressPort()
    {
        return string.Format("{0}:{1}", new IPAddress(ClientAddress).ToString(), ClientPort);
    }
 
    // returns host ip:host port as a string
    public string GetHostAddressPort()
    {
        return string.Format("{0}:{1}", new IPAddress(HostAddress).ToString(), HostPort);
    }
 
    // packet is from host
    public bool IsFromHost(TcpPacket tcp)
    {
        return ClientAddress == ((IpPacket)tcp.ParentPacket).DestinationAddress.Address &&
        ClientPort == tcp.DestinationPort &&
        HostAddress == ((IpPacket)tcp.ParentPacket).SourceAddress.Address &&
        HostPort == tcp.SourcePort;
    }
 
    // packet is from client
    public bool IsFromClient(TcpPacket tcp)
    {
        return ClientAddress == ((IpPacket)tcp.ParentPacket).SourceAddress.Address &&
        ClientPort == tcp.SourcePort &&
        HostAddress == ((IpPacket)tcp.ParentPacket).DestinationAddress.Address &&
        HostPort == tcp.DestinationPort;
    }
 
    public Connection(long clientAddress, int clientPort, long hostAddress, int hostPort, long clientSyn)
    {
        this.ClientAddress = clientAddress;
        this.ClientPort = clientPort;
        this.HostAddress = hostAddress;
        this.HostPort = hostPort;
        this.ClientSyn = clientSyn;
    }
}
```

Итак данный класс хранит информацию о соединении. В сниффере я пропускаю процесс установления соединения (так называемое тройное рукопожатие) так как возможно мы запустим сниффер когда уже соединения было установлено (о нем можно почитать [wikipedia.org](http://en.wikipedia.org/wiki/Transmission_Control_Protocol#Connection_establishment)). Поэтому происходит попытка создания соединения, если оно еще не было создано. Ловятся пакеты, которые имеют "полезную" информацию в себе (payload data).

Больше о TCP пакетах [wikipedia.org](http://ru.wikipedia.org/wiki/TCP).

Методы RunSniffer и StopSniffer - запускают и останавливают сниффер соответственно. Метод AssemblePacket на вход получает tcp-пакет, и проверяет существует ли соединение, которому "принадлежит" этот пакет. Если нет - создается, если да - то работает логика по упорядочиванию пакетов. Два абстрактных метода позволяют получить доступ к "полезным" данным последовательно (AddHostData и AddClientData) 

```csharp
/// <summary>
/// Main class for sniffering
/// </summary>
/// <remarks>
/// 1. I see that there is new version of SharpPcap library with new interface (29/11/2011)
/// So i just try to update some useful information about this class
/// 
/// 2. I believe that SynchronizatedConnection class is just 
/// class SynchronizatedConnection : Connection
/// {
///     private object syncObject = new object();
///     
///     public Synchro {get {return syncObject;}}
/// }
/// 
/// 3. Coordinator is class with constants.
/// </remarks>
internal abstract class Sniffer 
{
    private Timer timer;
    private object synchronizationObjectForConnection = new object();
    private List<SynchronizatedConnection> Connections = new List<SynchronizatedConnection>();
    private LibPcapLiveDevice _device;
    private readonly List<System.Net.IPAddress> _hosts;
 
    // when connected
    public abstract void Connected(Object conn);
    // when disconnected
    public abstract void Disconnected(Object conn);
    public abstract void AddHostData(byte[] data, string host);
    public abstract void AddClientData(byte[] data, string client);
 
    private void ClearConnections(object source)
    {
        //sometimes purely the collection, just in case
        lock (synchronizationObjectForConnection)
        {
            foreach (var key in Connections.Where(k => (DateTime.Now.ToFileTimeUtc() - k.TimeIdentifier) > 600000000 * Coordinator.Config.HowLongWeSaveTransaction.TotalMinutes).ToList())
            {
                Connections.Remove(key);
            }
        }
    }
 
    /// <summary>
    /// Create the exemplare of the class
    /// </summary>
    public Sniffer(List<System.Net.IPAddress> hosts)
    {
        timer = new Timer(ClearConnections, null, Coordinator.Config.HowLongWeSaveTransaction, Coordinator.Config.HowLongWeSaveTransaction);
        _hosts = hosts;
    }
 
    /// <summary>
    /// Start the tracing
    /// </summary>
    /// <param name="filter">See http://www.cs.ucr.edu/~marios/ethereal-tcpdump.pdf </param>
    public void RunSniffer(string filter)
    {
        var devices = LibPcapLiveDeviceList.Instance;
        //
        _device = devices[Coordinator.Config.ListeningInterface];
        //Register our handler function to the 'packet arrival' event
        _device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
        //Open the device for capturing
        int readTimeoutMilliseconds = 1000;
        _device.Open(DeviceMode.Normal, readTimeoutMilliseconds);
        // tcpdump filter to capture only TCP/IP packets
        _device.Filter = filter;
        // Start capture packets
        _device.StartCapture();
    }
 
    /// <summary>
    /// Stop the tracing
    /// </summary>
    public void StopSniffer()
    {
        _device.StopCapture();
        _device.Close();
    }
 
    /// <summary>
    /// Catch the packet
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void device_OnPacketArrival(object sender, CaptureEventArgs e)
    {
        try
        {
            // try to get TCP packet from Ip packet
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if (packet is PacketDotNet.EthernetPacket)
            {
                var eth = ((PacketDotNet.EthernetPacket)packet);
                var ip = PacketDotNet.IpPacket.GetEncapsulated(packet);
                if (ip != null)
                {
                    var tcp = PacketDotNet.TcpPacket.GetEncapsulated(packet);
                    if (tcp != null)
                    {
                        AssemblePacket(tcp);
                    }
                }
            }
        }
 
        // sometimes converting doesn't work - don't worry about it
        catch (InvalidOperationException ex)
        {
        }
    }
 
    /// <summary>
    /// Parse TCP packet
    /// </summary>
    /// <param name="tcp">TCP packet</param>
    privDescription. There are modules that have the input and output parameters (type and number of parameters may be different). The goal is to select some first modules that have no input parameters, then the modules whose inputs are the output parameters of the previously selected modules and so on. Until the last module will not output parameters.

The database stores all displayed modules and displays of all parameters where the parameter mapping to the mapping of modules is many-to-one.
Initially, we choose to do all the settings using a simple SQL query.ate void AssemblePacket(TcpPacket tcp)
    {
        // pass the packets that :
        // 1. tcp.Syn && tcp.PayloadData.Length == 0 - sent for synchronization
        // 2. tcp.PayloadData.Length > 0 - no useful data in packet
        // 3. tcp.Fin || tcp.Rst - connection is finished or reseted
        if (!(tcp.Syn && tcp.PayloadData.Length == 0) && (tcp.PayloadData.Length > 0) && !(tcp.Fin || tcp.Rst))
        {
            SynchronizatedConnection conn;
            // try to find connection in collection
            bool? res = IsTcpFromClient(tcp, out conn);
                
            if (res == null)
            {
                // connection is new
                SynchronizatedConnection con;
                if (_hosts.Contains(((IpPacket)tcp.ParentPacket).SourceAddress))
                {
                    // packet from host to client
                    conn = new SynchronizatedConnection(((IpPacket)tcp.ParentPacket).DestinationAddress.Address,
                    tcp.DestinationPort, ((IpPacket)tcp.ParentPacket).SourceAddress.Address, tcp.SourcePort, tcp.SequenceNumber);
                    conn.ClientSyn = tcp.AcknowledgmentNumber;
                    conn.HostSyn = tcp.SequenceNumber;
                    conn.NextClientSeq = tcp.AcknowledgmentNumber;
                    conn.NextHostSeq = tcp.SequenceNumber;
                    res = false;
                }
        Description. There are modules that have the input and output parameters (type and number of parameters may be different). The goal is to select some first modules that have no input parameters, then the modules whose inputs are the output parameters of the previously selected modules and so on. Until the last module will not output parameters.

The database stores all displayed modules and displays of all parameters where the parameter mapping to the mapping of modules is many-to-one.
Initially, we choose to do all the settings using a simple SQL query.        else
                {
                    // packet from host to client
                    conn = new SynchronizatedConnection(((IpPacket)tcp.ParentPacket).SourceAddress.Address,
                    tcp.SourcePort, ((IpPacket)tcp.ParentPacket).DestinationAddress.Address, tcp.DestinationPort, tcp.SequenceNumber);
                    conn.ClientSyn = tcp.SequenceNumber;
                    conn.HostSyn = tcp.AcknowledgmentNumber;
                    conn.NextHostSeq = tcp.AcknowledgmentNumber;
                    conn.NextClientSeq = tcp.SequenceNumber;
                    res = true;
                }
                conn.TimeIdentifier = DateTime.Now.ToFileTimeUtc();
                conn.ThreeWayCompleted = true;
                lock (synchronizationObjectForConnection)
                {
                    Connections.Add(conn);
                }
            }
            if (res == true)
            {
                // from client
                lock (conn.Synchro)
                {
                    if (tcp.SequenceNumber < conn.NextClientSeq)
                    // old packet
                    {
                        // just drop these for now
                        return;
                    }
                    if (tcp.SequenceNumber > conn.NextClientSeq)
                    // out of order data
                    {
                        if (!conn.ClientFragments.ContainsKey(tcp.SequenceNumber))
                        {
                            conn.ClientFragments.Add(tcp.SequenceNumber, tcp);
                        }
                        else
                        // expect new data to be better?
                        {
                            conn.ClientFragments[tcp.SequenceNumber] = tcp;
                        }
                    }
                    else
                    {
                        while (tcp.SequenceNumber == conn.NextClientSeq)
                        {
                            conn.ClientFragments.Remove(tcp.SequenceNumber);
                            // remove fragment
                            if (tcp.PayloadData.Length == 0)
                                break;
                            // new NextClientSeq for client packet 
                            conn.NextClientSeq = conn.NextClientSeq + tcp.PayloadData.Length;
                            // data should be valid here.
                            AddClientData(GetUsefulData(tcp), GetIdOfConnection(conn));
                            if (conn.ClientFragments.ContainsKey(conn.NextClientSeq))
                            // check if we have newer fragments which will now fit.
                            {
                                tcp = conn.ClientFragments[conn.NextClientSeq];
                            }
                            else
                                break;
                        }
                    }
                }
            }
            else
            {
                //from host
                lock (conn.Synchro)
                {
                    if (tcp.SequenceNumber < conn.NextHostSeq)
                    // old packet
                    {
                        // just drop these for now
                        return;
                    }
                    if (tcp.SequenceNumber > conn.NextHostSeq)
                    // newer out of order data
                    {
                        if (!conn.HostFragments.ContainsKey(tcp.SequenceNumber))
                        {
                            conn.HostFragments.Add(tcp.SequenceNumber, tcp);
                        }
                        else
                        {
                            conn.HostFragments[tcp.SequenceNumber] = tcp;
                        }
                    }
                    else
                    //
                    {
                        while (tcp.SequenceNumber == conn.NextHostSeq)
                        // on time
                        {
                            conn.HostFragments.Remove(tcp.SequenceNumber);
                            // remove fragment
                            if (tcp.PayloadData.Length == 0)
                                break;
                            conn.NextHostSeq = conn.NextHostSeq + tcp.PayloadData.Length;
                            // data should be valid here
                            AddHostData(GetUsefulData(tcp), GetIdOfConnection(conn));
                            if (conn.HostFragments.ContainsKey(conn.NextHostSeq))
                            // check if we have newer fragments which will now fit.
                            {
                                tcp = conn.HostFragments[conn.NextHostSeq];
                            }
                            else
                                break;
                        }
                    }
                }
            }
        }
    }
 
    private static string GetIdOfConnection(SynchronizatedConnection conn)
    {
        return conn.ClientAddress.ToString() + conn.ClientPort.ToString() + conn.HostAddress.ToString()
        + conn.HostPort.ToString();
    }
 
    private bool? IsTcpFromClient(TcpPacket tcp, out SynchronizatedConnection conn)
    {
        conn = null;
        lock (synchronizationObjectForConnection)
        {
            foreach (var connection in Connections)
            {
                if (connection.IsFromClient(tcp))
                {
                    conn = connection;
                    return true;
                }
                if (connection.IsFromHost(tcp))
                {
                    conn = connection;
                    return false;
                }
            }
        }
        return null;
    }
 
    /// <summary>
    /// Get Payload Data from tcp packet
    /// </summary>
    /// <param name="tcp">TCP packet</param>
    /// <returns>bytes of useful data</returns>
    protected static byte[] GetUsefulData(TcpPacket tcp)
    {
        var data = new byte[tcp.Bytes.Length - tcp.DataOffset * 4];
        for (int i = tcp.DataOffset * 4; i < tcp.Bytes.Length; i++)
        {
            data[i - tcp.DataOffset * 4] = tcp.Bytes[i];
        }
        return data;
    }
}
```

Проблемы данного класса/решения:
1. Если "словится" первый пакет, который является неправильным - возникает проблема с получением дальнейшей информации, которая идет по данному соединению;
2. Не проверяется контрольная сумма принятого пакета;
3. Все манипуляции (приведение, извлечение TCP пакета из IP пакета, работа с этим пакетом) довольно времяемкие операции. Необходимо задавать хороший фильтр

```csharp
(public void RunSniffer(string filter)) 
```

или советую отказаться от этого решения для наблюдения за высоконагруженным сетевым траффиком.

---
## Linq and Group - 16 November, 2010
Tags: linq

Description. There are modules that have the input and output parameters (type and number of parameters may be different). The goal is to select some first modules that have no input parameters, then the modules whose inputs are the output parameters of the previously selected modules and so on. Until the last module will not output parameters.

The database stores all displayed modules and displays of all parameters where the parameter mapping to the mapping of modules is many-to-one.
Initially, we choose to do all the settings using a simple SQL query.

```csharp
private static SqlCeDataReader SelectAllParamterAndModuleName()
{
    return ExecuteDataReader(String.Format(QSelectAllParamterANdModuleName));
}
```

where QSelectAllParamterANdModuleName is text of the query.

```csharp
private static IEnumerable<ParameterType> SelectAllParameterToArray()
{
    var query =
        from row in SelectAllParamterAndModuleName().Cast()
        select new ParameterType
        {
            Id = (int)row["FID"],
            Name = (string)row["FNAME"],
            TypeName = (string)row["FTYPE"],
            ModuleId = (int)row["FMODULEID"],
                        ModuleName = (string)row["FMODULE"], Number = (int)row["FNUMBER"], TypeOfPar = (bool)row["FTYPEOFPARAMETER"]};
    return query.ToArray();
}
```

create from selected SqlReader array of elements of ParameterType. The result can be cached.

```csharp
public static List<ModuleType> SelectModuleByInputParameter(List<ParameterType> inputParameter)
{
    if (inputParameter == null)
    {
        throw new ArgumentNullException();
    }
    var res = new List();
    var para = SelectAllParameterToArray();
    if (inputParameter.Count == 0)
    {
        var par = from parameter in para
                    where !(from parameter2 in para
                            where parameter2.TypeOfPar
                            select parameter2.ModuleId).Contains(parameter.ModuleId)
                    select parameter;
        return par.Select(x =&gt; new ModuleType {Id = x.ModuleId, Name = x.ModuleName}).ToList();
    }

    var parGr = from parameter in para
                where parameter.TypeOfPar
                group parameter by parameter.ModuleId
                into modPara
                where modPara.Count() == inputParameter.Count
                select modPara;

    foreach (var paramGr in parGr)
    {
        bool AddInRes = true;
        var listParaGr = paramGr.ToList();
        for (int i = 0; i &lt; listParaGr.Count; i++)
        {
            if (listParaGr[i].Number != inputParameter[i].Number ||
            listParaGr[i].TypeName != inputParameter[i].TypeName)
                AddInRes = false;
        }
        if (AddInRes)
        {
            res.Add(new ModuleType(){Id =  listParaGr[0].ModuleId, Name = listParaGr[0].ModuleName});
        }
    }
    return res;
}
```

This method returns a collection of our modules, which contains defined input parameters. Thus, this method should be called justbefore the loop until the result will contain at least one value. It should be noted that calls to this method will generally be bifurcate (the first layer selected modules, and for everyone in this layer, called again, this method and so on). But this is not Linq.
