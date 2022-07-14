using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class FileSegmentation
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<FileSegmentation> _logger;

    public FileSegmentation(IFileSystem fileSystem, ILogger<FileSegmentation> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<int> SegmentFile(Entities.Folder folder, Entities.File file, Func<int, byte[] , Entities.Folder, Entities.File ,Task> segmentCreatedFunc)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(2048);

        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath(folder));
        int bytesRead = 0;
        var segmentcount = 0;
        while ((bytesRead = await fs.ReadAsync(buffer)) > 0)
        {
            await segmentCreatedFunc(++segmentcount, buffer[0..bytesRead],folder,file);
            _logger.SegmentingFile(file.RelativePath,segmentcount, bytesRead);
        }

        return segmentcount;
    }

    public async Task<byte[]> GetSpecificSegment(Entities.Folder folder, Entities.File file,SegmentRange range)
    {
        _logger.GetSpecificSegment(file.RelativePath, range.Start, range.End);
        var buffer = ArrayPool<byte>.Shared.Rent(2048);

        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath(folder));
        fs.Seek(range.Start, SeekOrigin.Begin);
        var memory = buffer.AsMemory(0, range.Length);
        await fs.ReadAsync(memory);

        return memory.ToArray();

    }

    public async Task<string> BuildFile(Entities.Folder folder, Guid fileId, IEnumerable<byte[]> bytes)
    {
        var tempPath = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", "Temp");
        _fileSystem.CreateDirectory(tempPath);
        var filePath = Path.Combine(tempPath, fileId.ToString());

        _logger.BuildFileInTemp(filePath, bytes.Count());

        using var stream = _fileSystem.GetWriteFileStream(filePath);
        foreach (var segment in bytes)
            await stream.WriteAsync(segment);

        return filePath;
    }
}
