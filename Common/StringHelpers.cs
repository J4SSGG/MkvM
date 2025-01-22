namespace Common;

public static class StringHelpers
{
    public static string Sanitize(string input, IEnumerable<string> valuesToRemove, StringComparison matchType = StringComparison.OrdinalIgnoreCase)
    {
        if (input is null) return string.Empty;
        
        foreach (var value in valuesToRemove)
        {
            input = input.Replace(value, "", matchType);
        }
        return input;
    }
    
    public static bool RequiresSanitization(string input, IEnumerable<string> valuesToRemove, StringComparison matchType = StringComparison.OrdinalIgnoreCase)
    {
        if (input is null) return false;
        
        foreach (var value in valuesToRemove)
        {
            if (input.Contains(value, matchType))
            {
                return true;
            }
        }
        return false;
    }
}