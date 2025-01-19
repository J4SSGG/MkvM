namespace MkvM.Core;

public static class Commands
{
    public static string BuildGetMetadataCommand()
    {
        return $"-J";
    }

    public static string BuildOutputFileNameCommand(string outputFileName)
    {
        return $"-o \"{outputFileName}\"";
    }

    public static string BuildSetContainerTitleCommand(string title)
    {
        return $"--title \"{title}\"";
    }

    public static string BuildSetTrackNameCommand(ulong trackId, string trackName)
    {
        return $"--track-name {trackId}:\"{trackName}\"";
    }
}