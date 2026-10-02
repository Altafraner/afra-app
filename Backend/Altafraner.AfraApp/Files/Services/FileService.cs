using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Files.Services;

/// <summary>
///     Contains methods for handling file system operations
/// </summary>
public class FileService
{
    private readonly IOptions<FilesConfiguration> _config;

    ///
    public FileService(IOptions<FilesConfiguration> config)
    {
        _config = config;
    }

    /// <summary>
    ///     Opens a file from in filesystem
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    /// <param name="mode">Specifies how to handle nonexisting files</param>
    /// <param name="access">Specifies what level of access is needed</param>
    /// <param name="share">Can be used to place a lock on a file</param>
    /// <returns>Null, if a file is opened for read and not found</returns>
    /// <exception cref="InsecurePathException">The provided path contains possibly insecure elements</exception>
    public FileStream? OpenFile(string scope,
        IEnumerable<string> path,
        FileMode mode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        FileShare share = FileShare.Read)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var finalPath = GetPathFromComponents(scope, pathArray);

        try
        {
            return File.Open(finalPath, mode, access, share);
        }
        catch (DirectoryNotFoundException)
        {
            if (!access.HasFlag(FileAccess.Write)) return null;

            var di = new DirectoryInfo(finalPath);
            di.Parent!.Create();
            return File.Open(finalPath, mode, access, share);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Checks, whether a file exists
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    /// <returns>True, if the file exists; otherwise, false</returns>
    public bool CheckExists(string scope, IEnumerable<string> path)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var finalPath = GetPathFromComponents(scope, pathArray);

        return File.Exists(finalPath);
    }

    /// <summary>
    ///     Creates or overwrites a file
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    /// <param name="content">The content to be written</param>
    /// <remarks>The caller is responsible for making sure no malicious files are uploaded</remarks>
    public async Task WriteToFileAsync(string scope, IEnumerable<string> path, ReadOnlyMemory<byte> content)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var finalPath = GetPathFromComponents(scope, pathArray);
        var di = new DirectoryInfo(finalPath).Parent;
        if (di is not null && !di.Exists) di.Create();
        await File.WriteAllBytesAsync(finalPath, content);
    }

    /// <summary>
    ///     Lists the contents of a directory
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    /// <param name="searchOption">Defines search parameters</param>
    public string[] GetDirectoryContents(string scope,
        IEnumerable<string> path,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var finalPath = GetPathFromComponents(scope, pathArray);
        return Directory.GetFiles(finalPath, "*", searchOption);
    }

    /// <summary>
    ///     Tries to delete a path from the fs
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    public bool TryDeletePath(string scope, IEnumerable<string> path)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var fsPath = GetPathFromComponents(scope, pathArray);

        if (Directory.Exists(fsPath))
        {
            Directory.Delete(fsPath, true);
            return true;
        }

        if (File.Exists(fsPath))
        {
            File.Delete(fsPath);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Deletes a file from the fs
    /// </summary>
    /// <param name="scope">The module requesting the file. Each module has it's isolated filespace</param>
    /// <param name="path">
    ///     Should contain the relative file path where each part is a directory, except the last one, that
    ///     should be the filename
    /// </param>
    public void DeleteFile(string scope, IEnumerable<string> path)
    {
        var pathArray = path as string[] ?? path.ToArray();
        var finalPath = GetPathFromComponents(scope, pathArray);
        File.Delete(finalPath);
    }


    private string GetPathFromComponents(string scope, string[] pathComponents)
    {
        if (pathComponents.Any(pathElement => !ValidatePathElement(pathElement)))
            throw new InsecurePathException("Insecure path element detected.");

        var lastPath = Path.Combine(_config.Value.RootPath, scope);
        var lastDirectory = new DirectoryInfo(lastPath);

        foreach (var pathComponent in pathComponents)
        {
            if (!ValidatePathElement(pathComponent)) throw new InsecurePathException("Insecure path element detected.");
            var currentPath = Path.Combine(lastPath, pathComponent);
            var currentDirectory = new DirectoryInfo(currentPath);
            if (currentDirectory.FullName != lastDirectory.FullName &&
                currentDirectory.Parent?.FullName != lastDirectory.FullName)
                throw new InsecurePathException("Insecure path element detected.");
            lastPath = currentPath;
            lastDirectory = currentDirectory;
        }

        return lastPath;

        static bool ValidatePathElement(string pathElement)
        {
            return pathElement.All(ValidateChar);
        }

        static bool ValidateChar(char c)
        {
            switch (c)
            {
                case >= 'a' and <= 'z':
                case >= 'A' and <= 'Z':
                case >= '0' and <= '9':
                case '_':
                case ',':
                case ' ':
                case '-':
                    return true;
                default: return false;
            }
        }
    }
}

/// <summary>
///     Thrown, when the supplied path could possibly lead to security problems (like path traversal)
/// </summary>
public class InsecurePathException : Exception
{
    ///
    public InsecurePathException(string message) : base(message)
    {
    }
}
