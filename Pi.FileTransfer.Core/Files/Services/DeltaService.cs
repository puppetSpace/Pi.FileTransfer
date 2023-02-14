using Microsoft.Extensions.Logging;
using Octodiff.Core;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Files;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Files.Services;
public class DeltaService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeltaService> _logger;

    public DeltaService(IFileSystem fileSystem, ILogger<DeltaService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void CreateSignature(Folder folder, File file)
    {
        _logger.CreateSignature(file.RelativePath);
        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath());
        var signatureBuilder = new SignatureBuilder();
        using var writeStream = _fileSystem.GetWriteFileStream(FolderUtils.GetSignaturesFolderPath(folder.FullName,file.Id.ToString()));
        var signatureWriter = new SignatureWriter(writeStream);
        signatureBuilder.Build(fs, signatureWriter);
        writeStream.Flush();
    }

    public async Task CreateDelta(Folder folder, File file)
    {
        _logger.CreateDelta(file.RelativePath);

        var signatureFile = FolderUtils.GetSignaturesFolderPath(file.Id.ToString());
        if (!_fileSystem.FileExist(signatureFile))
        {
            _logger?.NoSignatureForFilePresent(file.GetFullPath());
        }
        else
        {
            using var fs = _fileSystem.GetReadFileStream(file.GetFullPath());
            using var deltaStream = _fileSystem.GetWriteFileStream(FolderUtils.GetDeltasFolderPath(folder.FullName, file.Id.ToString()));
            using var signatureStream = new MemoryStream(await _fileSystem.GetRawContentOfFile(signatureFile));
            var signatureReader = new SignatureReader(signatureStream, new LogProgressReporter(_logger));
            var deltaWriter = new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream));
            var deltaBuilder = new DeltaBuilder();
            deltaBuilder.BuildDelta(fs, signatureReader, deltaWriter);
            deltaStream.Flush();
        }
    }

    public DateTime ApplyDelta(string deltaFilePath, Folder folder, string destination)
    {
        var tempPath = FolderUtils.GetIncomingFolderTempPath(folder.FullName, $"delta_{Guid.NewGuid()}");
        _logger.ApplyDelta(tempPath, destination);
        using (var fs = _fileSystem.GetReadFileStream(destination))
        {
            using (var fsout = _fileSystem.GetWriteFileStream(tempPath))
            {
                using (var deltaStream = _fileSystem.GetReadFileStream(deltaFilePath))
                {
                    var deltaReader = new BinaryDeltaReader(deltaStream, new LogProgressReporter(_logger));
                    var deltaApplier = new DeltaApplier { SkipHashCheck = true };
                    deltaApplier.Apply(fs, deltaReader, fsout);
                }
            }
        }

        _fileSystem.CopyFile(tempPath, destination);

        try
        {
            _fileSystem.DeleteFile(tempPath);
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteTempDeltaFile(tempPath, ex);
        }

        return System.IO.File.GetLastWriteTimeUtc(destination);
    }

    private class LogProgressReporter : Octodiff.Diagnostics.IProgressReporter
    {
        private readonly ILogger _logger;

        public LogProgressReporter(ILogger logger)
        {
            _logger = logger;
        }

        public void ReportProgress(string operation, long currentPosition, long total)
        {
            _logger.ReportDeltaProgress(operation, currentPosition, total);
        }
    }

}
