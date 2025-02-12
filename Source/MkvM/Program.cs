using Common;
using DataLayer;
using DataLayer.Implementations;
using DataLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MkvM.Core;


var workerConfiguration = new WorkerConfiguration()
{
    WorkingDirectory =                  EnvironmentHelper.LoadStringEnvironmentVariable(nameof(WorkerConfiguration.WorkingDirectory)),
    ConfigurationDirectory =            EnvironmentHelper.LoadStringEnvironmentVariable(nameof(WorkerConfiguration.ConfigurationDirectory)),
    DatabaseFile =                      EnvironmentHelper.LoadStringEnvironmentVariable(nameof(WorkerConfiguration.DatabaseFile)),
    ReplacementsFile =                  EnvironmentHelper.LoadStringEnvironmentVariable(nameof(WorkerConfiguration.ReplacementsFile)),
    TrackNamesFile =                    EnvironmentHelper.LoadStringEnvironmentVariable(nameof(WorkerConfiguration.TrackNamesFile)),
    ReplaceOriginal =                   EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.ReplaceOriginal)),
    OverwriteExisting =                 EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.OverwriteExisting)),
    IncludeAllExtensions =              EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.IncludeAllExtensions)),
    Extensions =                        EnvironmentHelper.LoadListEnvironmentVariable(nameof(WorkerConfiguration.Extensions)),
    IncludeSubFolders =                 EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.IncludeSubFolders)),
    UpdateListOfFilesProcessed =        EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.UpdateListOfFilesProcessed)),
    IgnoreListOfFilesProcessed =        EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.IgnoreListOfFilesProcessed)),
    TimeInMinutesBetweenExecutions =    EnvironmentHelper.LoadIntEnvironmentVariable(nameof(WorkerConfiguration.TimeInMinutesBetweenExecutions)),
    RenameMainVideoTitle =              EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.RenameMainVideoTitle)),
    ExtractTrackNamesOnly =             EnvironmentHelper.LoadBoolEnvironmentVariable(nameof(WorkerConfiguration.ExtractTrackNamesOnly))
};

var replacementsFilePath = Path.Combine(workerConfiguration.ConfigurationDirectory, workerConfiguration.ReplacementsFile);
workerConfiguration.Replacements = FileHelpers.GetFileContent(replacementsFilePath);

Console.WriteLine("Worker configuration:");
Console.WriteLine(workerConfiguration);
    
IHostBuilder host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Register services
        services.AddSingleton<IDataLayer, SqLiteDataLayer>();
        services.AddSingleton<WorkerConfiguration>(workerConfiguration);
        services.AddHostedService<MkvMWorker>();
    });
    
await host.Build().RunAsync();