using System.ComponentModel;
using System.Text;
using Frends.Zip.CreateArchive.Definitions;
using Frends.Zip.CreateArchive.Helpers;
using Ionic.Zip;

namespace Frends.Zip.CreateArchive;

/// <summary>
/// Frends ZIP task.
/// </summary>
public static class Zip
{
    /// <summary>
    /// Create zip file from selected files and/or folders. Created zip file content can be flatten and file can be protected with password.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Zip-CreateArchive)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Path, int FileCount, List&lt;string&gt; ArchivedFiles, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result CreateArchive(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate that source and destination folders exist.
            if (!Directory.Exists(input.SourceDirectory) && input.SourceType == SourceFilesType.PathAndFileMask)
                throw new DirectoryNotFoundException($"Source directory {input.SourceDirectory} does not exist.");
            if (!Directory.Exists(input.DestinationDirectory) && !options.CreateDestinationFolder)
                throw new DirectoryNotFoundException($"Destination directory {input.DestinationDirectory} does not exist.");

            var sourceFiles = new List<string>();
            var encoding = GetEncoding(options.Encoding, options.EncodingInString, options.EnableBom);

            // Populate source files list according to input type.
            switch (input.SourceType)
            {
                case SourceFilesType.PathAndFileMask:
                    sourceFiles = Directory.EnumerateFiles(input.SourceDirectory, input.FileMask, input.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                    break;
                case SourceFilesType.FileList:
                    sourceFiles = input.FilePathsList;
                    break;
            }

            // If no files were found, throw error or return empty Output object.
            if (sourceFiles.Count == 0)
            {
                if (options.ThrowErrorIfNoFilesFound)
                    throw new FileNotFoundException($"No files found in {input.SourceDirectory} with file mask '{input.FileMask}'");

                return new Result
                {
                    Success = true,
                    Path = string.Empty,
                    FileCount = 0,
                    ArchivedFiles = [],
                    Error = null,
                };
            }

            // Check if destination directory exist and if it should be created.
            if (options.CreateDestinationFolder && !Directory.Exists(input.DestinationDirectory))
                Directory.CreateDirectory(input.DestinationDirectory);

            var destinationZipFileName = Path.Combine(input.DestinationDirectory, input.DestinationFileName);

            // Check if destination zip exists.
            var destinationZipExists = File.Exists(destinationZipFileName);
            if (destinationZipExists)
            {
                switch (options.DestinationFileExistsAction)
                {
                    case FileExistAction.Error:
                        throw new Exception($"Destination file {destinationZipFileName} already exists.");

                    case FileExistAction.Rename:
                        destinationZipFileName = GetRenamedZipFileName(destinationZipFileName);
                        break;
                }
            }

            // Either create a new zip file or open existing one if Append was selected.
            using var zipFile = (destinationZipExists && options.DestinationFileExistsAction == FileExistAction.Append) ? ZipFile.Read(destinationZipFileName) : new ZipFile(encoding);

            // Set 'UseZip64WhenSaving' - needed for large zip files.
            zipFile.UseZip64WhenSaving = options.UseZip64.ConvertEnum<Zip64Option>();

            // If password is given, add it to archive.
            if (!string.IsNullOrWhiteSpace(input.Password)) zipFile.Password = input.Password;

            foreach (var fullPath in sourceFiles)
            {
                // Check if cancellation is requested.
                cancellationToken.ThrowIfCancellationRequested();

                // FlattenFolders = true: add all files to zip root, otherwise adda folders to zip.
                // Only available when source type is path and filemask
                var pathInArchive = (input.FlattenFolders || input.SourceType == SourceFilesType.FileList) ? string.Empty : fullPath.GetRelativePath(input.SourceDirectory);

                AddFileToZip(zipFile, fullPath, pathInArchive, input.RenameDuplicateFiles);
            }

            // Save zip (overwites existing file).
            zipFile.Save(destinationZipFileName);

            // Remove source files?
            foreach (var fullPath in sourceFiles) if (input.RemoveZippedFiles) File.Delete(fullPath);

            return new Result
            {
                Success = true,
                Path = destinationZipFileName,
                FileCount = zipFile.Count,
                ArchivedFiles = zipFile.EntryFileNames.ToList(),
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    internal static Encoding GetEncoding(FileEncoding encoding, string encodingString, bool enableBom)
    {
        return encoding switch
        {
            FileEncoding.UTF8 => enableBom ? new UTF8Encoding(true) : new UTF8Encoding(false),
            FileEncoding.ASCII => new ASCIIEncoding(),
            FileEncoding.ANSI => Encoding.Default,
            FileEncoding.WINDOWS1252 => CodePagesEncodingProvider.Instance.GetEncoding("windows-1252"),
            FileEncoding.Other => CodePagesEncodingProvider.Instance.GetEncoding(encodingString),
            _ => throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'."),
        };
    }

    private static void AddFileToZip(ZipFile zipFile, string fullPath, string pathInArchive, bool renameDublicateFile)
    {
        // Check is file with same name alredy added.
        if (zipFile.ContainsEntry(Path.GetFileName(fullPath)))
        {
            if (renameDublicateFile) RenameAndAddFile(zipFile, fullPath);
            else throw new Exception($"File {fullPath} already exists in zip!");
        }
        else
        {
            zipFile.AddFile(fullPath, pathInArchive);
        }
    }

    private static void RenameAndAddFile(ZipFile zipFile, string filePath)
    {
        var renamedFileName = GetRenamedFileName(zipFile.EntryFileNames, Path.GetFileName(filePath));
        zipFile.AddEntry(renamedFileName, File.ReadAllBytes(filePath));
    }

    private static string GetRenamedFileName(ICollection<string> existingFileNames, string fileName)
    {
        var index = 1;
        var renamedFile = fileName.RenameFile(index);
        while (existingFileNames.Contains(renamedFile))
        {
            index++;
            renamedFile = fileName.RenameFile(index);
        }

        return renamedFile;
    }

    private static string GetRenamedZipFileName(string fullPath)
    {
        var index = 1;
        var renamedFile = Path.GetFileName(fullPath).RenameFile(index);
        var path = Path.GetDirectoryName(fullPath);
        var renamedFileFullPath = Path.Combine(path, renamedFile);
        while (File.Exists(renamedFileFullPath))
        {
            index++;
            renamedFile = Path.GetFileName(fullPath).RenameFile(index);
            renamedFileFullPath = Path.Combine(path, renamedFile);
        }

        return renamedFileFullPath;
    }
}
