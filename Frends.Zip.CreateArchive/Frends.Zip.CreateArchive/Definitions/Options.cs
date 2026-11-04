using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Zip.CreateArchive.Definitions;

/// <summary>
/// Options.
/// </summary>
public class Options
{
    /// <summary>
    /// Always: Always use ZIP64 extensions when writing zip archives, even when unnecessary.
    /// AsNecessary: Use ZIP64 extensions when writing zip archives, as necessary (when single entry or entries in total exceeds 0xFFFFFFFF in size, or when there are more than 65535 entries).
    /// Never: Do not use ZIP64 extensions when writing zip archives.
    /// </summary>
    /// <example>AsNecessary</example>
    [DefaultValue(UseZip64Option.AsNecessary)]
    public UseZip64Option UseZip64 { get; set; }

    /// <summary>
    /// Encoding for file and directory names.
    /// </summary>
    /// <example>Encoding.UTF8</example>
    public FileEncoding Encoding { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(Encoding), "", FileEncoding.UTF8)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example>utf-8</example>
    [UIHint(nameof(Encoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }

    /// <summary>
    /// Throw error if no source files are found. Otherwise returns object with FileCount: 0.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorIfNoFilesFound { get; set; }

    /// <summary>
    /// Choose action if destination zip file already exists.
    /// Error: throws error.
    /// Overwrite: Overwrites existing zip file with new one.
    /// Rename: Renames new zip file (example.zip --&gt; example_(1).zip).
    /// Append: Adds new files to zip, if file already exists in zip, it is renamed.
    /// </summary>
    /// <example>Error</example>
    [DefaultValue(FileExistAction.Error)]
    public FileExistAction DestinationFileExistsAction { get; set; }

    /// <summary>
    /// Create destination folder if it does not exist.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool CreateDestinationFolder { get; set; }

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to create the archive</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
