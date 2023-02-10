namespace Pi.FileTransfer.Core.Transfers;
public record FailedSegment(Guid FileId, int Sequencenumber, SegmentRange Range, bool IsFileUpdate);