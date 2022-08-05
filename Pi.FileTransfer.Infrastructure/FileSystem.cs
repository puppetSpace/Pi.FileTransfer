using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Interfaces;
using System.Buffers;
using System.Text.Json;

namespace Pi.FileTransfer.Infrastructure;
public class FileSystem : IFileSystem
{
    private readonly IOptions<AppSettings> _options;
    private readonly ILogger<FileSystem> _logger;
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public FileSystem(IOptions<AppSettings> options, ILogger<FileSystem> logger)
    {
        _options = options;
        _logger = logger;
    }

    public IEnumerable<(string file, DateTime lastModified)> GetFiles(Core.Entities.Folder folder)
    {
        if (!Directory.Exists(folder.FullName))
            throw new DirectoryNotFoundException($"'{folder.FullName}' is not a directory or does not exist");
        var files = Directory.GetFiles(folder.FullName, "*", SearchOption.AllDirectories)
                    .Where(x => !x.Contains(Constants.RootDirectoryName))
                    .Select(x => new FileInfo(x))
                    .Select(x => (x.FullName, x.LastWriteTimeUtc))
                    .ToList();
        
        _logger.GetFilesForFolder(files.Count,folder.FullName);

        return files;
    }

    public IEnumerable<string> GetFiles(string path,string pattern, bool recursive = false)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"'{path}' is not a directory or does not exist");

        var files = Directory.GetFiles(path, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        _logger.GetFilesForFolder(files.Length, path);

        return files;
    }

    public IEnumerable<string> GetFoldersFromBasePath()
    {
        var directories = Directory.GetDirectories(_options.Value.BasePath);
        _logger.GetFoldersFromBasePath(directories.Length, _options.Value.BasePath);
        return directories;
    }

    public FileStream GetReadFileStream(string file, int bufferSize = 2048)
    {
        _logger.OpeningReadOnlyStream(file, bufferSize);
        return new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
    }

    public FileStream GetWriteFileStream(string file, int bufferSize = 2048)
    {
        _logger.OpeningWriteOnlyStream(file, bufferSize);
        return new(file, FileMode.Create, FileAccess.Write, FileShare.Write, bufferSize, true);
    }

    public void CreateDirectory(string path)
    {
        _logger.CreateFolder(path);
        Directory.CreateDirectory(path);
    }
    public async Task WritetoFile<TE>(string transferFile, TE value)
    {

        await _semaphore.WaitAsync();
        try
        {
            _logger.WriteToFile(transferFile, typeof(TE).FullName);
            await File.WriteAllTextAsync(transferFile, JsonSerializer.Serialize(value));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<TE> GetContentOfFile<TE>(string file)
    {
        _logger.ReadFromFile(file,typeof(TE).FullName);
        using var fs = File.Open(file,FileMode.Open,FileAccess.Read,FileShare.Read);
        return (await JsonSerializer.DeserializeAsync<TE>(fs))!;
    }

    public async Task<byte[]> GetRawContentOfFile(string file)
    {
        _logger.ReadFromFile(file, typeof(byte[]).FullName);
        using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var sr = new MemoryStream();
        await fs.CopyToAsync(sr);
        return sr.ToArray();
    }

    public void DeleteFile(string file)
    {
        _logger.DeleteFile(file);
        if (File.Exists(file))
            File.Delete(file);
    }

    public void CopyFile(string source,string destination)
    {
        _logger.CopyFile(source, destination);
        File.Copy(source,destination, true);
    }

    public void MoveFile(string source, string destination)
    {
        _logger.MoveFile(source, destination);
        File.Move(source, destination,true);
    }

    public void DeleteDirectory(string incomingDataFolder)
    {
        _logger.DeleteFolder(incomingDataFolder);
        if(Directory.Exists(incomingDataFolder))
            Directory.Delete(incomingDataFolder,true);
    }
}
