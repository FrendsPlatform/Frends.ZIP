using System.ComponentModel;
using Frends.Zip.ExtractArchive.Definitions;
using Frends.Zip.ExtractArchive.Helpers;
using Ionic.Zip;

namespace Frends.Zip.ExtractArchive;

/// <summary>
/// ZIP task.
/// </summary>
public static class Zip
{
    /// <summary>
    /// A Frends task for extracting zip archives.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.Zip.ExtractArchive)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, List&lt;string&gt; ExtractedFiles, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result ExtractArchive(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(input.SourceFile))
                throw new FileNotFoundException($"Source file {input.SourceFile} does not exist.");
            if (!Directory.Exists(input.DestinationDirectory) && !options.CreateDestinationDirectory)
                throw new DirectoryNotFoundException($"Destination directory {input.DestinationDirectory} does not exist.");

            var destinationRoot = Path.GetFullPath(input.DestinationDirectory);
            var destinationRootWithSeparator = Path.EndsInDirectorySeparator(destinationRoot)
                ? destinationRoot
                : destinationRoot + Path.DirectorySeparatorChar;
            var pathComparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (options.CreateDestinationDirectory) Directory.CreateDirectory(destinationRoot);

            var extractedFiles = new List<string>();

            using (var zip = ZipFile.Read(input.SourceFile))
            {
                if (!string.IsNullOrWhiteSpace(input.Password)) zip.Password = input.Password;

                foreach (var z in zip)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (z.Attributes.HasFlag(FileAttributes.Directory)) continue;

                    string normalizedEntryPath = z.FileName
                        .Replace('/', Path.DirectorySeparatorChar)
                        .Replace('\\', Path.DirectorySeparatorChar);

                    if (Path.IsPathRooted(normalizedEntryPath)
                        || (normalizedEntryPath.Length >= 2
                            && char.IsLetter(normalizedEntryPath[0])
                            && normalizedEntryPath[1] == ':'))
                    {
                        throw new IOException($"Archive entry path is rooted: {z.FileName}");
                    }

                    var targetFilePath = Path.GetFullPath(Path.Combine(destinationRoot, normalizedEntryPath));
                    if (!targetFilePath.StartsWith(destinationRootWithSeparator, pathComparison))
                        throw new IOException($"Archive entry escapes the destination directory: {z.FileName}");

                    if (File.Exists(targetFilePath))
                    {
                        if (options.DestinationFileExistsAction == UnzipFileExistAction.Error)
                        {
                            throw new IOException($"Destination file already exists: {targetFilePath}");
                        }

                        if (options.DestinationFileExistsAction == UnzipFileExistAction.Rename)
                        {
                            targetFilePath = Extensions.GetNewFilename(
                                targetFilePath,
                                cancellationToken);
                        }
                    }

                    string directory = Path.GetDirectoryName(targetFilePath);
                    if (directory != null) Directory.CreateDirectory(directory);

                    using var fs = File.Create(targetFilePath);
                    z.Extract(fs);
                    extractedFiles.Add(targetFilePath);
                }
            }

            if (options.DeleteZipFileAfterExtract)
            {
                try
                {
                    File.Delete(input.SourceFile);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"Extraction was completed but an exception was thrown when trying to delete the source file: {ex.Message}");
                }
            }

            return new Result
            {
                Success = true,
                ExtractedFiles = extractedFiles,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }
}
