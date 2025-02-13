﻿using System.Text;
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
                Console.WriteLine("Adding file to process: " + file);
                var fileTracks = MkvMergeHandler.ExtractTracks(file);
                if (fileTracks != null)
                {
                    tracks.Add(file, fileTracks);
                }
                else
                {
                    Console.WriteLine("No tracks found in file: " + file);
                }
            }
            else
            {
                Console.WriteLine("File already processed: " + file);
            }
        }
        
        
        if (_workerConfiguration.ExtractTrackNamesOnly)
        {
            Console.WriteLine("Extracting track names only...");
            
            var trackNames = tracks.SelectMany(x => x.Value.Select(t => t.properties.track_name)).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            
            // select distinct
            trackNames = trackNames.Distinct().ToArray();
            
            // Sort longest first
            Array.Sort(trackNames, (a, b) => a.Length < b.Length ? 1 : -1);
            
            string content = string.Join("\n", trackNames);
            
            Console.WriteLine("Track names extracted: {0}", trackNames.Length);
            
            string outputFileName = Path.Combine(_workerConfiguration.ConfigurationDirectory, _workerConfiguration.TrackNamesFile);

            if (File.Exists(outputFileName))
            {
                Console.WriteLine("Removing existing file: " + outputFileName);
                File.Delete(outputFileName);
            }
            
            Console.WriteLine("Saving track names to file: " + outputFileName);
            File.WriteAllText(outputFileName, content, Encoding.UTF8);
            
            Console.WriteLine("Track names saved to file: " + outputFileName);
            return;
        } else { /*continue*/ }
        
        // Process tracks
        if (cancellationToken.IsCancellationRequested) return;
        ProcessTracks(ref tracks, ref commands);
        
        // Execute commands
        foreach (var command in commands)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested) return;
                Console.WriteLine("Repacking file: " + command.Key);
                string repackedFileName = MkvMergeHandler.RepackFile(command.Key, command.Value,
                    _workerConfiguration.ReplaceOriginal, _workerConfiguration.OverwriteExisting);
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

                Console.WriteLine("Repacking finished: " + command.Key);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error processing file: " + command.Key);
                Console.WriteLine(e);
            }
        }
    }

    public void ProcessTracks(ref Dictionary<string, List<Track>> tracks, ref Dictionary<string, List<string>> commands)
    {
        foreach (var key in tracks.Keys)
        {
            Console.WriteLine("Processing file: " + key);
            
            bool requiresProcessing = false; 
            if (!_workerConfiguration.RenameMainVideoTitle) // In case we do not have to process the main track, it is worth checking if we need to process the rest
            {
                Console.WriteLine("Renaming main video title is disabled. Checking if we need to process the tracks...");
                // Validate if we even need to process the tracks
                
                // 1. Get all track names for the current file
                var trackNames = tracks[key].Select(t => t.properties.track_name);
                
                // 2. Check if any track requires processing (it contains any of the replacements value from the replacements file)
                foreach (var track in trackNames)
                {
                    if (StringHelpers.RequiresSanitization(track, _workerConfiguration.Replacements))
                    {
                        requiresProcessing = true;
                        break;
                    }
                }
            }
        
            if (!requiresProcessing && !_workerConfiguration.RenameMainVideoTitle)
            {
                // This avoids unnecessary processing of files (rewriting the same file with the same content)
                Console.WriteLine("No tracks require processing. Skipping file...");
                continue;
            }
            
            List<string> _commands = new();
            
            // 1. Rename file title 
            if (_workerConfiguration.RenameMainVideoTitle)
            {
                var newTitle = Path.GetFileNameWithoutExtension(key);
                _commands.Add(Commands.BuildSetContainerTitleCommand(newTitle));
            }

            // 2. Rename each track name
            var videoTracks = tracks[key].Where(x => x.type == "video");
            var audioTracks = tracks[key].Where(x => x.type == "audio");
            var subtitlesTracks = tracks[key].Where(x => x.type == "subtitles");

            foreach (var track in videoTracks)
            {
                Console.WriteLine("Processing video track: " + track.id);
                _commands.Add(Commands.BuildSetContainerTitleCommand(Path.GetFileNameWithoutExtension(key)));
            }

            foreach (var track in audioTracks)
            {
                Console.WriteLine("Processing audio track: " + track.id);
                var sanitizedTrackName = Common.StringHelpers.Sanitize(track.properties.track_name, _workerConfiguration.Replacements);
                var command = Commands.BuildSetTrackNameCommand(track.id, sanitizedTrackName);
                _commands.Add(command);
            }
    
            foreach (var track in subtitlesTracks)
            {
                Console.WriteLine("Processing subtitle track: " + track.id);
                var sanitizedTrackName = Common.StringHelpers.Sanitize(track.properties.track_name, _workerConfiguration.Replacements);
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