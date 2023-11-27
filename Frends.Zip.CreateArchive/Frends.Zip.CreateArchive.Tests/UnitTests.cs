using System;
using System.IO;
using NUnit.Framework;
using System.Threading;
using System.Linq;
using Ionic.Zip;
using System.Collections.Generic;
using Frends.Zip.CreateArchive.Definitions;

namespace Frends.Zip.CreateArchive.Tests;

[TestFixture]
public class ZipTests
{
    private static readonly string _basePath = Path.Combine(Path.GetTempPath(), "frends.zip.createarchive.tests");

    private readonly string _dirIn = Path.Combine(_basePath, $"In{Path.DirectorySeparatorChar}");
    private readonly string _subDir = "Subdir";
    private string _subDirIn;
    private readonly string _dirOut = Path.Combine(_basePath, $"Out{Path.DirectorySeparatorChar}");
    private readonly string _zipFileName = "zip_test.zip";

    private SourceProperties _source;
    private DestinationProperties _destination;
    private Options _options;

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_basePath, true);
    }

    [SetUp]
    public void SetupTests()
    {
        _subDirIn = Path.Combine(_dirIn, _subDir);
        _source = new SourceProperties { Directory = _dirIn, FileMask = "*.txt", IncludeSubFolders = false, FlattenFolders = false };
        _destination = new DestinationProperties
        {
            Directory = _dirOut,
            FileName = _zipFileName,
            Password = "",
            RenameDuplicateFiles = false
        };
        _options = new Options { ThrowErrorIfNoFilesFound = true, CreateDestinationFolder = false, DestinationFileExistsAction = FileExistAction.Error };

        // Create source directory and files.
        Directory.CreateDirectory(_dirIn);
        File.WriteAllText(Path.Combine(_dirIn, "test_1_file.txt"), "foobaar foobar");
        File.WriteAllText(Path.Combine(_dirIn, "test_2_file.txt"), "foobaar foobar");
        // Create subdirectory for recursive test.
        Directory.CreateDirectory(_subDirIn);
        File.WriteAllText(Path.Combine(_subDirIn, "sub_test_1_file.txt"), "foobaar foobar");
        File.WriteAllText(Path.Combine(_subDirIn, "sub_test_2_file.txt"), "foobaar foobar");

        Directory.CreateDirectory(_dirOut);
    }

    private Result ExecuteCreateArchive()
    {
        return Zip.CreateArchive(_source, _destination, _options, CancellationToken.None);
    }

    [Test]
    public void ZipFiles_NonRecursive()
    {
        var result = ExecuteCreateArchive();

        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(2, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _zipFileName)));
    }

    [Test]
    public void ZipFiles_DoesNotDeleteSourceFiles()
    {
        ExecuteCreateArchive();
        var sourceFiles = Directory.GetFiles(_dirIn, "*.txt");
        Assert.AreEqual(2, sourceFiles.Length);
    }

    [Test]
    public void ZipFiles_DeletesSourceFiles()
    {
        _source.RemoveZippedFiles = true;
        ExecuteCreateArchive();
        var sourceFiles = Directory.GetFiles(_dirIn, "*.txt");
        Assert.AreEqual(0, sourceFiles.Length);
    }

    [Test]
    public void ZipFiles_FilePathsZipsFilesInList()
    {
        _source.SourceType = SourceFilesType.FileList;
        _source.Directory = "";
        var filePath = new List<string>
        {
            Directory.GetFiles(_dirIn, "*.txt")[0]
        };
        _source.FilePathsList = filePath;

        var result = ExecuteCreateArchive();
        Assert.AreEqual(1, result.FileCount);
    }

    [Test]
    public void ZipFiles_Fails_If_SourcePathDoesNotExist()
    {
        _source.Directory = Path.Combine(_dirIn, "foobar");
        var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("Source directory"));
        Assert.IsTrue(result.Message.Contains("does not exist."));
    }

    [Test]
    public void Zipfiles_DoesNotFail_IfSourceFilesAreNotFound()
    {
        _source.FileMask = "foobar.txt";
        _options.ThrowErrorIfNoFilesFound = false;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(0, result.FileCount);
        Assert.IsFalse(File.Exists(Path.Combine(_destination.Directory, _zipFileName)));
    }

    [Test]
    public void ZipFiles_Fails_IfNoSourceFilesFound()
    {
        _source.FileMask = "foobar.txt";
        var result = Assert.Throws<FileNotFoundException>(() => ExecuteCreateArchive());
    }

    [Test]
    public void ZipFiles_Fails_If_DestinationPathDoesNotExist()
    {
        _destination.Directory = Path.Combine(_destination.Directory, "foobar");
        var result = Assert.Throws<DirectoryNotFoundException>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("Destination directory"));
        Assert.IsTrue(result.Message.Contains("does not exist."));
    }

    [Test]
    public void ZipFiles_NonRecursive_And_CreateDestinationDirectory()
    {
        _options.CreateDestinationFolder = true;
        _destination.Directory = Path.Combine(_dirOut, "newDir");
        var result = ExecuteCreateArchive();
        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(2, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _zipFileName)));
    }


    [Test]
    public void ZipFiles_Recursive()
    {
        _source.IncludeSubFolders = true;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(4, result.FileCount);
        Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _zipFileName)));
        var fileNamesWithSubDir = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Count();
        Assert.AreEqual(2, fileNamesWithSubDir);
    }

    [Test]
    public void ZipFiles_FlattenFolders()
    {
        _source.IncludeSubFolders = true;
        _source.FlattenFolders = true;
        var result = ExecuteCreateArchive();
        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(4, result.FileCount);
        var subDirIsPresent = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Any();
        Assert.IsFalse(subDirIsPresent);
    }

    [Test]
    public void ZipFiles_FlattenFolders_Fails_IfDublicateFileNames_And_RenameFalse()
    {
        var dublicateFileName = "dublicate_file.txt";

        // Create files with dublicate names in separate folders.
        File.WriteAllText(Path.Combine(_dirIn, dublicateFileName), "Seaman: Swallow, come!");
        File.WriteAllText(Path.Combine(_subDirIn, dublicateFileName), "Seaman: Swallow, come!");

        _source.IncludeSubFolders = true;
        _source.FlattenFolders = true;
        var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Contains("already exists in zip!"));
    }

    [Test]
    public void ZipFiles_Flattenfolders_RenamesDublicateFiles()
    {
        var dublicateFileName = "dublicate_file.txt";

        // Create files with dublicate names in separate folders.
        File.WriteAllText(Path.Combine(_dirIn, dublicateFileName), "Seaman: Swallow, come!");
        File.WriteAllText(Path.Combine(_subDirIn, dublicateFileName), "Seaman: Swallow, come!");
        var subDir2 = Path.Combine(_subDirIn, "subdir2");
        Directory.CreateDirectory(subDir2);
        File.WriteAllText(Path.Combine(subDir2, dublicateFileName), "Seaman: Swallow, come!");

        _source.IncludeSubFolders = true;
        _source.FlattenFolders = true;
        _destination.RenameDuplicateFiles = true;

        var result = ExecuteCreateArchive();
        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.AreEqual(7, result.FileCount);
        var subDirCount = result.ArchivedFiles.Where(s => s.Contains(_subDir)).Count();
        Assert.AreEqual(0, subDirCount);
        Assert.Contains("dublicate_file.txt", result.ArchivedFiles);
        Assert.Contains("dublicate_file_(1).txt", result.ArchivedFiles);
        Assert.Contains("dublicate_file_(2).txt", result.ArchivedFiles);
    }

    [Test]
    public void ZipFiles_FileList_FlattensFolders()
    {
        _source.FlattenFolders = false;
        _source.Directory = "";
        _source.SourceType = SourceFilesType.FileList;
        var filePath = Directory.GetFiles(_dirIn)[0];
        _source.FilePathsList = new List<string> { filePath };
        var result = ExecuteCreateArchive();
        Assert.AreEqual(Path.GetFileName(filePath), result.ArchivedFiles[0]);
    }

    [Test]
    public void ZipFile_Exists_ThrowsError()
    {
        _options.DestinationFileExistsAction = FileExistAction.Error;
        ExecuteCreateArchive();
        var result = Assert.Throws<Exception>(() => ExecuteCreateArchive());
        Assert.IsTrue(result.Message.Equals($"Destination file {Path.Combine(_destination.Directory, _destination.FileName)} already exists."));
    }

    [Test]
    public void ZipFile_Exists_OverwriteFile()
    {
        _options.DestinationFileExistsAction = FileExistAction.Overwrite;
        ExecuteCreateArchive();
        ExecuteCreateArchive();
        Assert.AreEqual(1, Directory.GetFiles(_destination.Directory, "*.zip").Length);
    }

    [Test]
    public void ZipFile_Exists_Rename()
    {
        _options.DestinationFileExistsAction = FileExistAction.Rename;
        var result1 = ExecuteCreateArchive();
        var result2 = ExecuteCreateArchive();
        var result3 = ExecuteCreateArchive();
        var zipFiles = Directory.GetFiles(_destination.Directory, "*.zip");
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
        _options.DestinationFileExistsAction = FileExistAction.Append;
        _destination.RenameDuplicateFiles = true;
        var appendResult = ExecuteCreateArchive();
        Assert.AreEqual(4, appendResult.FileCount);
    }

    [Test]
    public void ZipFiles_WithPassword_NeedsPasword_For_Extraction()
    {
        _destination.Password = "password";
        var result = ExecuteCreateArchive();
        var zipFileName = result.Path;
        var extractPath = Path.Combine(_dirOut, "extracted");

        using (var zip = ZipFile.Read(zipFileName))
        {
            Assert.Throws<BadPasswordException>(() => zip.ExtractAll(extractPath));
            zip.Password = _destination.Password;
            zip.ExtractAll(extractPath);
        }

        Assert.AreEqual(_zipFileName, Path.GetFileName(result.Path));
        Assert.IsTrue(Directory.Exists(extractPath));
        Assert.AreEqual(result.FileCount, Directory.GetFiles(extractPath, "*").Length);
    }
}