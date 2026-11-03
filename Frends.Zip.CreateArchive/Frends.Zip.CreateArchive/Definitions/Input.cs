using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Zip.CreateArchive.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Source files input type.
    /// </summary>
    /// <example>PathAndFileMask</example>
    [DefaultValue(SourceFilesType.PathAndFileMask)]
    public SourceFilesType SourceType { get; set; }

    /// <summary>
    /// Source directory.
    /// </summary>
    /// <example>C:\example\folder\</example>
    [UIHint(nameof(SourceType), "", SourceFilesType.PathAndFileMask)]
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceDirectory { get; set; }

    /// <summary>
    /// The search string to match against the names of files.
    /// This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.
    /// The default pattern is "*", which returns all files.
    /// </summary>
    /// <example>*</example>
    [DefaultValue("*")]
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(SourceType), "", SourceFilesType.PathAndFileMask)]
    public string FileMask { get; set; }

    /// <summary>
    /// Indicates if subfolders and files should also be zipped.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    [UIHint(nameof(SourceType), "", SourceFilesType.PathAndFileMask)]
    public bool IncludeSubFolders { get; set; }

    /// <summary>
    /// Choose if source folder structure should be flatten when zipped.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    [UIHint(nameof(SourceType), "", SourceFilesType.PathAndFileMask)]
    public bool FlattenFolders { get; set; }

    /// <summary>
    /// List&lt;string&gt; of full file paths to include in zip.
    /// </summary>
    /// <example>new List&lt;string&gt; { Directory.GetFiles(_dir, "*.txt")[0] };</example>
    [DisplayFormat(DataFormatString = "Expression")]
    [UIHint(nameof(SourceType), "", SourceFilesType.FileList)]
    public List<string> FilePathsList { get; set; }

    /// <summary>
    /// If true, files added to the zip are removed from source directory.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool RemoveZippedFiles { get; set; }

    /// <summary>
    /// Destination directory.
    /// </summary>
    /// <example>c:\temp</example>
    [DisplayFormat(DataFormatString = "Text")]
    [Required]
    public string DestinationDirectory { get; set; }

    /// <summary>
    /// Filename of the zip to create.
    /// </summary>
    /// <example>sample.zip</example>
    [DisplayFormat(DataFormatString = "Text")]
    [Required]
    public string DestinationFileName { get; set; }

    /// <summary>
    /// Add password protection to zip.
    /// </summary>
    /// <example>foobar123</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// True: If source files contains duplicate names, they are renamed (example.txt --&gt; example_(1).txt).
    /// False: Throws error if duplicate file names are found.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool RenameDuplicateFiles { get; set; }
}
