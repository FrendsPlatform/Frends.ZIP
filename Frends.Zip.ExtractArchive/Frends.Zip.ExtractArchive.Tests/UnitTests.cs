using Frends.Zip.ExtractArchive.Definitions;
using Ionic.Zip;
using NUnit.Framework;

namespace Frends.Zip.ExtractArchive.Tests;

[TestFixture]
public class UnitTests
{
    private readonly string[] fileNames =
    {
        "logo1.png", "logo2.png", Path.Combine("folder", "logo1.png"), Path.Combine("folder", "logo2.png"),
        Path.Combine("folder", "folder", "folder", "logo1.png"),
        Path.Combine("folder", "folder", "folder", "logo2.png"),
        Path.Combine("folder", "folder", "folder", "folder", "logo1.png"),
    };

    // Paths to TestIn and TestOut.
    private readonly string inputPath =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "../../..", "TestData", "TestIn");

    private readonly string outputPath = Path.Combine(
        TestContext.CurrentContext.TestDirectory,
        "../../..",
        "TestData",
        "TestOut" + Path.DirectorySeparatorChar);

    private List<string> outputFiles;
    private Input sp;
    private Options opt;

    [SetUp]
    public void Setup()
    {
        sp = new Input();
        opt = new Options();
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
        sp.SourceFile = Path.Combine(inputPath, "DoesNotExist.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        sp.DestinationDirectory = outputPath;
        Assert.Throws<FileNotFoundException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
    }

    [Test]
    public void DestinationDirectoryNotFound()
    {
        // Throws directory not found exception, destination directory does not exist and create destination directory == false
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = false;
        sp.DestinationDirectory = Path.Combine(outputPath, "DoesNot", "exist");
        Assert.Throws<DirectoryNotFoundException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
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
        var output = Zip.ExtractArchive(sp, opt, CancellationToken.None);

        foreach (var s in outputFiles) Assert.That(File.Exists(s), Is.True);
        Assert.That(output.ExtractedFiles.Count, Is.EqualTo(7));
    }

    [Test]
    public void ExtractWithPassword()
    {
        // Extract password-protected archive.
        sp.SourceFile = Path.Combine(inputPath, "HiQLogosWithPassword.zip");
        sp.Password = "secret";

        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        Zip.ExtractArchive(sp, opt, CancellationToken.None);
        Assert.That(File.Exists(Path.Combine(outputPath, "new_directory", "logo1.png")), Is.True);
        Assert.That(File.Exists(Path.Combine(outputPath, "new_directory", "logo2.png")), Is.True);
    }

    [Test]
    public void PasswordError()
    {
        // Should throw Ionic.Zip.BadPasswordException.
        sp.SourceFile = Path.Combine(inputPath, "HiQLogosWithPassword.zip");
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;
        opt.CreateDestinationDirectory = true;
        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        Assert.Throws<BadPasswordException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
    }

    [Test]
    public void ThrowErrorOnOverwrite()
    {
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");

        var opt2 = new Options
        {
            DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
            CreateDestinationDirectory = true,
        };

        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = Path.Combine(outputPath, "new_directory");

        // Unzip files to TestOut, so that there are existing files.
        Zip.ExtractArchive(sp, opt2, CancellationToken.None);

        Assert.Throws<IOException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
    }

    [Test]
    public void OverwriteFiles()
    {
        sp.SourceFile = Path.Combine(inputPath, "testZip.zip");
        opt = new Options
        {
            DestinationFileExistsAction = UnzipFileExistAction.Overwrite,
            CreateDestinationDirectory = true,
        };

        sp.DestinationDirectory = Path.Combine(sp.DestinationDirectory = Path.Combine(outputPath, "new_directory"));

        // Extract testZip.zip.
        Zip.ExtractArchive(sp, opt, CancellationToken.None);

        // Read first line from each file.
        var lines = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories)
            .Select(x => File.ReadLines(x).First()).ToList();

        Assert.That(lines.Contains("First file") && lines.Contains("Second file") && lines.Contains("Third file"), Is.True);

        sp.SourceFile = Path.Combine(inputPath, "testZip2.zip");
        Zip.ExtractArchive(sp, opt, CancellationToken.None);
        var lines2 = Directory.EnumerateFiles(sp.DestinationDirectory, "*", SearchOption.AllDirectories)
            .Select(x => File.ReadLines(x).First()).ToList();
        Assert.That(lines2.Contains("First file") && lines2.Contains("Second file") && lines2.Contains("Third file"), Is.False);
        Assert.That(lines2.Contains("Fourth file") && lines2.Contains("Fifth file") && lines2.Contains("Sixth file"), Is.True);
    }

    [Test]
    public void RenameFiles()
    {
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos.zip");

        opt.DestinationFileExistsAction = UnzipFileExistAction.Rename;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = outputPath;

        // Extract files to TestOut, so that there are existing files.
        Zip.ExtractArchive(sp, opt, CancellationToken.None);

        var output = Zip.ExtractArchive(sp, opt, CancellationToken.None);

        // Create filenames to test against.
        outputFiles = new List<string>();
        output.ExtractedFiles.ForEach(x => outputFiles.Add(x));

        foreach (var s in outputFiles)
            Assert.That(File.Exists(s), Is.True);

        Assert.That(output.ExtractedFiles.Count, Is.EqualTo(7));
    }

    [TestCase("../traversal_forward_marker.txt", "traversal_forward_marker.txt")]
    [TestCase(@"..\traversal_backslash_marker.txt", "traversal_backslash_marker.txt")]
    public void RejectsTraversalEntry(string entryName, string escapedFileName)
    {
        var destinationDirectory = Path.Combine(outputPath, "traversal_destination");
        var escapedFilePath = Path.Combine(outputPath, escapedFileName);
        sp.SourceFile = CreateArchiveWithEntry(entryName, "traversal.zip");
        sp.DestinationDirectory = destinationDirectory;
        opt.CreateDestinationDirectory = true;
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;

        Assert.Throws<IOException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
        Assert.That(File.Exists(escapedFilePath), Is.False);
    }

    [Test]
    public void RejectsRootedEntry()
    {
        var destinationDirectory = Path.Combine(outputPath, "rooted_destination");
        var escapedFilePath = Path.Combine(outputPath, $"rooted_marker_{Guid.NewGuid():N}.txt");
        var entryName = escapedFilePath.Replace(Path.DirectorySeparatorChar, '/');
        sp.SourceFile = CreateArchiveWithEntry(entryName, "rooted.zip");
        sp.DestinationDirectory = destinationDirectory;
        opt.CreateDestinationDirectory = true;
        opt.DestinationFileExistsAction = UnzipFileExistAction.Overwrite;

        Assert.Throws<IOException>(() => Zip.ExtractArchive(sp, opt, CancellationToken.None));
        Assert.That(File.Exists(escapedFilePath), Is.False);
    }

    [Test]
    public void DeleteSourceFile()
    {
        File.Copy(Path.Combine(inputPath, "HiQLogos.zip"), Path.Combine(inputPath, "HiQLogos(1).zip"));
        sp.SourceFile = Path.Combine(inputPath, "HiQLogos(1).zip");
        opt.DeleteZipFileAfterExtract = true;

        opt.DestinationFileExistsAction = UnzipFileExistAction.Error;
        opt.CreateDestinationDirectory = true;

        sp.DestinationDirectory = outputPath;

        Zip.ExtractArchive(sp, opt, CancellationToken.None);

        Assert.That(File.Exists(sp.SourceFile), Is.False);
    }

    private string CreateArchiveWithEntry(string entryName, string archiveName)
    {
        var archivePath = Path.Combine(outputPath, archiveName);
        using var archiveStream = File.Create(archivePath);
        using var zip = new System.IO.Compression.ZipArchive(
            archiveStream,
            System.IO.Compression.ZipArchiveMode.Create);
        using var writer = new StreamWriter(zip.CreateEntry(entryName).Open());
        writer.Write("harmless test content");
        return archivePath;
    }
}
