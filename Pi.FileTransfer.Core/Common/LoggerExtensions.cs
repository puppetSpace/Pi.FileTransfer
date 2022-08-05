using Microsoft.Extensions.Logging;

namespace Pi.FileTransfer.Core;

public static partial class LoggerExtensionFileAssemblerService
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Searching for receipts in '{folderPath}'")]
    public static partial void SearchReceiptInFolder(this ILogger logger, string folderPath);


    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "'{File}' exists out of {ExpectedAmountOfSegments}. {ActualAmountOfSegments} segment files are present")]
    public static partial void AmountOfSegmentFilesPresent(this ILogger logger, string file, int expectedAmountOfSegments, int ActualAmountOfSegments);

}

public static partial class LoggerExtensionFileIndexerService
{

    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Indexing folders")]
    public static partial void IndexingFolders(this ILogger logger);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Indexing files")]
    public static partial void IndexingFiles(this ILogger logger);

}

public static partial class LoggerExtensionFileRetryService
{

    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Retry sending segments of folder '{folder}' for destination '{destination}'")]
    public static partial void RetrySendingSegments(this ILogger logger,string folder,string destination);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Retry sending receipts of folder '{folder}' for destination '{destination}'")]
    public static partial void RetrySendingReceipts(this ILogger logger, string folder, string destination);

}

public static partial class LoggerExtensionsAddFileCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Processing new file {File}")]
    public static partial void ProcessNewFile(this ILogger logger, string file);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Clearing last read position for '{File}'")]
    public static partial void ClearLastPosition(this ILogger logger, string file);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "Sending segment of file '{File}' to '{Destination}'")]
    public static partial void SendSegment(this ILogger logger, string file, string destination);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Error,
    Message = "Failed to send segment of file '{File}' to '{Destination}'")]
    public static partial void SendSegmentFailed(this ILogger logger, string file, string destination, Exception exception);

    [LoggerMessage(
    EventId = 5,
    Level = LogLevel.Information,
    Message = "Sending receipt of file '{File}' to '{Destination}'")]
    public static partial void SendReceipt(this ILogger logger, string file, string destination);

    [LoggerMessage(
    EventId = 6,
    Level = LogLevel.Error,
    Message = "Failed to send receipt of file '{File}' to '{Destination}'")]
    public static partial void SendReceiptFailed(this ILogger logger, string file, string destination, Exception exception);
}

public static partial class LoggerExtensionsAssembleFileCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Building file {File}")]
    public static partial void BuildingFile(this ILogger logger, string file);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Building file {File} failed.")]
    public static partial void BuildingFileFailed(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Error,
    Message = "Applying delta to file {File} failed.")]
    public static partial void ApplyingDeltaFailed(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Warning,
    Message = "File {File} does not exists. Unable to apply delta.")]
    public static partial void FileDoesNotExistForApplyingDelta(this ILogger logger, string file);
}

public static partial class LoggerExtensionsIndexFileCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Indexing files for folder '{Folder}'")]
    public static partial void IndexingFiles(this ILogger logger, string folder);
}

public static partial class LoggerExtensionsIndexFolderCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Indexing folder '{Folder}'")]
    public static partial void IndexingFolder(this ILogger logger, string folder);
}

public static partial class LoggerExtensionsRetryTransferReceiptCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Retry sending receipt of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void RetryTransferReceipt(this ILogger logger, Guid fileId, string folder, string destination);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to retry sending receipt of file with id {FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void FailedRetryTransferReceipt(this ILogger logger, Guid fileId, string folder, string destination, Exception exception);
}

public static partial class LoggerExtensionsRetryTransferSegmentCommand
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "Retry sending segment {Sequencenumber} of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void RetryTransferSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Information,
    Message = "Retry sending segment {Sequencenumber} of delta from file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void RetryDeltaTransferSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to retry sending segment {Sequencenumber} of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void FailedRetryTransferSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination, Exception exception);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Warning,
    Message = "File with id {FileId} not found in index of folder '{Folder}'. Deleting all failed items")]
    public static partial void DeleteFailedItems(this ILogger logger, Guid fileId, string folder);
}

