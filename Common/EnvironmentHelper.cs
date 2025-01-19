namespace Common;

public static class EnvironmentHelper
{
    // We need to load "user settings" from the Docker container
    // User will set these values in the docker-compose file, as environment variables
    // We will use these values to configure the application
    // (╯°□°）╯︵ ┻━┻  -- Not really my favorite code
    
    public static string? LoadStringEnvironmentVariable(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName);
    }
    
    public static bool LoadBoolEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        bool.TryParse(value, out var result);
        return result;
    }
    
    public static int LoadIntEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        int.TryParse(value, out var result);
        return result;
    }
    
    public static IEnumerable<string>? LoadListEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value)) return Enumerable.Empty<string>();
        return value?.Split(',');
    }
}