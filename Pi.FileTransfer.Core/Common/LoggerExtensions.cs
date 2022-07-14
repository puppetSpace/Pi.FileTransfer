using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core;
public static class LoggerExtensions
{
    private static readonly Action<ILogger, string, Exception?> _searchReceipts;
    private static readonly Action<ILogger, string,int,int, Exception?> _amountOfSegmentFilesPresent;
    private static readonly Action<ILogger, string, Exception?> _buildingFile;
    private static readonly Action<ILogger, string, Exception?> _buildingFileFailed;
    private static readonly Action<ILogger, string,int,int, Exception?> _segmentingFile;
    private static readonly Action<ILogger, string,int,int, Exception?> _getSpecificSegment;
    private static readonly Action<ILogger, string,int, Exception?> _buildFileInTemp;
    private static readonly Action<ILogger, string, Exception?> _segmentFile;
    private static readonly Action<ILogger,string,Exception?> _clearLastPosition;
    private static readonly Action<ILogger,string,string,Exception?> _sendSegment;
    private static readonly Action<ILogger,string,string,Exception?> _sendSegmentFailed;
    private static readonly Action<ILogger, string, string, Exception?> _sendReceipt;
    private static readonly Action<ILogger, string, string, Exception?> _sendReceiptFailed;
    private static readonly Action<ILogger, string, Exception?> _indexingFolder;

    static LoggerExtensions()
    {
        _searchReceipts = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Searching for receipts in '{FolderPath}'");
        _buildingFile = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Building file {File}");
        _segmentFile = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Segmenting file {File}");
        _clearLastPosition = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Clearing last read position for '{File}'");
        _sendSegment = LoggerMessage.Define<string,string>(LogLevel.Information, new EventId(), "Sending segment of file '{File}' to '{Destination}'");
        _sendReceipt = LoggerMessage.Define<string,string>(LogLevel.Information, new EventId(), "Sending receipt of file '{File}' to '{Destination}'");
        _indexingFolder = LoggerMessage.Define<string>(LogLevel.Information, new EventId(), "Indexing folder '{Folder}'");

        _buildingFileFailed = LoggerMessage.Define<string>(LogLevel.Error, new EventId(), "Building file {File} failed.");
        _sendSegmentFailed = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Failed to send segment of file '{File}' to '{Destination}'");
        _sendReceiptFailed = LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(), "Failed to send receipt of file '{File}' to '{Destination}'");


        _amountOfSegmentFilesPresent = LoggerMessage.Define<string,int,int>(LogLevel.Debug, new EventId(), "'{File}' exists out of {ExpectedAmountOfSegments}. {ActualAmountOfSegments} segment files are present");
        _segmentingFile = LoggerMessage.Define<string, int, int>(LogLevel.Debug, new EventId(), "Segmenting file '{File}'. Segment: {SegmentCount}, BytesRead: {BytesRead}");
        _getSpecificSegment = LoggerMessage.Define<string, int, int>(LogLevel.Debug, new EventId(), "Getting segment for file '{File}'. Start: {Start}, End: {End}");
        _buildFileInTemp = LoggerMessage.Define<string, int>(LogLevel.Debug, new EventId(), "Building temp file '{File}' from {SegmentCount} segments");
    }

    public static void SearchReceiptInFolder(this ILogger logger, string folderPath) => _searchReceipts(logger, folderPath, null);

    public static void AmountOfSegmentFilesPresent(this ILogger logger, string file,int expectedAmountOfSegments, int ActualAmountOfSegments) 
        => _amountOfSegmentFilesPresent(logger,file,expectedAmountOfSegments, ActualAmountOfSegments, null);

    public static void BuildingFile(this ILogger logger, string file) => _buildingFile(logger, file, null);
    public static void BuildingFileFailed(this ILogger logger, string file,Exception exception) => _buildingFileFailed(logger, file, exception);

    public static void SegmentingFile(this ILogger logger, string file, int segmentCount, int bytesRead)
        => _segmentingFile(logger, file, segmentCount, bytesRead, null);

    public static void GetSpecificSegment(this ILogger logger, string file,int start,int end)
        => _getSpecificSegment(logger, file, start, end, null);

    public static void BuildFileInTemp(this ILogger logger, string file, int segmentCount)
        => _buildFileInTemp(logger, file, segmentCount, null);

    public static void SegmentFile(this ILogger logger, string file)
        => _segmentFile(logger, file, null);

    public static void ClearLastPosition(this ILogger logger, string file)
        => _clearLastPosition(logger, file, null);

    public static void SendSegment(this ILogger logger, string file,string destination)
        => _sendSegment(logger, file, destination, null);

    public static void SendSegmentFailed(this ILogger logger, string file,string destination,Exception exception)
        => _sendSegmentFailed(logger,file,destination, exception);

    public static void SendReceipt(this ILogger logger, string file, string destination)
    => _sendReceipt(logger, file, destination, null);

    public static void SendReceiptFailed(this ILogger logger, string file, string destination, Exception exception)
        => _sendReceiptFailed(logger, file, destination, exception);

    public static void IndexingFolder(this ILogger logger, string folder)
        => _indexingFolder(logger, folder, null);
}
