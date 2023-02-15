using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IFileTransferRepository : IRepository
{
    void AddFailedReceipt(FailedReceipt failedReceipt);
    void AddFailedSegment(FailedSegment failedSegment);
    Task AddOrUpdateLastPosition(LastPosition lastPosition);
    Task ClearLastPosition(Core.Files.File file, Destination destination);
    Task<List<FailedReceipt>> GetFailedReceipts(Folder folder, Destination destination);
    Task<List<FailedSegment>> GetFailedSegments(Folder folder, Destination destination);
    Task<LastPosition?> GetLastPosition(Files.File file, Destination destination);
    void RemoveFailedReceipt(FailedReceipt failedReceipt);
    void RemoveFailedSegment(FailedSegment failedSegment);
}
