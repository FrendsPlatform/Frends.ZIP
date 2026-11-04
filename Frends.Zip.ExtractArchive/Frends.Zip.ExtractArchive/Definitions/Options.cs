using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Zip.ExtractArchive.Definitions;

/// <summary>
/// Options.
/// </summary>
public class Options
{
    /// <summary>
    /// Action to be taken when destination file/files exist.
    /// </summary>
    /// <example>Error</example>
    [DefaultValue(UnzipFileExistAction.Error)]
    public UnzipFileExistAction DestinationFileExistsAction { get; set; }

    /// <summary>
    /// Create destination directory if it does not exist.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    [DisplayName(@"Create destination directory")]
    public bool CreateDestinationDirectory { get; set; }

    /// <summary>
    /// If set to true, the source ZIP file will be deleted after successful extraction.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    [DisplayName("Delete ZIP file after extraction")]
    public bool DeleteZipFileAfterExtract { get; set; }

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to extract the archive</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
