using Microsoft.Extensions.Logging;
using Octodiff.Core;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class DeltaService
{
    private readonly DataStore _dataStore;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeltaService> _logger;

    public DeltaService(DataStore dataStore, IFileSystem fileSystem, ILogger<DeltaService> logger)
    {
        _dataStore = dataStore;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void CreateSignature(Entities.Folder folder, Entities.File file)
    {
        _logger.CreateSignature(file.RelativePath);
        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath(folder));
        var signatureBuilder = new Octodiff.Core.SignatureBuilder();
        using var writeStream = _fileSystem.GetWriteFileStream(_dataStore.GetSignatureFilePath(folder, file));
        var signatureWriter = new Octodiff.Core.SignatureWriter(writeStream);
        signatureBuilder.Build(fs, signatureWriter);
        writeStream.Flush();
    }

    public async Task CreateDelta(Entities.Folder folder, Entities.File file)
    {
        _logger.CreateDelta(file.RelativePath);
        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath(folder));
        using var deltaStream = _fileSystem.GetWriteFileStream(_dataStore.GetDeltaFilePath(folder, file));
        using var signatureStream = new System.IO.MemoryStream(await _dataStore.GetSignatureFileContent(folder, file));
        var signatureReader = new Octodiff.Core.SignatureReader(signatureStream, new LogProgressReporter(_logger));
        var deltaWriter = new Octodiff.Core.AggregateCopyOperationsDecorator(new Octodiff.Core.BinaryDeltaWriter(deltaStream));
        var deltaBuilder = new Octodiff.Core.DeltaBuilder();
        deltaBuilder.BuildDelta(fs, signatureReader, deltaWriter);
        deltaStream.Flush();
    }

    public void ApplyDelta(string deltaFilePath, Entities.Folder folder, string destination)
    {
        var tempPath = _dataStore.GetIncomingTempFilePath(folder, $"delta_{Guid.NewGuid()}");
        _logger.ApplyDelta(tempPath,destination);
        using (var fs = _fileSystem.GetReadFileStream(destination))
        {
            using (var fsout = _fileSystem.GetWriteFileStream(tempPath))
            {
                using (var deltaStream = _fileSystem.GetReadFileStream(deltaFilePath))
                {
                    var deltaReader = new Octodiff.Core.BinaryDeltaReader(deltaStream, new LogProgressReporter(_logger));
                    var deltaApplier = new Octodiff.Core.DeltaApplier { SkipHashCheck = true };
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
            _logger.FailedToDeleteTempDeltaFile(tempPath,ex);
        }
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
