﻿using Frends.Zip.ExtractArchive.Definitions;
using Ionic.Zip;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Frends.Zip.ExtractArchive.Tests;

[TestFixture]
public class UnZipTests
{
    private readonly string[] fileNames = 
    {
        "logo1.png",
        "logo2.png",
        Path.Combine("folder", "logo1.png"),
        Path.Combine("folder", "logo2.png"),
        Path.Combine("folder", "folder", "folder", "logo1.png"),
        Path.Combine("folder", "folder", "folder", "logo2.png"),
        Path.Combine("folder", "folder", "folder", "folder", "logo1.png"),
    };
    // Used for testing the rename-option.
    private readonly string[] renamedFilenames = 
    {
        "logo1(0).png",
        "logo2(0).png",
        Path.Combine("folder", "logo1(0).png"),
        Path.Combine("folder", "logo2(0).png"),
        Path.Combine("folder", "folder", "folder", "logo1(0).png"),
        Path.Combine("folder", "folder", "folder", "logo2(0).png"),
        Path.Combine("folder", "folder", "folder", "folder", "logo1(0).png")
    };

    // Paths to TestIn and TestOut.
    private readonly string inputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../..", "TestData", "TestIn");
    private readonly string outputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../..", "TestData", "TestOut" + Path.DirectorySeparatorChar);
    List<string> outputFiles;
    UnzipInputProperties sp;
    UnzipOptions opt;

    [SetUp]
    public void Setup()
    {
        sp = new UnzipInputProperties();
        opt = new UnzipOptions();
        Directory.CreateDirectory(outputPath);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(outputPath, true);
    }

    [Test]
    public void SourceFileDoesNotExist()
    {
        // Throws System.IO.FileNotFoundException.
        sp.SourceFile = Path.Combine(inputPath, "doesnotexist.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        sp.DestinationDirectory = outputPath;
        Assert.That(() => Zip.ExtractArchive(sp, opt, new CancellationToken()), Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void DestinatioDirectoryNotFound()
    {
        // Throws directory not found exception, destination directory does not exist and create destination directory == false
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = false;
        sp.DestinationDirectory = Path.Combine(outputPath, "doesnot", "exist");
        Assert.That(() => Zip.ExtractArchive(sp, opt, new CancellationToken()), Throws.TypeOf<DirectoryNotFoundException>());
    }

    [Test]
    public void CreateDirectory()
    {
        // Create destination directory if it does not exist.
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = true;
        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        outputFiles = new List<string>();
        fileNames.ToList().ForEach(x => outputFiles.Add(Path.Combine(sp.DestinationDirectory, x)));
        var output = Zip.ExtractArchive(sp, opt, new CancellationToken());

        foreach (var s in outputFiles) Assert.True(File.Exists(s));
        Assert.AreEqual(output.ExtractedFiles.Count, 7);
    }

    [Test]
    public void ExtractWithPassword()
    {
        // Extract password protected archive.
        sp.SourceFile = Path.Combine(inputPath, "HiQLogosWithPassword.zip");
        sp.Password = "secret";

        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        Zip.ExtractArchive(sp, opt, new CancellationToken());
        Assert.True(File.Exists(Path.Combine(outputPath, "new_directory", "logo1.png")));
        Assert.True(File.Exists(Path.Combine(outputPath, "new_directory", "logo2.png")));
    }

    [Test]
    public void PasswordError()
    {
        // Should throw Ionic.Zip.BadPasswordException.
        sp.SourceFile = Path.Combine(inputPath, "HiQLogosWithPassword.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        opt.CreateDestinationDirectory = true;
        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        Assert.That(() => Zip.ExtractArchive(sp, opt, new CancellationToken()), Throws.TypeOf<BadPasswordException>());
    }

    [Test]
    public void ThrowErrorOnOverwrite()
    {
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");

        var opt2 = new UnzipOptions()
        {
            DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
            CreateDestinationDirectory = true
        };

        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        // Unzip files to TestOut, so that there are existing files.
        Zip.ExtractArchive(sp, opt2, new CancellationToken());

        Assert.That(() => Zip.ExtractArchive(sp, opt, new CancellationToken()), Throws.TypeOf<ZipException>());
    }

    [Test]
    public void OverwriteFiles()
    {
        sp.SourceFile = Path.Combine(inputPath, "testzip.zip");
        var opt = new UnzipOptions()
        {
            DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
            CreateDestinationDirectory = true
        };

        sp.DestinationDirectory = Path.Combine(sp.DestinationDirectory = Path.Combine(outputPath, "new_directory"));

        // Extract testzip.zip.
        var output = Zip.ExtractArchive(sp, opt, new CancellationToken());

        // Read first line from each file.
        var lines = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories).Select(x => File.ReadLines(x).First()).ToList();

        Assert.True(lines.Contains("First file") && lines.Contains("Second file") && lines.Contains("Third file"));

        sp.SourceFile = Path.Combine(inputPath, "testzip2.zip");
        output = Zip.ExtractArchive(sp,  opt, new CancellationToken());
        var lines2 = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories).Select(x => File.ReadLines(x).First()).ToList();
        Assert.False(lines2.Contains("First file") && lines2.Contains("Second file") && lines2.Contains("Third file"));
        Assert.True(lines2.Contains("Fourth file") && lines2.Contains("Fifth file") && lines2.Contains("Sixth file"));
    }

    [Test]
    public void RenameFiles()
    {
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");

        opt.DestinationFileExistsAction = UnzipFileExistAction.Rename;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = outputPath;

        // Extract files to TestOut, so that there are existing files.
        Zip.ExtractArchive(sp,  opt, new CancellationToken());
        var output = Zip.ExtractArchive(sp,  opt, new CancellationToken());

        // Create filenames to test against.
        outputFiles = new List<string>();
        renamedFilenames.ToList().ForEach(x => outputFiles.Add(Path.Combine(sp.DestinationDirectory, x)));

        foreach (var s in outputFiles) Assert.True(File.Exists(s));

        Assert.AreEqual(output.ExtractedFiles.Count, 7);
    }
}