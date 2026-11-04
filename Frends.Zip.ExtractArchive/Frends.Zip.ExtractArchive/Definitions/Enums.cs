namespace Frends.Zip.ExtractArchive.Definitions;

/// <summary>
/// Unzip file exist actions.
/// </summary>
public enum UnzipFileExistAction
{
    /// <summary>
    /// Throw an error if the destination file already exists.
    /// </summary>
    Error,

    /// <summary>
    /// Overwrite the existing destination file.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Extract the file with a new name, leaving the existing file untouched.
    /// </summary>
    Rename,
}
