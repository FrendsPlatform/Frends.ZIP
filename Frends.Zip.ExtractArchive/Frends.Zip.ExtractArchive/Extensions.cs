namespace Frends.Zip.ExtractArchive;

/// <summary>
/// Extensions class.
/// </summary>
internal static class Extensions
{
    internal static string GetNewFilename(string path, CancellationToken cancellationToken)
    {
        var index = 1;
        string newPath;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var newFilename = $"{Path.GetFileNameWithoutExtension(path)}({index}){Path.GetExtension(path)}";
            newPath = Path.Combine(Path.GetDirectoryName(path), newFilename);
            index++;
        }
        while (File.Exists(newPath));
        return newPath;
    }
}
