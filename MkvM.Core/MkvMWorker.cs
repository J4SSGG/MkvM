using Common;
using DataLayer;
using DataLayer.Models;
using Microsoft.Extensions.Hosting;

namespace MkvM.Core;

public class MkvMWorker : IHostedService
{
    private readonly IDataLayer _dataLayer;
    private readonly WorkerConfiguration _workerConfiguration;
    
    public MkvMWorker(IDataLayer dataLayer, WorkerConfiguration workerConfiguration)
    {
        _dataLayer = dataLayer;
        _workerConfiguration = workerConfiguration;
    }
    
    public void Process(CancellationToken cancellationToken)
    {
        
        IEnumerable<string> files;
        Dictionary<string, List<Track>> tracks = new();
        Dictionary<string, List<string>> commands = new();
        
        // Get all files in directory based on configuration
        if (_workerConfiguration.IncludeAllExtensions)
        {
            files = FileHelpers.GetAllFilesInDirectory(_workerConfiguration.WorkingDirectory, _workerConfiguration.IncludeSubFolders);
        }
        else
        {
            files = FileHelpers.GetAllFilesInDirectoryWithExtensions(_workerConfiguration.WorkingDirectory, _workerConfiguration.Extensions, _workerConfiguration.IncludeSubFolders);
        }
        
        if (!files.Any())
        {
            if (_workerConfiguration.IncludeSubFolders)
            {
                Console.WriteLine("No new files were found in this directory or any of its subdirectories: " + _workerConfiguration.WorkingDirectory);
            }
            else
            {
                Console.WriteLine("No new files found in this directory: " + _workerConfiguration.WorkingDirectory);
            }
            Console.WriteLine("If this directory has files, check the configuration. Common errors may be related to the extensions or the IncludeAllExtensions flag.");
            return;
        }
        
        // Extract tracks from each file
        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (_workerConfiguration.IgnoreListOfFilesProcessed || !_dataLayer.IsFileProcessed(file))
            {
                tracks.Add(file, MkvMergeHandler.ExtractTracks(file));
            }
            else
            {
                Console.WriteLine("File already processed: " + file);
            }
        }
        
        // Process tracks
        if (cancellationToken.IsCancellationRequested) return;
        ProcessTrack(ref tracks, ref commands);
        
        // Execute commands
        foreach (var command in commands)
        {
            if (cancellationToken.IsCancellationRequested) return;
            Console.WriteLine("Processing file: " + command.Key);
            string repackedFileName = MkvMergeHandler.RepackFile(command.Key, command.Value, _workerConfiguration.ReplaceOriginal, _workerConfiguration.OverwriteExisting);
            if (_workerConfiguration.UpdateListOfFilesProcessed)
            {
                // Save processed files
                _dataLayer.SaveProcessedFile(command.Key);
                if (repackedFileName != command.Key)
                {
                    // also saved the repacked file to avoid processing it again in the future runs
                    _dataLayer.SaveProcessedFile(repackedFileName);
                }
            }
            Console.WriteLine("Done");
        }
    }

    public void ProcessTrack(ref Dictionary<string, List<Track>> tracks, ref Dictionary<string, List<string>> commands)
    {
        foreach (var key in tracks.Keys)
        {
            Console.WriteLine("Reading file: " + key);
            
            // 1. Rename file title 
            List<string> _commands = new();
            var newTitle = Path.GetFileNameWithoutExtension(key);
            _commands.Add(Commands.BuildSetContainerTitleCommand(newTitle));

            // 2. Rename each track name
            var videoTracks = tracks[key].Where(x => x.type == "video");
            var audioTracks = tracks[key].Where(x => x.type == "audio");
            var subtitlesTracks = tracks[key].Where(x => x.type == "subtitles");

            foreach (var track in videoTracks)
            {
                _commands.Add(Commands.BuildSetContainerTitleCommand(Path.GetFileNameWithoutExtension(key)));
            }

            foreach (var track in audioTracks)
            {
                var sanitizedTrackName = Common.StringHelpers.Sanitize(track.properties.track_name, _workerConfiguration.Replacements.ToArray());
                var command = Commands.BuildSetTrackNameCommand(track.id, sanitizedTrackName);
                _commands.Add(command);
            }
    
            foreach (var track in subtitlesTracks)
            {
                var sanitizedTrackName = Common.StringHelpers.Sanitize(track.properties.track_name, _workerConfiguration.Replacements.ToArray());
                var command = Commands.BuildSetTrackNameCommand(track.id, sanitizedTrackName);
                _commands.Add(command);
            }
    
            commands.Add(key, _commands);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        do
        {
            Process(cancellationToken); 
            Thread.Sleep(TimeSpan.FromMinutes(_workerConfiguration.TimeInMinutesBetweenExecutions));
        } while (!cancellationToken.IsCancellationRequested);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do here
        return Task.CompletedTask;
    }
}