public static partial class LoggerExtensionsDataStore
{
    [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Debug,
    Message = "Storing last positition '{LastPosition}' of file '{File}' for destination '{Destination}'")]
    public static partial void StoreLastPosition(this ILogger logger, int lastPosition, string file, string destination);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Debug,
    Message = "Storing failed segment '{Sequencenumber}' of file '{File}' for destination '{Destination}'")]
    public static partial void StoreFailedSegment(this ILogger logger, int sequencenumber, string file, string destination);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Debug,
    Message = "Storing failed receipt of file '{File}' for destination '{Destination}'")]
    public static partial void StoreFailedReceipt(this ILogger logger, string file, string destination);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Debug,
    Message = "Getting {AmountOfSegments} failed segments from folder {Folder}' for destination '{Destination}'")]
    public static partial void GetFailedSegments(this ILogger logger, int amountOfSegments, string folder, string destination);

    [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Debug,
    Message = "Getting {AmountOfReceipts} failed receipts from folder {Folder}' for destination '{Destination}'")]
    public static partial void GetFailedReceipts(this ILogger logger, int amountOfReceipts, string folder, string destination);

    [LoggerMessage(
    EventId = 5,
    Level = LogLevel.Debug,
    Message = "Storing received segment {Sequencenumber} of file with id '{FileId}' for folder '{Folder}'")]
    public static partial void StoreReceivedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder);

    [LoggerMessage(
    EventId = 6,
    Level = LogLevel.Debug,
    Message = "Storing received receipt of file '{File}' for folder '{Folder}'")]
    public static partial void StoreReceivedReceipt(this ILogger logger, string file, string folder);

    [LoggerMessage(
    EventId = 7,
    Level = LogLevel.Debug,
    Message = "Getting {AmountOfSegments} received segments for file with id '{FileId}' of folder '{Folder}'")]
    public static partial void GetReceivedSegments(this ILogger logger, int amountOfSegments, Guid fileId, string folder);

    [LoggerMessage(
    EventId = 9,
    Level = LogLevel.Debug,
    Message = "Getting {AmountOfReceipts} received receipts for folder '{Folder}'")]
    public static partial void GetReceivedReceipts(this ILogger logger, int amountOfReceipts, string folder);

    [LoggerMessage(
    EventId = 10,
    Level = LogLevel.Debug,
    Message = "Deleting failed item of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void DeleteFailedItemOfFile(this ILogger logger, Guid fileId, string folder, string destination);

    [LoggerMessage(
    EventId = 11,
    Level = LogLevel.Error,
    Message = "Failed to delete failed segmentof file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void FailedToDeleteFailedItemOfFile(this ILogger logger, Guid fileId, string folder, string destination, Exception exception);

    [LoggerMessage(
    EventId = 12,
    Level = LogLevel.Debug,
    Message = "Deleting failed segment '{SegmentFile}' for destination '{Destination}'")]
    public static partial void DeleteFailedSegment(this ILogger logger, string segmentFile, string destination);

    [LoggerMessage(
    EventId = 13,
    Level = LogLevel.Error,
    Message = "Failed to delete failed segment '{Sequencenumber}' of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void FailedToDeleteFailedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination, Exception exception);

    [LoggerMessage(
    EventId = 14,
    Level = LogLevel.Debug,
    Message = "Deleting failed receipt '{ReceiptFile}' for destination '{Destination}'")]
    public static partial void DeleteFailedReceipt(this ILogger logger, string receiptFile, string destination);

    [LoggerMessage(
    EventId = 15,
    Level = LogLevel.Debug,
    Message = "Failed to delete failed receipt of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'")]
    public static partial void FailedToDeleteFailedReceipt(this ILogger logger, Guid fileId, string folder, string destination, Exception exception);

    [LoggerMessage(
    EventId = 16,
    Level = LogLevel.Debug,
    Message = "Deleting received data from folder '{Folder}' for file with id '{FileId}'")]
    public static partial void DeleteReceivedData(this ILogger logger, string folder, Guid fileId);

    [LoggerMessage(
    EventId = 17,
    Level = LogLevel.Debug,
    Message = "Clear last position of file '{File}' for destination '{Destination}'\"")]
    public static partial void ClearLastPosition(this ILogger logger, string file, string destination);
    [LoggerMessage(
    EventId = 18,
    Level = LogLevel.Error,
    Message = "Failed to delete received data from folder '{Folder}' for file with id '{FileId}'s")]
    public static partial void FailedToDeleteReceivedData(this ILogger logger, string folder, Guid fileId, Exception exception);

    [LoggerMessage(
    EventId = 19,
    Level = LogLevel.Error,
    Message = "Failed to clear last position of file '{File}' for destination '{Destination}'")]
    public static partial void FailedToClearLastPosition(this ILogger logger, string file, string destination, Exception exception);

    [LoggerMessage(
    EventId = 20,
    Level = LogLevel.Error,
    Message = "Failed to get content of receipt '{file}'")]
    public static partial void FailedToGetContentOfReceipt(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 21,
    Level = LogLevel.Error,
    Message = "Failed to get content of segment '{file}'")]
    public static partial void FailedToGetContentOfSegment(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 22,
    Level = LogLevel.Error,
    Message = "Failed to store received receipt of file '{File}' for folder '{Folder}'")]
    public static partial void FailedToStoreReceivedReceipt(this ILogger logger, string file, string folder, Exception exception);

    [LoggerMessage(
    EventId = 23,
    Level = LogLevel.Error,
    Message = "Failed to store received segment {Sequencenumber} of file with id '{FileId}' for folder '{Folder}'")]
    public static partial void FailedToStoreReceivedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, Exception exception);

    [LoggerMessage(
    EventId = 24,
    Level = LogLevel.Error,
    Message = "Failed to get content of failed receipt '{file}'")]
    public static partial void FailedToGetContentOfFailedReceipt(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 25,
    Level = LogLevel.Error,
    Message = "Failed to get content of failed segment '{file}'")]
    public static partial void FailedToGetContentOfFailedSegment(this ILogger logger, string file, Exception exception);

    [LoggerMessage(
    EventId = 26,
    Level = LogLevel.Debug,
    Message = "Failed to store failed receipt of file '{File}' for destination '{Destination}'")]
    public static partial void FailedToStoreFailedReceipt(this ILogger logger, string file, string destination, Exception exception);

    [LoggerMessage(
    EventId = 27,
    Level = LogLevel.Error,
    Message = "Failed to store failed segment '{Sequencenumber}' of file '{File}' for destination '{Destination}'")]
    public static partial void FailedToStoreFailedSegment(this ILogger logger, int sequencenumber, string file, string destination, Exception exception);

    [LoggerMessage(
    EventId = 28,
    Level = LogLevel.Error,
    Message = "Failed to store last positition '{LastPosition}' of file '{File}' for destination '{Destination}'")]
    public static partial void FailedToStoreLastPosition(this ILogger logger, int lastPosition, string file, string destination, Exception exception);

    [LoggerMessage(
    EventId = 29,
    Level = LogLevel.Debug,
    Message = "Creating delta signature file for '{file}' with name '{fileId}'")]
    public static partial void CreateSignatureFile(this ILogger logger, string file, Guid fileId);

    [LoggerMessage(
    EventId = 30,
    Level = LogLevel.Error,
    Message = "Failed to create delta signature file for '{file}' with name '{fileId}'")]
    public static partial void FailedToCreateSignatureFile(this ILogger logger, string file, Guid fileId, Exception exception);

    [LoggerMessage(
    EventId = 31,
    Level = LogLevel.Debug,
    Message = "Get delta signature content for '{file}' with name '{fileId}'")]
    public static partial void GetSignatureFileContent(this ILogger logger, string file, Guid fileId);

    [LoggerMessage(
    EventId = 32,
    Level = LogLevel.Error,
    Message = "Failed to get delta signature content for '{file}' with name '{fileId}'")]
    public static partial void FailedToGetGetSignatureFileContent(this ILogger logger, string file, Guid fileId, Exception exception);
}

public static partial class LoggerExtensionsFileIndex
{
     [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Debug,
    Message = "Remove file '{File}' from folder '{Folder}' index")]
    public static partial void RemoveFileFromIndex(this ILogger logger, string file, string folder);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Debug,
    Message = "Add file '{File}' to folder '{Folder}' index")]
    public static partial void AddFileToIndex(this ILogger logger, string file, string folder);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Debug,
    Message = "Update file '{File}' in folder '{Folder}' index")]
    public static partial void UpdateFileInIndex(this ILogger logger, string file, string folder);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Error,
    Message = "Failed to index files for folder '{Folder}")]
    public static partial void FailedToIndexFiles(this ILogger logger, string folder, Exception exception);
}

