namespace Pi.FileTransfer.Core.Entities;
public record FailedReceipt(Guid FileId, int TotalAmountOfSegments, bool IsFileUpdate);
