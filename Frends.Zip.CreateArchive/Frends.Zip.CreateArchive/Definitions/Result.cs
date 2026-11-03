namespace Frends.Zip.CreateArchive.Definitions;

/// <summary>
/// Task result.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Full path to zip created.
    /// </summary>
    /// <example>c:\temp\zip_sample.zip</example>
    public string Path { get; set; }

    /// <summary>
    /// Number of files in created zip file.
    /// </summary>
    /// <example>1</example>
    public int FileCount { get; set; }

    /// <summary>
    /// List of files zipped.
    /// </summary>
    /// <example>TestFile.txt, TestFile2.txt</example>
    public List<string> ArchivedFiles { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
