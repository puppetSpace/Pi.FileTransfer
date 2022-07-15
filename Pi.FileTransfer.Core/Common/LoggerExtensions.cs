using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core;

public static class LoggerExtensionFileAssemblerService
{
    private static readonly Action<ILogger, string, Exception?> _searchReceipts;
    private static readonly Action<ILogger, string, int, int, Exception?> _amountOfSegmentFilesPresent;

    static LoggerExtensionFileAssemblerService()
    {
        _searchReceipts = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Searching for receipts in '{FolderPath}'");
        _amountOfSegmentFilesPresent = LoggerMessage.Define<string, int, int>(LogLevel.Information, new EventId(), "'{File}' exists out of {ExpectedAmountOfSegments}. {ActualAmountOfSegments} segment files are present");


    }
    public static void SearchReceiptInFolder(this ILogger logger, string folderPath) => _searchReceipts(logger, folderPath, null);

    public static void AmountOfSegmentFilesPresent(this ILogger logger, string file, int expectedAmountOfSegments, int ActualAmountOfSegments)
    => _amountOfSegmentFilesPresent(logger, file, expectedAmountOfSegments, ActualAmountOfSegments, null);

}

public static class LoggerExtensionFileIndexerService
{
    private static readonly Action<ILogger, Exception?> _indexingFolders;
    private static readonly Action<ILogger, Exception?> _indexingFiles;

    static LoggerExtensionFileIndexerService()
    {
        _indexingFolders = LoggerMessage.Define(LogLevel.Information, new EventId(), "Indexing folders");
        _indexingFiles = LoggerMessage.Define(LogLevel.Information, new EventId(), "Indexing files");
    }
    public static void IndexingFolders(this ILogger logger) => _indexingFolders(logger, null);

    public static void IndexingFiles(this ILogger logger)
    => _indexingFiles(logger, null);

}

public static class LoggerExtensionFileRetryService
{
    private static readonly Action<ILogger, Exception?> _retrySendingSegments;
    private static readonly Action<ILogger, Exception?> _retrySendingReceipts;

    static LoggerExtensionFileRetryService()
    {
        _retrySendingSegments = LoggerMessage.Define(LogLevel.Information, new EventId(), "Indexing folders");
        _retrySendingReceipts = LoggerMessage.Define(LogLevel.Information, new EventId(), "Indexing files");
    }
    public static void RetrySendingSegments(this ILogger logger) => _retrySendingSegments(logger, null);

    public static void RetrySendingReceipts(this ILogger logger)
    => _retrySendingReceipts(logger, null);

}

public static class LoggerExtensionsAddFileCommand
{
    private static readonly Action<ILogger, string, Exception?> _segmentFile;
    private static readonly Action<ILogger, string, Exception?> _clearLastPosition;
    private static readonly Action<ILogger, string, string, Exception?> _sendSegment;
    private static readonly Action<ILogger, string, string, Exception?> _sendSegmentFailed;
    private static readonly Action<ILogger, string, string, Exception?> _sendReceipt;
    private static readonly Action<ILogger, string, string, Exception?> _sendReceiptFailed;

    static LoggerExtensionsAddFileCommand()
    {
        _segmentFile = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Segmenting file {File}");
        _clearLastPosition = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Clearing last read position for '{File}'");
        _sendSegment = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(), "Sending segment of file '{File}' to '{Destination}'");
        _sendSegmentFailed = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(), "Failed to send segment of file '{File}' to '{Destination}'");
        _sendReceipt = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(), "Sending receipt of file '{File}' to '{Destination}'");
        _sendReceiptFailed = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(), "Failed to send receipt of file '{File}' to '{Destination}'");
    }

    public static void SegmentFile(this ILogger logger, string file)
    => _segmentFile(logger, file, null);

    public static void ClearLastPosition(this ILogger logger, string file)
     => _clearLastPosition(logger, file, null);

    public static void SendSegment(this ILogger logger, string file, string destination)
    => _sendSegment(logger, file, destination, null);

    public static void SendSegmentFailed(this ILogger logger, string file, string destination, Exception exception)
    => _sendSegmentFailed(logger, file, destination, exception);

    public static void SendReceipt(this ILogger logger, string file, string destination)
        => _sendReceipt(logger, file, destination, null);

    public static void SendReceiptFailed(this ILogger logger, string file, string destination, Exception exception)
        => _sendReceiptFailed(logger, file, destination, exception);
}

