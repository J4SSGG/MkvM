namespace Common;

public static class StringHelpers
{
    public static string Sanitize(string input, string[] valuesToRemove, StringComparison matchType = StringComparison.OrdinalIgnoreCase)
    {
        if (input is null) return string.Empty;
        
        foreach (var value in valuesToRemove)
        {
            input = input.Replace(value, "", matchType);
        }
        return input;
    }
}