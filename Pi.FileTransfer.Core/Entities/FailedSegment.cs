namespace Pi.FileTransfer.Core.Entities;
public record FailedSegment(Guid FileId, int Sequencenumber, SegmentRange Range, bool IsFileUpdate);