public static class LoggerExtensionsAssembleFileCommand
{
    private static readonly Action<ILogger, string, Exception?> _buildingFile;
    private static readonly Action<ILogger, string, Exception?> _buildingFileFailed;
    static LoggerExtensionsAssembleFileCommand()
    {
        _buildingFile = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Building file {File}");
        _buildingFileFailed = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Building file {File} failed.");

    }

    public static void BuildingFile(this ILogger logger, string file) => _buildingFile(logger, file, null);
    public static void BuildingFileFailed(this ILogger logger, string file, Exception exception) => _buildingFileFailed(logger, file, exception);
}

public static class LoggerExtensionsIndexFileCommand
{
    private static readonly Action<ILogger, string, Exception?> _indexingFiles;

    static LoggerExtensionsIndexFileCommand()
    {
        _indexingFiles = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Indexing files for folder '{Folder}'");
    }

    public static void IndexingFiles(this ILogger logger, string folder) => _indexingFiles(logger, folder, null);
}

public static class LoggerExtensionsIndexFolderCommand
{
    private static readonly Action<ILogger, string, Exception?> _indexingFolder;

    static LoggerExtensionsIndexFolderCommand()
    {
        _indexingFolder = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Indexing folder '{Folder}'");
    }

    public static void IndexingFolder(this ILogger logger, string folder)
    => _indexingFolder(logger, folder, null);
}

public static class LoggerExtensionsRetryTransferReceiptCommand
{
    private static readonly Action<ILogger, Guid, string, string, Exception?> _retryTransferReceipt;
    private static readonly Action<ILogger, Guid, string, string, Exception?> _failedRetryTransferReceipt;

    static LoggerExtensionsRetryTransferReceiptCommand()
    {
        _retryTransferReceipt = LoggerMessage.Define<Guid, string, string>(LogLevel.Information, new EventId(), "Retry sending receipt of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _failedRetryTransferReceipt = LoggerMessage.Define<Guid, string, string>(LogLevel.Error, new EventId(), "Failed to retry sending receipt of file with id {FileId}' from folder '{Folder}' for destination '{Destination}'");
    }

    public static void RetryTransferReceipt(this ILogger logger, Guid fileId, string folder, string destination)
        => _retryTransferReceipt(logger, fileId, folder, destination, null);

    public static void FailedRetryTransferReceipt(this ILogger logger, Guid fileId, string folder, string destination, Exception exception)
        => _failedRetryTransferReceipt(logger, fileId, folder, destination, exception);
}

public static class LoggerExtensionsRetryTransferSegmentCommand
{
    private static readonly Action<ILogger, int, Guid, string, string, Exception?> _retryTransferSegment;
    private static readonly Action<ILogger, int, Guid, string, string, Exception?> _failedRetryTransferSegment;
    private static readonly Action<ILogger, Guid, string, Exception?> _deleteFailedItems;

    static LoggerExtensionsRetryTransferSegmentCommand()
    {
        _retryTransferSegment = LoggerMessage.Define<int, Guid, string, string>(LogLevel.Information, new EventId(), "Retry sending segment {Sequencenumber} of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _failedRetryTransferSegment = LoggerMessage.Define<int, Guid, string, string>(LogLevel.Error, new EventId(), "Failed to retry sending segment {FailedSegment} of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _deleteFailedItems = LoggerMessage.Define<Guid, string>(LogLevel.Warning, new EventId(), "File with id {FileId} not found in index of folder '{Folder}'. Deleting all failed items");
    }

    public static void RetryTransferSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination)
        => _retryTransferSegment(logger, sequencenumber, fileId, folder, destination, null);

