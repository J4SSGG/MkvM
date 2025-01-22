using System.Text.Json;
using DataLayer.Models;

namespace MkvM.Core;


/// <summary>
/// This class contains functions to handle the execution of "mkvmerge" from the host system.
/// Currently, there are 4 functions:
///     ExecuteCommands: this is the main function to invoke "mkvmerge". All other functions call this one!
///     ExtractTracks: this function extracts the tracks from a given file.
///     ExtractMetadata: this function extracts the metadata from a given file.
///     
/// </summary>
public static class MkvMergeHandler
{
    public static string ExecuteCommands(string fileName, string outputFileName, string[]? commands = null)
    {
        //Create process
        System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

        //strCommand is path and file name of command to run
        pProcess.StartInfo.FileName = "mkvmerge";

        //strCommandParameters are parameters to pass to program
        pProcess.StartInfo.Arguments = $"{outputFileName} {string.Join(" ", commands ?? [])} \"{fileName}\"";

        pProcess.StartInfo.UseShellExecute = false;

        //Set output of program to be written to process output stream
        pProcess.StartInfo.RedirectStandardOutput = true;   

        //Optional
        pProcess.StartInfo.WorkingDirectory = "./";

        //Start the process
        pProcess.Start();

        //Get program output
        string strOutput = pProcess.StandardOutput.ReadToEnd();

        //Wait for process to finish
        pProcess.WaitForExit();
    
        return strOutput;
    }

    public static List<Track>? ExtractTracks(string file)
    {
        var metadata = ExtractMetadata(file);
        return metadata?.tracks;
    }

    public static MkvMetadata? ExtractMetadata(string file)
    {
        string metadata = String.Empty;
        try
        {
            metadata = ExecuteCommands(file, null!, [Commands.BuildGetMetadataCommand()]);

            if (string.IsNullOrEmpty(metadata)) return default;

            return JsonSerializer.Deserialize<MkvMetadata>(metadata,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error extracting metadata from file: " + file);
            Console.WriteLine("Metadata: \n" + metadata);
            Console.WriteLine(e);
            return default;
        }
    }

    public static string RepackFile(string file, List<string> commands, bool replaceOriginal = false, bool overwriteExisting = false)
    {
        var fileDirectory = Path.GetDirectoryName(file);
        var fileName = Path.GetFileNameWithoutExtension(file);
        var fileExt = ".mkv"; // this is the default extension for the output file since we are working with mkvmerge.
        
        string tempFileName = "MkvM_" + DateTime.Now.Ticks + "_" + fileName + fileExt;
        
        if (replaceOriginal)
        {
            overwriteExisting = true;
        }
        else
        {
            // this is here in case we ever support custom output file names.
        }
        
        if (string.Compare(tempFileName, fileName, StringComparison.OrdinalIgnoreCase) == 0 && !replaceOriginal) // just in case
        {
            Console.WriteLine("Output name is the same as the original file name and ReplaceOriginal is false. The output file will be prepended with 'MkvM_{datetime}'");
            tempFileName = "MkvM_" + DateTime.Now.Ticks + "_" + fileName + fileExt;
        }
            
        if (File.Exists(Path.Combine(fileDirectory, tempFileName)) && !overwriteExisting) // we valide output file existence when OverwriteExisting is false
        {
            Console.WriteLine("Output file already exists and OverwriteExisting is false. The output file will be prepended with 'MkvM_{datetime}'");
            tempFileName = "MkvM_" + DateTime.Now.Ticks + "_" + fileName + fileExt;
        }

        tempFileName = Path.Combine(fileDirectory, tempFileName);


        var outputFileName = Commands.BuildOutputFileNameCommand(tempFileName);

        ExecuteCommands(file, outputFileName, commands.ToArray());
        
        if (replaceOriginal)
        {
            File.Move(tempFileName, file, overwriteExisting);
        }
        
        return tempFileName;
    }
}