public static partial class LoggerExtensionsTransferService
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Debug,
        Message = "Sending segment {Sequencenumber} of file with id {FileId} from folder {FolderName} to {destination}"
        )]
    public static partial void SendSegment(this ILogger logger, int sequenceNumber, Guid fileId, string folderName, string destination);


    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Sending receipt of file {filepath} from folder {FolderName} to {destination}")]
    public static partial void SendReceipt(this ILogger logger, string filePath, string folderName, string destination);
}

public static partial class LoggerExtensionsFileSegmentation
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Debug,
        Message = "Segmenting file '{File}'. Segment: {SegmentCount}, BytesRead: {BytesRead}")]
    public static partial void SegmentingFile(this ILogger logger, string file, int segmentCount, int bytesRead);

    [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Debug,
    Message = "Getting segment for file '{File}'. Start: {Start}, End: {End}")]
    public static partial void GetSpecificSegment(this ILogger logger, string file, int start, int end);

    [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Debug,
    Message = "Building temp file '{File}' from {SegmentCount} segments")]
    public static partial void BuildFileInTemp(this ILogger logger, string file, int segmentCount);













}

public static partial class LoggerExtensionsDeltaService
{
    [LoggerMessage(
        EventId = 0, 
        Level = LogLevel.Debug, 
        Message = "operation: {operation}, currentPosition: {currentPosition}, total: {total}")]
    public static partial void ReportDeltaProgress(this ILogger logger, string operation, long currentPosition, long total);
}