    public static void FailedRetryTransferSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination, Exception exception)
        => _failedRetryTransferSegment(logger, sequencenumber, fileId, folder, destination, exception);

    public static void DeleteFailedItems(this ILogger logger, Guid fileId, string folder)
    => _deleteFailedItems(logger, fileId, folder, null);
}

public static class LoggerExtensionsDataStore
{
    private static readonly Action<ILogger, int, string, string, Exception?> _storeLastPosition;
    private static readonly Action<ILogger, int, string, string, Exception?> _storeFailedSegment;
    private static readonly Action<ILogger, string, string, Exception?> _storeFailedReceipt;
    private static readonly Action<ILogger, int, string, string, Exception?> _getFailedSegments;
    private static readonly Action<ILogger, int, string, string, Exception?> _getFailedReceipts;
    private static readonly Action<ILogger, int, Guid, string, Exception?> _storeReceivedSegment;
    private static readonly Action<ILogger, string, string, Exception?> _storeReceivedReceipt;
    private static readonly Action<ILogger, int, Guid, string, Exception?> _getReceivedSegments;
    private static readonly Action<ILogger, int, string, Exception?> _getReceivedReceipts;
    private static readonly Action<ILogger, Guid, string, string, Exception?> _deleteFailedItemOfFile;
    private static readonly Action<ILogger, Guid, string, string, Exception> _failedDeleteFailedItemOfFile;
    private static readonly Action<ILogger, string, string, Exception?> _deleteFailedSegment;
    private static readonly Action<ILogger, int, Guid, string, string, Exception> _failedDeleteFailedSegment;
    private static readonly Action<ILogger, string, string, Exception?> _deleteFailedReceipt;
    private static readonly Action<ILogger, Guid, string, string, Exception> _failedDeleteFailedReceipt;
    private static readonly Action<ILogger, string, Guid, Exception?> _deleteReceivedData;
    private static readonly Action<ILogger, string, Guid, Exception> _failedDeleteReceivedData;
    private static readonly Action<ILogger, string, string, Exception?> _clearLastPosition;
    private static readonly Action<ILogger, string, string, Exception> _failedClearLastPosition;
    private static readonly Action<ILogger, string, Exception> _failedToGetContentOfReceipt;
    private static readonly Action<ILogger, string, Exception> _failedToGetContentOfSegment;
    private static readonly Action<ILogger, string,string, Exception> _failedToStoreReceipt;
    private static readonly Action<ILogger, int, Guid, string, Exception> _failedToStoreSegment;
    private static readonly Action<ILogger, string, Exception> _failedToGetContentOfFailedSegment;
    private static readonly Action<ILogger, string, Exception> _failedToGetContentOfFailedReceipt;
    private static readonly Action<ILogger, string, string, Exception> _failedToStoreFailedReceipt;
    private static readonly Action<ILogger, int, string, string, Exception?> _failedToStoreFailedSegment;
    private static readonly Action<ILogger, int, string, string, Exception?> _failedToStoreLastPosition;




