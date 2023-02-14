using Pi.FileTransfer.Core.Destinations;

namespace Pi.FileTransfer.Core.Transfers;
public record FailedSegment(Core.Files.File File,Destination Destination, int Sequencenumber, SegmentRange Range, bool IsFileUpdate);