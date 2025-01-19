
using Common;
using DataLayer.Implementations;
using DataLayer.Models;
using MkvM.Core;
//**
// This software is in beta. Use at your own risk!
//**/
var workerConfiguration = new WorkerConfiguration()
{
    WorkingDirectory = "/home/abraham/Videos",
    DatabaseFile = "/home/abraham/Videos/database.sqlite",
    ReplacementsFile = "/home/abraham/Videos/replacements.txt",
    ReplaceOriginal = true,
    OverwriteExisting = false,
    IncludeAllExtensions = false,
    Extensions = new List<string>() { ".mkv", ".mp4", ".avi" },
    IncludeSubFolders = true,
    UpdateListOfFilesProcessed = true,
    IgnoreListOfFilesProcessed = false,
    Replacements = FileHelpers.GetFileContent("/home/abraham/Videos/replacements.txt"),
    TimeInMinutesBetweenExecutions = 0 // not used in this Program.cs
};
var database = new SqLiteDataLayer(workerConfiguration);
var worker = new MkvMWorker(database, workerConfiguration);

worker.Process(CancellationToken.None);