    static LoggerExtensionsDataStore()
    {
        _storeLastPosition = LoggerMessage.Define<int, string, string>(LogLevel.Debug, new EventId(), "Storing last positition '{LastPosition}' of file '{File}' for destination '{Destination}'");
        _storeFailedSegment = LoggerMessage.Define<int, string, string>(LogLevel.Debug, new EventId(), "Storing failed segment '{Sequencenumber}' of file '{File}' for destination '{Destination}'");
        _storeFailedReceipt = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Storing failed receipt of file '{File}' for destination '{Destination}'");
        _getFailedSegments = LoggerMessage.Define<int, string, string>(LogLevel.Debug, new EventId(), "Getting {AmountOfSegments} failed segments from folder {Folder}' for destination '{Destination}'");
        _getFailedReceipts = LoggerMessage.Define<int, string, string>(LogLevel.Debug, new EventId(), "Getting {AmountOfReceipts} failed receipts from folder {Folder}' for destination '{Destination}'");
        _storeReceivedSegment = LoggerMessage.Define<int, Guid, string>(LogLevel.Debug, new EventId(), "Storing received segment {Sequencenumber} of file with id '{FileId}' for folder '{Folder}'");
        _storeReceivedReceipt = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Storing received receipt of file '{File}' for folder '{Folder}'");
        _getReceivedSegments = LoggerMessage.Define<int, Guid, string>(LogLevel.Debug, new EventId(), "Getting {AmountOfSegments} received segments for file with id '{FileId}' of folder '{Folder}'");
        _getReceivedReceipts = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(), "Getting {AmountOfReceipts} received receipts for folder '{Folder}'");
        _deleteFailedItemOfFile = LoggerMessage.Define<Guid, string, string>(LogLevel.Debug, new EventId(), "Deleting failed item of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _failedDeleteFailedItemOfFile = LoggerMessage.Define<Guid, string, string>(LogLevel.Error, new EventId(), "Exception happend while deleting failed items of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _deleteFailedSegment = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Deleting failed segment '{SegmentFile}' for destination '{Destination}'");
        _failedDeleteFailedSegment = LoggerMessage.Define<int, Guid, string, string>(LogLevel.Error, new EventId(), "Exception happend while deleting failed segment '{Sequencenumber}' of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _deleteFailedReceipt = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Deleting failed receipt '{File}' for destination '{Destination}'");
        _failedDeleteFailedReceipt = LoggerMessage.Define<Guid, string, string>(LogLevel.Error, new EventId(), "Exception happend while deleting failed receipt of file with id '{FileId}' from folder '{Folder}' for destination '{Destination}'");
        _deleteReceivedData = LoggerMessage.Define<string, Guid>(LogLevel.Debug, new EventId(), "Deleting received data from folder '{Folder}' for file with id '{FileId}'");
        _clearLastPosition = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Clear last position of file '{File}' for destination '{Destination}'");
        _failedDeleteReceivedData = LoggerMessage.Define<string, Guid>(LogLevel.Error, new EventId(), "Exception happend while deleting received data from folder '{Folder}' for file with id '{FileId}'");
        _failedClearLastPosition = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(), "Exception happend while clearing last position of file '{File}' for destination '{Destination}'");
        _failedToGetContentOfReceipt = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Failed to get content of receipt '{file}'");
        _failedToGetContentOfSegment = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Failed to get content of segment '{file}'");
        _failedToStoreReceipt = LoggerMessage.Define<string,string>(LogLevel.Error, new EventId(), "Exception happend while storing received receipt of file '{File}' for folder '{Folder}'");
        _failedToStoreSegment = LoggerMessage.Define<int, Guid, string>(LogLevel.Error, new EventId(), "Exception happend while storing received segment {Sequencenumber} of file with id '{FileId}' for folder '{Folder}'");
        _failedToGetContentOfFailedReceipt = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Failed to get content of failed receipt '{file}'");
        _failedToGetContentOfFailedSegment = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Failed to get content of failed segment '{file}'");
        _failedToStoreFailedReceipt = LoggerMessage.Define<string, string>(LogLevel.Error, new EventId(), "Exception happend while storing failed receipt of file '{File}' for destination '{Destination}'");
        _failedToStoreFailedSegment = LoggerMessage.Define<int,string, string>(LogLevel.Error, new EventId(), "Exception happend while storing failed segment '{Sequencenumber}' of file '{File}' for destination '{Destination}'");
        _failedToStoreLastPosition = LoggerMessage.Define<int,string, string>(LogLevel.Error, new EventId(), "Exception happend while storing last positition '{LastPosition}' of file '{File}' for destination '{Destination}'");

    }
    public static void StoreLastPosition(this ILogger logger, int lastPosition, string file, string destination)
    => _storeLastPosition(logger, lastPosition, file, destination, null);

    public static void StoreFailedSegment(this ILogger logger, int sequencenumber, string file, string destination)
        => _storeFailedSegment(logger, sequencenumber, file, destination, null);

    public static void StoreFailedReceipt(this ILogger logger, string file, string destination)
        => _storeFailedReceipt(logger, file, destination, null);

    public static void GetFailedSegments(this ILogger logger, int amountOfSegments, string folder, string destination)
        => _getFailedSegments(logger, amountOfSegments, folder, destination, null);

    public static void GetFailedReceipts(this ILogger logger, int amountOfReceipts, string folder, string destination)
        => _getFailedReceipts(logger, amountOfReceipts, folder, destination, null);

    public static void StoreReceivedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder)
        => _storeReceivedSegment(logger, sequencenumber, fileId, folder, null);

    public static void StoreReceivedReceipt(this ILogger logger, string file, string folder)
        => _storeReceivedReceipt(logger, file, folder, null);

    public static void GetReceivedSegments(this ILogger logger, int amountOfSegments, Guid fileId, string folder)
        => _getReceivedSegments(logger, amountOfSegments, fileId, folder, null);

    public static void GetReceivedReceipts(this ILogger logger, int amountOfReceipts, string folder)
        => _getReceivedReceipts(logger, amountOfReceipts, folder, null);

    public static void DeleteFailedItemOfFile(this ILogger logger, Guid fileId, string folder, string destination)
        => _deleteFailedItemOfFile(logger, fileId, folder, destination, null);

    public static void FailedToDeleteFailedItemOfFile(this ILogger logger, Guid fileId, string folder, string destination, Exception exception)
        => _failedDeleteFailedItemOfFile(logger, fileId, folder, destination, exception);

    public static void DeleteFailedSegment(this ILogger logger, string segmentFile, string destination)
        => _deleteFailedSegment(logger, segmentFile, destination, null);

    public static void FailedToDeleteFailedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, string destination, Exception exception)
        => _failedDeleteFailedSegment(logger, sequencenumber, fileId, folder, destination, exception);

    public static void DeleteFailedReceipt(this ILogger logger, string receiptFile, string destination)
        => _deleteFailedReceipt(logger, receiptFile, destination, null);

    public static void FailedToDeleteFailedReceipt(this ILogger logger, Guid fileId, string folder, string destination, Exception exception)
        => _failedDeleteFailedReceipt(logger, fileId, folder, destination, exception);

    public static void DeleteReceivedData(this ILogger logger, string folder, Guid fileId)
        => _deleteReceivedData(logger, folder, fileId, null);

    public static void ClearLastPosition(this ILogger logger, string file,string destination)
        => _clearLastPosition(logger, file, destination, null);

    public static void FailedToDeleteReceivedData(this ILogger logger, string folder, Guid fileId, Exception exception)
        => _failedDeleteReceivedData(logger, folder, fileId, exception);

    public static void FailedToClearLastPosition(this ILogger logger, string file, string destination,Exception exception)
        => _failedClearLastPosition(logger, file, destination, exception);

    public static void FailedToGetContentOfReceipt(this ILogger logger, string file, Exception exception)
        => _failedToGetContentOfReceipt(logger,file,exception);

    public static void FailedToGetContentOfSegment(this ILogger logger, string file, Exception exception)
        => _failedToGetContentOfSegment(logger, file, exception);

    public static void FailedToStoreReceivedReceipt(this ILogger logger, string file, string folder, Exception exception)
        => _failedToStoreReceipt(logger, file, folder, exception);

    public static void FailedToStoreReceivedSegment(this ILogger logger, int sequencenumber, Guid fileId, string folder, Exception exception)
        => _failedToStoreSegment(logger, sequencenumber, fileId, folder, exception);

    public static void FailedToGetContentOfFailedReceipt(this ILogger logger, string file, Exception exception)
        => _failedToGetContentOfFailedReceipt(logger, file, exception);

    public static void FailedToGetContentOfFailedSegment(this ILogger logger, string file, Exception exception)
        => _failedToGetContentOfFailedSegment(logger, file, exception);

    public static void FailedToStoreFailedReceipt(this ILogger logger, string file, string destination, Exception exception)
        => _failedToStoreFailedReceipt(logger, file, destination, exception);

    public static void FailedToStoreFailedSegment(this ILogger logger, int sequencenumber, string file, string destination, Exception exception)
        => _failedToStoreFailedSegment(logger, sequencenumber, file, destination, exception);

    public static void FailedToStoreLastPosition(this ILogger logger, int lastPosition, string file, string destination, Exception exception)
        => _failedToStoreLastPosition(logger, lastPosition, file, destination, exception);
}

