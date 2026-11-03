namespace Frends.Zip.CreateArchive;

/// <summary>
/// Helper extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Converts enum to requested enum type.
    /// </summary>
    /// <param name="source">Enum value to convert.</param>
    /// <typeparam name="TEnum">Target enum type.</typeparam>
    /// <returns>Value of the target enum type with the same name.</returns>
    public static TEnum ConvertEnum<TEnum>(this Enum source)
    {
        return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
    }

    /// <summary>
    /// Adds running index to the file name.
    /// </summary>
    /// <param name="fileName">Original file name.</param>
    /// <param name="index">Index to add.</param>
    /// <returns>File name with index, e.g. example_(1).txt.</returns>
    public static string RenameFile(this string fileName, int index)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}_({index}){Path.GetExtension(fileName)}";
    }

    /// <summary>
    /// Gets directory of the file relative to the base directory.
    /// </summary>
    /// <param name="fullPath">Full path of the file.</param>
    /// <param name="baseDirectory">Base directory.</param>
    /// <returns>Relative directory path.</returns>
    public static string GetRelativePath(this string fullPath, string baseDirectory)
    {
        baseDirectory = baseDirectory.EndsWith(@"\") ? baseDirectory : $"{baseDirectory}\\";
        return Path.GetDirectoryName(fullPath).Replace(Path.GetDirectoryName(baseDirectory), string.Empty);
    }
}
