namespace Pi.FileTransfer.Core.Transfers;
public record FailedReceipt(Guid FileId, int TotalAmountOfSegments, bool IsFileUpdate);
