# Xamarin Dev Days in Warsawa - 24 September, 2016

Hi All, it is my overview of Xamarin Dev Days - short summary. Hope it will be useful to create an  impression what you can expect from this event.

My stream technology is server-side .NET or ASP.NET MVC. My current project is on .NET platform, the main technology is Windows Workflow Foundation and I am not using Xamarin platform. But I had a small experience in Xamarin that I got during working on my previous project.

Some time ago I had a chance to visit Xamarin Dev Days event in Warsaw. It was one day event in Warsaw Microsoft office. There were two part: the first one is generic information about Xamarin ecosystem and Azure Mobile Apps. The second one is hands on lab. So this presentation has been created to summarize and do a short recap mentioned event.

Agenda:
* Introduction to Xamarin
* Xamarin.Forms
* Azure Mobile Apps
* Sample application
* Resources

The first thing that you should keep in mind when you are talking about Xamarin is that you are able to write application not using Visual Studio on Windows only, but also [Xamarin Studio](https://www.xamarin.com/studio) on Mac. Basically, as I understand, usually people are using Visual Studio to develop Android and Phone applications and Xamarin Studio on Mac to develop IOS application. But, of course, the team is using one code base. The reason of this approach, that it is much more quicker to build IOS application on MAC directly, without remote building using Visual Studio on Windows.

The second thing is that Xamarin is not only about developing cross-platform applications using Visual Studio or Xamarin Studio, but Xamarin also provides possibility to test you application on various number of devices using [Xamarin Cloud](https://www.xamarin.com/test-cloud). As you can imagine, it is very expensive to buy all set of mobile devices for testing, so this cloud allows you to run your application on thousand of real devices in the cloud, analyze detailed test reports with results, screenshots, and performance metrics. Also it allows you to measure performance of your application. Sounds very cool, but there was a question from Xamarin Dev Days presenter about if there is someone who is using this cloud and nobody answered.

The third thing is about building and continuous integration. Actually it is not about Xamarin but about[TFS](https://www.visualstudio.com/tfs/)(Team Foundation Server). You are able to install it on your private server or it is possible to use [Visual Studio Online](https://www.visualstudio.com/) service from Microsoft. It is free for small teams. It provides opportunity to work with your code (git), use agile board to organize your work, set up your continuous integration project (pure version of team city), create test cases.

The last one is about distributing and monitoring. I can’t say a lot about distributing as I have never put any application to stores. As for monitoring, there is Xamarin Insight. It is the same approach as [Visual Studio Application Insights](https://www.visualstudio.com/en-us/docs/insights/application-insights). It is an extensible analytics service that helps you understand the performance and usage of your live mobile application. It's designed for developers, to help you continuously improve the performance and usability of your app. It [allows](http://www.joesauve.com/xamarin-insights-after-only-5-minutes-its-already-saving-my-arse/):

* see user sessions in realtime
* see which users are being affected by which errors
* see stacktraces for each exception
* see device stats for each exception (operating system, app version, network status, device orientation, jalbreak status, and bluetooth status)
* see advanced reporting and filtering of aggregate exception statistics
* setup webhooks for triggering actions on certain Insights events
* integrate with third-party services (Campire, Github, HipChat, Jira, PIvotalTracker, and Visual Studio Online)

* Separate solutions for each platform (Android, IOS, Windows)
    * Many projects, many languages, many teams
* One universal solution (JS + HTML + CSS) - Cordova
    * Slow performance, limited native API
* Xamarin approach (shared code + platform specific UI)
    * Good performance, almost all native API
* ReactNative and NativeScript
    * Only Android and IOS (UWP in future)

On this slide I want to show the main ways to build mobile application.<
    
1. Of course, firstly is native apps. It is clear. Swift, object-C for IOS, java is for Android, C# is  for Windows Phone. It means you should have and support many projects and many teams. It is a good option if you are planning to build complex and big mobile application. The best scenario is if this application has only mobile version.
2. Universal solution. You are able to use Cordova and build you application using JavaScript. Personally I really like this approach as you are able to build almost any type of application using Javascript now. To execute javascript on server - NodeJs. For desktop application there is Electron framework. Cordova is to create mobile applications.The problem here is performance. The resulting applications are hybrid, meaning that they are neither truly native mobile application (because all layout rendering is done via Web views instead of the platform's native UI framework) nor purely Web-based (because they are not just Web apps, but are packaged as apps for distribution and have access to native device APIs). [[link](https://en.wikipedia.org/wiki/Apache_Cordova)]
3. And Xamarin. It looks like win-win strategy if you already have web or desktop application written on .NET. You are able to share code, get native performance (almost, depends how you are creating application, Xamarin.Forms, for example, can create non the best implementation), access to all native API (almost). If there is a new version of OS, need to wait implementation in Xamarin up to one month.
4. [ReactNative](https://www.reactnative.com/) and [NativeScript](https://www.nativescript.org/) created and supported by Facebook and Telerik. ReactNative hasn’t final version still, but NativeScript has version 2.0. They are the most young libraries in the list. JavaScript is the language to write a code. But unlike Cordova transform Javascript elements to native UI elements. Support Android and IOS now. Microsoft is working to add UWP here (NativeScript). Looks like the most perspective platforms. You are able to use [Angular2 + Typescript + NativeScript or ReactJs + ReactNative](https://www.quora.com/What-are-the-key-difference-between-ReactNative-and-NativeScript/answer/Valentin-Stoychev) to write mobile applications and share code also with your web version of application. Probably it is the best frameworks if your application is web first.

[Xamarin features](https://www.xamarin.com/download-it)

* Produce ARM binary for Apple store
* Produce APK for Android
* Possibility to use only one IDE (Visual Studio)
* [Android Hyper-V]("https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/debug-on-emulator/visual-studio-android-emulator/) and IOS Remote emulators
* Designers for IOS, Android and Windows Phone in Visual Studio
* Xamarin Studio for Mac
* MVVM pattern (XAML)
