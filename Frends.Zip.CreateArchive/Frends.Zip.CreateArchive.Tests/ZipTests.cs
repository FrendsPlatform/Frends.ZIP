using Frends.Zip.CreateArchive.Definitions;
using Ionic.Zip;
using NUnit.Framework;

namespace Frends.Zip.CreateArchive.Tests;

[TestFixture]
public class ZipTests
{
    private static readonly string BasePath = Path.Combine(Path.GetTempPath(), "frends.zip.createarchive.tests");

    private readonly string dirIn = Path.Combine(BasePath, $"In{Path.DirectorySeparatorChar}");
    private readonly string subDir = "Subdir";
    private readonly string dirOut = Path.Combine(BasePath, $"Out{Path.DirectorySeparatorChar}");
    private readonly string zipFileName = "zip_test.zip";

    private string subDirIn;
    private Input input;
    private Options options;

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(BasePath, true);
    }

    [SetUp]
    public void SetupTests()
    {
        subDirIn = Path.Combine(dirIn, subDir);
        input = new Input
        {
            SourceDirectory = dirIn,
            FileMask = "*.txt",
            IncludeSubFolders = false,
            FlattenFolders = false,
            DestinationDirectory = dirOut,
            DestinationFileName = zipFileName,
            Password = string.Empty,
            RenameDuplicateFiles = false,
        };
        options = new Options { ThrowErrorIfNoFilesFound = true, CreateDestinationFolder = false, DestinationFileExistsAction = FileExistAction.Error, ThrowErrorOnFailure = true };

        // Create source directory and files.
        Directory.CreateDirectory(dirIn);
        File.WriteAllText(Path.Combine(dirIn, "test_1_file.txt"), "foobaar foobar");
        File.WriteAllText(Path.Combine(dirIn, "test_2_file.txt"), "foobaar foobar");

        // Create subdirectory for recursive test.
        Directory.CreateDirectory(subDirIn);
        File.WriteAllText(Path.Combine(subDirIn, "sub_test_1_file.txt"), "foobaar foobar");
        File.WriteAllText(Path.Combine(subDirIn, "sub_test_2_file.txt"), "foobaar foobar");

        Directory.CreateDirectory(dirOut);
    }

    [Test]
    public void ZipFiles_ReturnsErrorInResult_WhenThrowErrorOnFailureIsFalse()
    {
        input.SourceDirectory = Path.Combine(dirIn, "foobar");
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Archive creation failed";

        var result = ExecuteCreateArchive();

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Error.Message.StartsWith("Archive creation failed: Source directory"));
        Assert.IsInstanceOf<DirectoryNotFoundException>(result.Error.AdditionalInfo);
    }

    [Test]
    public void ZipFiles_NonRecursive()
    {
        var result = ExecuteCreateArchive();

        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(2, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(input.DestinationDirectory, zipFileName)));
    }

    [Test]
    public void ZipFiles_DoesNotDeleteSourceFiles()
    {
        ExecuteCreateArchive();
        var sourceFiles = Directory.GetFiles(dirIn, "*.txt");
        Assert.AreEqual(2, sourceFiles.Length);
    }

    [Test]
    public void ZipFiles_DeletesSourceFiles()
    {
        input.RemoveZippedFiles = true;
        ExecuteCreateArchive();
        var sourceFiles = Directory.GetFiles(dirIn, "*.txt");
        Assert.AreEqual(0, sourceFiles.Length);
    }

    [Test]
    public void ZipFiles_FilePathsZipsFilesInList()
    {
        input.SourceType = SourceFilesType.FileList;
        input.SourceDirectory = string.Empty;
        var filePath = new List<string>
        {
            Directory.GetFiles(dirIn, "*.txt")[0],
        };
        input.FilePathsList = filePath;

        var result = ExecuteCreateArchive();
        Assert.AreEqual(1, result.FileCount);
    }

    [Test]
    public void ZipFiles_Fails_If_SourcePathDoesNotExist()
    {
        input.SourceDirectory = Path.Combine(dirIn, "foobar");
        var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("Source directory"));
        Assert.IsTrue(result.Message.Contains("does not exist."));
    }

    [Test]
    public void Zipfiles_DoesNotFail_IfSourceFilesAreNotFound()
    {
        input.FileMask = "foobar.txt";
        options.ThrowErrorIfNoFilesFound = false;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(0, result.FileCount);
        Assert.IsFalse(File.Exists(Path.Combine(input.DestinationDirectory, zipFileName)));
    }

    [Test]
    public void ZipFiles_Fails_IfNoSourceFilesFound()
    {
        input.FileMask = "foobar.txt";
        var result = Assert.Throws<FileNotFoundException>(() => ExecuteCreateArchive());
    }

    [Test]
    public void ZipFiles_Fails_If_DestinationPathDoesNotExist()
    {
        input.DestinationDirectory = Path.Combine(input.DestinationDirectory, "foobar");
        var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("Destination directory"));
        Assert.IsTrue(result.Message.Contains("does not exist."));
    }

    [Test]
    public void ZipFiles_NonRecursive_And_CreateDestinationDirectory()
    {
        options.CreateDestinationFolder = true;
        input.DestinationDirectory = Path.Combine(dirOut, "newDir");
        var result = ExecuteCreateArchive();
        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(2, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(input.DestinationDirectory, zipFileName)));
    }

    [Test]
    public void ZipFiles_Recursive()
    {
        input.IncludeSubFolders = true;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(4, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(input.DestinationDirectory, zipFileName)));
        var fileNamesWithSubDir = result.ArchivedFiles.Where(s => s.Contains(subDir)).Count();
        Assert.AreEqual(2, fileNamesWithSubDir);
    }

    [Test]
    public void ZipFiles_FlattenFolders()
    {
        input.IncludeSubFolders = true;
        input.FlattenFolders = true;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(4, result.FileCount);
        var subDirIsPresent = result.ArchivedFiles.Where(s => s.Contains(subDir)).Any();
        Assert.IsFalse(subDirIsPresent);
    }

    [Test]
    public void ZipFiles_FlattenFolders_Fails_IfDublicateFileNames_And_RenameFalse()
    {
        var dublicateFileName = "dublicate_file.txt";

        // Create files with dublicate names in separate folders.
        File.WriteAllText(Path.Combine(dirIn, dublicateFileName), "Seaman: Swallow, come!");
        File.WriteAllText(Path.Combine(subDirIn, dublicateFileName), "Seaman: Swallow, come!");

        input.IncludeSubFolders = true;
        input.FlattenFolders = true;
        var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("already exists in zip!"));
    }

    [Test]
    public void ZipFiles_Flattenfolders_RenamesDublicateFiles()
    {
        var dublicateFileName = "dublicate_file.txt";

        // Create files with dublicate names in separate folders.
        File.WriteAllText(Path.Combine(dirIn, dublicateFileName), "Seaman: Swallow, come!");
        File.WriteAllText(Path.Combine(subDirIn, dublicateFileName), "Seaman: Swallow, come!");
        var subDir2 = Path.Combine(subDirIn, "subdir2");
        Directory.CreateDirectory(subDir2);
        File.WriteAllText(Path.Combine(subDir2, dublicateFileName), "Seaman: Swallow, come!");

        input.IncludeSubFolders = true;
        input.FlattenFolders = true;
        input.RenameDuplicateFiles = true;

        var result = ExecuteCreateArchive();
        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(7, result.FileCount);
        var subDirCount = result.ArchivedFiles.Where(s => s.Contains(subDir)).Count();
        Assert.AreEqual(0, subDirCount);
        Assert.Contains("dublicate_file.txt", result.ArchivedFiles);
        Assert.Contains("dublicate_file_(1).txt", result.ArchivedFiles);
        Assert.Contains("dublicate_file_(2).txt", result.ArchivedFiles);
    }

    [Test]
    public void ZipFiles_FileList_FlattensFolders()
    {
        input.FlattenFolders = false;
        input.SourceDirectory = string.Empty;
        input.SourceType = SourceFilesType.FileList;
        var filePath = Directory.GetFiles(dirIn)[0];
        input.FilePathsList = new List<string> { filePath };
        var result = ExecuteCreateArchive();
        Assert.AreEqual(Path.GetFileName(filePath), result.ArchivedFiles[0]);
    }

    [Test]
    public void ZipFile_Exists_ThrowsError()
    {
        options.DestinationFileExistsAction = FileExistAction.Error;
        ExecuteCreateArchive();
        var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());
        Assert.AreEqual($"Destination file {Path.Combine(input.DestinationDirectory, input.DestinationFileName)} already exists.", result.Message);
    }

    [Test]
    public void ZipFile_Exists_OverwriteFile()
    {
        options.DestinationFileExistsAction = FileExistAction.Overwrite;
        ExecuteCreateArchive();
        ExecuteCreateArchive();
        Assert.AreEqual(1, Directory.GetFiles(input.DestinationDirectory, "*.zip").Length);
    }

    [Test]
    public void ZipFile_Exists_Rename()
    {
        options.DestinationFileExistsAction = FileExistAction.Rename;
        var result1 = ExecuteCreateArchive();
        var result2 = ExecuteCreateArchive();
        var result3 = ExecuteCreateArchive();
        var zipFiles = Directory.GetFiles(input.DestinationDirectory, "*.zip");
        Assert.AreEqual(3, zipFiles.Length);
        Assert.AreEqual("zip_test.zip", Path.GetFileName(result1.Path));
        Assert.AreEqual("zip_test_(1).zip", Path.GetFileName(result2.Path));
        Assert.AreEqual("zip_test_(2).zip", Path.GetFileName(result3.Path));
    }

    [Test]
    public void ZipFile_Exists_Appends_NewFiles()
    {
        var result = ExecuteCreateArchive();
        Assert.AreEqual(2, result.FileCount);
        options.DestinationFileExistsAction = FileExistAction.Append;
        input.RenameDuplicateFiles = true;
        var appendResult = ExecuteCreateArchive();
        Assert.AreEqual(4, appendResult.FileCount);
    }

    [Test]
    public void ZipFiles_WithPassword_NeedsPasword_For_Extraction()
    {
        input.Password = "password";
        var result = ExecuteCreateArchive();
        var zipPath = result.Path;
        var extractPath = Path.Combine(dirOut, "extracted");

        using (var zip = ZipFile.Read(zipPath))
        {
            Assert.Throws<BadPasswordException>(() => zip.ExtractAll(extractPath));
            zip.Password = input.Password;
            zip.ExtractAll(extractPath);
        }

        Assert.AreEqual(zipFileName, Path.GetFileName(result.Path));
        Assert.IsTrue(Directory.Exists(extractPath));
        Assert.AreEqual(Directory.GetFiles(extractPath, "*").Length, result.FileCount);
    }

    private Result ExecuteCreateArchive()
    {
        return Zip.CreateArchive(input, options, CancellationToken.None);
    }
}