public static class LoggerExtensionsFileIndex
{
    private static readonly Action<ILogger, string, string, Exception?> _removeFileFromIndex;
    private static readonly Action<ILogger, string, string, Exception?> _addFileToIndex;
    private static readonly Action<ILogger, string, string, Exception?> _updateFileInIndex;
    private static readonly Action<ILogger, string, Exception?> _failedToIndexFiles;

    static LoggerExtensionsFileIndex()
    {
        _removeFileFromIndex = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Remove file '{File}' from folder '{Folder}' index");
        _addFileToIndex = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Add file '{File}' to folder '{Folder}' index");
        _updateFileInIndex = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Update file '{File}' in folder '{Folder}' index");
        _failedToIndexFiles = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Failed to index files for folder '{Folder}'");
    }

    public static void RemoveFileFromIndex(this ILogger logger, string file, string folder)
        => _removeFileFromIndex(logger, file, folder, null);
    public static void AddFileToIndex(this ILogger logger, string file, string folder)
        => _addFileToIndex(logger, file, folder, null);
    public static void UpdateFileInIndex(this ILogger logger, string file, string folder)
        => _updateFileInIndex(logger, file, folder, null);

    public static void FailedToIndexFiles(this ILogger logger, string folder, Exception exception)
    => _failedToIndexFiles(logger, folder, exception);
}

