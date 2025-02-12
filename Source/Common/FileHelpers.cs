namespace Common;

public static class FileHelpers
{
    
    /// <summary>
    /// Searches for all files in a given directory and its subdirectories.
    /// </summary>
    /// <param name="directory">The parent folder to be explored.</param>
    /// <returns>A list including the full path of all files found.</returns>
    public static IEnumerable<string> GetAllFilesInDirectory(string directory, bool includeSubFolders = false)
    {
        return Directory.GetFiles(directory, "*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }
    
    /// <summary>
    /// Searches for all files in a given directory and its subdirectories whose extensions match within a group of extensions.
    /// The extension param can receive a file name with the extension or just the extension.
    /// </summary>
    /// <param name="directory">The parent folder to be explored.</param>
    /// <param name="extensions">A collection of extensions of filenames with extensions including the dot (without quotes): ".mkv", "movie.mkv".</param>
    /// <returns>A list including the full path of all files found matching the extensions passed.</returns>
    public static IEnumerable<string> GetAllFilesInDirectoryWithExtensions(string directory, IEnumerable<string> extensions, bool includeSubFolders = false)
    {
        return Directory
            .EnumerateFiles(directory, "*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(file => extensions.Contains(Path.GetExtension(file)));
    }
    
    /// <summary>
    /// Loads a file and returns its content as list of strings, each string item being a line in the file.
    /// </summary>
    /// <param name="directory">The parent folder to be explored.</param>
    /// <param name="extensions">The file to locate by name, including extension</param>
    /// <returns>A list of strings. If file has no content or cannot be read it returns an empty list. If file does not exist it returns null</returns>
    public static IEnumerable<string>? GetFileContent(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            return null;
        };
        
        // read file
        try
        {
            var content = File.ReadAllLines(path);
            return content.Distinct(); // remove duplicates
        }
        catch (Exception e)
        {
            // any exception, return empty list
            Console.WriteLine(e);
            return Enumerable.Empty<string>();
        }
    }
}