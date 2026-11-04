using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Zip.ExtractArchive.Definitions;

/// <summary>
/// Source properties.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path to the source file.
    /// </summary>
    /// <example>c:\example\file.zip</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFile { get; set; }

    /// <summary>
    /// Password for the ZIP file.
    /// </summary>
    /// <example>foobar123</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// Destination directory.
    /// </summary>
    /// <example>C:\example</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string DestinationDirectory { get; set; }
}
