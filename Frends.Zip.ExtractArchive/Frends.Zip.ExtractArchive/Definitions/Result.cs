namespace Frends.Zip.ExtractArchive.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// A List-object of extracted files.
    /// </summary>
    /// <example>["C:\\temp\\sample.txt", "C:\\temp\\sample2.txt"]</example>
    public List<string> ExtractedFiles { get; set; } = [];

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