public static partial class LoggerExtensionsUpdateFileCommand
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Processing updated file {File}")]
    public static partial void ProcessUpdatedFile(this ILogger logger, string file);
}

public static partial class LoggerExtensionsDeltaService
{
    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Debug,
       Message = "Creating delta signature for file {File}")]
    public static partial void CreateSignature(this ILogger logger,string file);

    [LoggerMessage(
       EventId = 1,
       Level = LogLevel.Debug,
       Message = "Creating delta for file {File}")]
    public static partial void CreateDelta(this ILogger logger, string file);

    [LoggerMessage(
       EventId = 2,
       Level = LogLevel.Debug,
       Message = "Applying delta '{source}' to file {File}")]
    public static partial void ApplyDelta(this ILogger logger,string source, string file);

    [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Warning,
    Message = "Failed to delete temp delta file {File}")]
    public static partial void FailedToDeleteTempDeltaFile(this ILogger logger, string file,Exception exception);
}

public static partial class LoggerExtensionsFileSystem
{
    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Debug,
       Message = "Getting {noOfFiles} files for folder '{folder}'")]
    public static partial void GetFilesForFolder(this ILogger logger, int noOfFiles ,string folder);

    [LoggerMessage(
       EventId = 1,
       Level = LogLevel.Debug,
       Message = "Getting {noOfFolders} folders for folder '{folder}'")]
    public static partial void GetFoldersFromBasePath(this ILogger logger, int noOfFolders, string folder);

    [LoggerMessage(
       EventId = 2,
       Level = LogLevel.Debug,
       Message = "Opening readonly filestream for '{file}' with buffersize {bufferSize}")]
    public static partial void OpeningReadOnlyStream(this ILogger logger, string file, int bufferSize);

    [LoggerMessage(
   EventId = 3,
   Level = LogLevel.Debug,
   Message = "Opening writeonly filestream for '{file}' with buffersize {bufferSize}")]
    public static partial void OpeningWriteOnlyStream(this ILogger logger, string file, int bufferSize);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Creating folder {folder}")]
    public static partial void CreateFolder(this ILogger logger, string folder);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Write to {file} with content type of {type}")]
    public static partial void WriteToFile(this ILogger logger, string file,string? type);

    [LoggerMessage(
    EventId = 6,
    Level = LogLevel.Debug,
    Message = "Read content from file {file} with content type of {type}")]
    public static partial void ReadFromFile(this ILogger logger, string file, string? type);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Delete file {file}")]
    public static partial void DeleteFile(this ILogger logger, string file);

    [LoggerMessage(
    EventId = 8,
    Level = LogLevel.Debug,
    Message = "Copy file {source} to {destination}")]
    public static partial void CopyFile(this ILogger logger, string source, string destination);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Move file {source} to {destination}")]
    public static partial void MoveFile(this ILogger logger, string source, string destination);

    [LoggerMessage(
    EventId = 10,
    Level = LogLevel.Debug,
    Message = "Delete folder {folder}")]
    public static partial void DeleteFolder(this ILogger logger, string folder);
}