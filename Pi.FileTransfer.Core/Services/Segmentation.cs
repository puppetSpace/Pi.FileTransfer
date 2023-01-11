using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
public abstract class Segmentation
{
    private readonly IFileSystem _fileSystem;
    private readonly DataStore _dataStore;
    private readonly ILogger _logger;
    private readonly int _sizeOfSegment;

	public Segmentation(IFileSystem fileSystem, DataStore dataStore, IOptions<AppSettings> options,ILogger logger)
	{
        _fileSystem = fileSystem;
        _dataStore = dataStore;
        _sizeOfSegment = options.Value.SizeOfSegmentInBytes;
        _logger = logger;
	}


    public async Task<int> Segment(Entities.Folder folder, Entities.File file, Func<int, byte[], Entities.Folder, Entities.File, Task> segmentCreatedFunc)
    {
        return await Segment(folder, file, 0, segmentCreatedFunc);
    }

	public async Task<int> Segment(Entities.Folder folder, Entities.File file, int startPosition, Func<int, byte[], Entities.Folder, Entities.File, Task> segmentCreatedFunc)
	{
		using var fs = GetStream(folder, file);
		var buffer = ArrayPool<byte>.Shared.Rent(_sizeOfSegment);
		int bytesRead = startPosition;
        //todo test
		var segmentcount = startPosition <= _sizeOfSegment ? 0 : startPosition / _sizeOfSegment;
        fs.Position = startPosition;
		while ((bytesRead = await fs.ReadAsync(buffer)) > 0)
		{
			await segmentCreatedFunc(++segmentcount, buffer[0..bytesRead], folder, file);
			_logger.SegmentingFile(file.RelativePath, segmentcount, bytesRead);
		}

		return segmentcount;
	}

	public async Task<byte[]> GetSpecificSegment(Entities.Folder folder, Entities.File file, SegmentRange range)
    {
        _logger.GetSpecificSegment(file.RelativePath, range.Start, range.End);
        var buffer = ArrayPool<byte>.Shared.Rent(_sizeOfSegment);

        using var fs = GetStream(folder,file);
        fs.Seek(range.Start, SeekOrigin.Begin);
        var memory = buffer.AsMemory(0, range.Length);
        await fs.ReadAsync(memory);

        return memory.ToArray();

    }

    public async Task<string> BuildFile(Entities.Folder folder, Guid fileId, IEnumerable<byte[]> bytes)
    {
        var filePath = _dataStore.GetIncomingTempFilePath(folder, fileId.ToString());

        _logger.BuildFileInTemp(filePath, bytes.Count());

        using var stream = _fileSystem.GetWriteFileStream(filePath);
        foreach (var segment in bytes)
            await stream.WriteAsync(segment);

        return filePath;
    }

    protected abstract Stream GetStream(Entities.Folder folder, Entities.File file);

}
