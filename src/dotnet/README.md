Had massive issues getting Azure Functions with .NET 6.0 to work. Kept getting issues with my startup class.

AZFD0005
Diagnostic event
Error code
AZFD0005
Level
Error
Message
Error configuring services in an external startup class.
Details
Microsoft.Azure.WebJobs.Script.ExternalStartupException : Error configuring services in an external startup class. ---> System.IO.FileNotFoundException : Could not load file or assembly 'Microsoft.Extensions.Http, Version=7.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'. The system cannot find the file specified. at GitHubCrawler.Startup.Configure(IFunctionsHostBuilder builder) at Microsoft.Azure.WebJobs.WebJobsBuilderExtensions.ConfigureStartup(IWebJobsStartup startup,WebJobsBuilderContext context,IWebJobsBuilder builder) at D:\a\_work\1\s\src\Microsoft.Azure.WebJobs.Host\Hosting\WebJobsBuilderExtensions.cs : 162 at Microsoft.Azure.WebJobs.WebJobsBuilderExtensions.ConfigureAndLogUserConfiguredServices(IWebJobsStartup startup,WebJobsBuilderContext context,IWebJobsBuilder builder,ILoggerFactory loggerFactory) at D:\a\_work\1\s\src\Microsoft.Azure.WebJobs.Host\Hosting\WebJobsBuilderExtensions.cs : 130 at Microsoft.Azure.WebJobs.WebJobsBuilderExtensions.UseWebJobsStartup(IWebJobsBuilder builder,Type startupType,WebJobsBuilderContext context,ILoggerFactory loggerFactory) at D:\a\_work\1\s\src\Microsoft.Azure.WebJobs.Host\Hosting\WebJobsBuilderExtensions.cs : 115 at Microsoft.Azure.WebJobs.WebJobsBuilderExtensions.UseExternalStartup(IWebJobsBuilder builder,IWebJobsStartupTypeLocator startupTypeLocator,WebJobsBuilderContext context,ILoggerFactory loggerFactory) at D:\a\_work\1\s\src\Microsoft.Azure.WebJobs.Host\Hosting\WebJobsBuilderExtensions.cs : 213 at Microsoft.Azure.WebJobs.Script.ScriptHostBuilderExtensions.<>c__DisplayClass7_0.<AddScriptHostCore>b__1(HostBuilderContext context,IWebJobsBuilder webJobsBuilder) at /src/azure-functions-host/src/WebJobs.Script/ScriptHostBuilderExtensions.cs : 226 End of inner exception

https://azurelessons.com/could-not-load-file-or-assembly-microsoft-extensions-dependencyinjection-abstractions/

dotnet user-secrets init
dotnet user-secrets set "QUEUE_CONNECTION_STRING" "FOO"
dotnet user-secrets set "STORAGE_CONNECTION_STRING" "FOO"