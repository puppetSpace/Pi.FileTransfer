using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Files.Commands;
public abstract class AssembleCommandHandlerBase
{
    private readonly Segmentation _segmentation;

    public AssembleCommandHandlerBase(Segmentation segmentation, ILogger logger)
    {
        _segmentation = segmentation;
        Logger = logger;
    }

    protected ILogger Logger { get; }

    protected async Task<string> BuildFile(Folder folder, TransferReceipt transferReceipt, IEnumerable<TransferSegment> transferSegments)
    {
        Logger.BuildingFile(transferReceipt.RelativePath);
        return await _segmentation.BuildFile(folder, transferReceipt.FileId, transferSegments.OrderBy(x => x.Sequencenumber).Select(x => x.Buffer));
    }
}
