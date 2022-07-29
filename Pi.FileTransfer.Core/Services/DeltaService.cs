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

    public DeltaService(DataStore dataStore, IFileSystem fileSystem)
    {
        _dataStore = dataStore;
        _fileSystem = fileSystem;
    }

    public void CreateSignature(Entities.Folder folder, Entities.File file)
    {
        using var fs = _fileSystem.GetReadFileStream(file.GetFullPath(folder));
        var signatureBuilder = new Octodiff.Core.SignatureBuilder();
        using var writeStream = _dataStore.CreateSignatureFile(folder, file);
        var signatureWriter = new Octodiff.Core.SignatureWriter(writeStream);
        signatureBuilder.Build(fs, signatureWriter);
        writeStream.Flush();
    }

}
