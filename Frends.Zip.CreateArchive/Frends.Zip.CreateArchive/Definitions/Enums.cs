namespace Frends.Zip.CreateArchive.Definitions;

/// <summary>
/// File exist actions.
/// </summary>
public enum FileExistAction
{
    /// <summary>
    /// Throws an error.
    /// </summary>
    Error,

    /// <summary>
    /// Adds new files to the existing zip.
    /// </summary>
    Append,

    /// <summary>
    /// Overwrites the existing zip.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Renames the new zip file.
    /// </summary>
    Rename,
}

/// <summary>
/// Source file's type.
/// </summary>
public enum SourceFilesType
{
    /// <summary>
    /// Files are searched from a directory with a file mask.
    /// </summary>
    PathAndFileMask,

    /// <summary>
    /// Files are given as a list of full paths.
    /// </summary>
    FileList,
}

/// <summary>
/// Use ZIP64 options.
/// </summary>
public enum UseZip64Option
{
    /// <summary>
    /// Always use ZIP64 extensions.
    /// </summary>
    Always,

    /// <summary>
    /// Use ZIP64 extensions when necessary.
    /// </summary>
    AsNecessary,

    /// <summary>
    /// Never use ZIP64 extensions.
    /// </summary>
    Never,
}
