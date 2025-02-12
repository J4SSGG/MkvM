namespace DataLayer.Models;

public class WorkerConfiguration
{
    public string WorkingDirectory { get; set; } // The main folder to be explored for movies and videos.
    public string ConfigurationDirectory { get; set; } // The main folder to read and save configuration files.
    public string DatabaseFile { get; set; } // The name for the SQLiteDataLayer database file.
    public string ReplacementsFile { get; set; } // The name for the Replacements file.
    public string TrackNamesFile { get; set; } // The name for the Track Names file. 
    public int TimeInMinutesBetweenExecutions { get; set; } // Default is 60 minutes.
    public bool ReplaceOriginal { get; set; } // Whether to replace the original files with the processed ones, or leave them untouched.
    public bool OverwriteExisting { get; set; } // Whether to overwrite existing files with the same name as the output file. Original files are never overwritten with this setting.
    public bool IncludeAllExtensions { get; set; } // Whether to include all files in the WorkingDirectory, regardless of their extension. Default is false. Enable this only when you are sure all files in the WorkingDirectory are supported files.
    public IEnumerable<string> Extensions { get; set; } // A list of extensions to be processed. If IncludeAllExtensions is true, this list is ignored, required otherwise. Case-sensitive. The default is a list of supported extensions by MkvM.
    public bool IncludeSubFolders { get; set; } // Whether to include subfolders in the WorkingDirectory. Default is false.
    public bool UpdateListOfFilesProcessed { get; set; } // Save in the database the list of files processed. Default is true. If false, the list of files processed will not be updated.
    public bool IgnoreListOfFilesProcessed { get; set; } // Ignore the list of files processed. Default is false. If true, the list of files processed will not be checked (all files will be (re)processed).
    public IEnumerable<string> Replacements { get; set; } // The list of "values" to be replaced in the file tracks. The format is "old_value:new_value". If the list is empty, no replacements will be made (no files will be processed).
    public bool RenameMainVideoTitle { get; set; } // Whether to rename the main video title with the file name. Default is false.
    public bool ExtractTrackNamesOnly { get; set; } // Whether to extract the track names only. Default is false. If true, a file with the track names will be created and no files will be processed.
    public override string ToString()
    {
        return $"WorkingDirectory: {WorkingDirectory}\n" +
               $"ConfigurationDirectory: {ConfigurationDirectory}\n" +
               $"DatabaseFile: {DatabaseFile}\n" +
               $"ReplacementsFile: {ReplacementsFile}\n" +
               $"TrackNamesFile: {TrackNamesFile}\n" +
               $"TimeInMinutesBetweenExecutions: {TimeInMinutesBetweenExecutions}\n" +
               $"ReplaceOriginal: {ReplaceOriginal}\n" +
               $"OverwriteExisting: {OverwriteExisting}\n" +
               $"IncludeAllExtensions: {IncludeAllExtensions}\n" +
               $"Extensions: {string.Join(", ", Extensions)}\n" +
               $"IncludeSubFolders: {IncludeSubFolders}\n" +
               $"UpdateListOfFilesProcessed: {UpdateListOfFilesProcessed}\n" +
               $"IgnoreListOfFilesProcessed: {IgnoreListOfFilesProcessed}\n" +
               $"RenameMainVideoTitle: {RenameMainVideoTitle}\n" +
               $"ExtractTrackNamesOnly: {ExtractTrackNamesOnly}\n";
    }
}