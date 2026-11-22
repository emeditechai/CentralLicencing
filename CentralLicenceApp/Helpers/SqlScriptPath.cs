namespace CentralLicenceApp.Helpers
{
    /// <summary>
    /// Locates a file in SqlScripts/ both when running from source (dotnet run → bin/Debug/net8.0)
    /// and when running a published build (scripts are copied next to the app via the csproj).
    /// </summary>
    public static class SqlScriptPath
    {
        public static string Resolve(string fileName)
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "SqlScripts", fileName),                    // published / build output
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", fileName),  // dotnet run from source
                Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", fileName)              // content root
            };
            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }
    }
}
