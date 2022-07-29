using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Interfaces;
using System.Buffers;
using System.Text.Json;

namespace Pi.FileTransfer.Infrastructure;
public class FileSystem : IFileSystem
{
    private readonly IOptions<AppSettings> _options;
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public FileSystem(IOptions<AppSettings> options)
    {
        _options = options;
    }

    public IEnumerable<(string file, DateTime lastModified)> GetFiles(Core.Entities.Folder folder)
    {
        if (!Directory.Exists(folder.FullName))
            throw new DirectoryNotFoundException($"'{folder.FullName}' is not a directory or does not exist");

        return Directory.GetFiles(folder.FullName, "*", SearchOption.AllDirectories)
                    .Where(x => !x.Contains(Constants.RootDirectoryName))
                    .Select(x => new FileInfo(x))
                    .Select(x => (x.FullName, x.LastWriteTimeUtc))
                    .ToList();
    }

    public IEnumerable<string> GetFiles(string path,string pattern, bool recursive = false)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"'{path}' is not a directory or does not exist");

        return Directory.GetFiles(path, pattern,recursive ? SearchOption.AllDirectories: SearchOption.TopDirectoryOnly);
    }

    public IEnumerable<string> GetFoldersFromBasePath()
    {
        return Directory.GetDirectories(_options.Value.BasePath);
    }

    public FileStream GetReadFileStream(string file,int bufferSize = 2048 ) 
        => new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize , true);

    public FileStream GetWriteFileStream(string file, int bufferSize = 2048)
     => new(file, FileMode.Create, FileAccess.Write, FileShare.Write, bufferSize, true);

    public void CreateDirectory(string path)=> Directory.CreateDirectory(path);
    public async Task WritetoFile<TE>(string transferFile, TE value)
    {
        await _semaphore.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(transferFile, JsonSerializer.Serialize(value));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<TE> GetContentOfFile<TE>(string file)
    {
        using var fs = File.Open(file,FileMode.Open,FileAccess.Read,FileShare.Read);
        return (await JsonSerializer.DeserializeAsync<TE>(fs))!;
    }

    public async Task<byte[]> GetRawContentOfFile(string file)
    {
        using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var sr = new MemoryStream();
        await fs.CopyToAsync(sr);
        return sr.ToArray();
    }

    public void DeleteFile(string file)
    {
        if (File.Exists(file))
            File.Delete(file);
    }

    public void MoveFile(string source, string destination)
    {
        File.Move(source, destination,true);
    }

    public void DeleteDirectory(string incomingDataFolder)
    {
        if(Directory.Exists(incomingDataFolder))
            Directory.Delete(incomingDataFolder,true);
    }
}