public static class LoggerExtensions
{


    private static readonly Action<ILogger, string, int, int, Exception?> _segmentingFile;
    private static readonly Action<ILogger, string, int, int, Exception?> _getSpecificSegment;
    private static readonly Action<ILogger, string, int, Exception?> _buildFileInTemp;




 

    static LoggerExtensions()
    {




        
        _segmentingFile = LoggerMessage.Define<string, int, int>(LogLevel.Debug, new EventId(), "Segmenting file '{File}'. Segment: {SegmentCount}, BytesRead: {BytesRead}");
        _getSpecificSegment = LoggerMessage.Define<string, int, int>(LogLevel.Debug, new EventId(), "Getting segment for file '{File}'. Start: {Start}, End: {End}");
        _buildFileInTemp = LoggerMessage.Define<string, int>(LogLevel.Debug, new EventId(), "Building temp file '{File}' from {SegmentCount} segments");
    }






    public static void SegmentingFile(this ILogger logger, string file, int segmentCount, int bytesRead)
        => _segmentingFile(logger, file, segmentCount, bytesRead, null);

    public static void GetSpecificSegment(this ILogger logger, string file, int start, int end)
        => _getSpecificSegment(logger, file, start, end, null);

    public static void BuildFileInTemp(this ILogger logger, string file, int segmentCount)
        => _buildFileInTemp(logger, file, segmentCount, null);













}
