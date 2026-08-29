namespace TraceLock.Desktop;

public static class AppPaths
{
    private const string AppFolderName = "TraceLock";

    public static string BaseDirectory { get; private set; } = string.Empty;
    public static string DataDirectory => Path.Combine(BaseDirectory, "App_Data");
    public static string EvidenceStorage => Path.Combine(DataDirectory, "EvidenceStorage");

    public static void Initialize()
    {
        BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);

        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(EvidenceStorage);

        var bundledEvidence = Path.Combine(AppContext.BaseDirectory, "App_Data", "EvidenceStorage");
        if (Directory.Exists(bundledEvidence))
        {
            CopyDirectory(bundledEvidence, EvidenceStorage);
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(
                Path.Combine(destination, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            if (!File.Exists(target))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, false);
            }
        }
    }
}


