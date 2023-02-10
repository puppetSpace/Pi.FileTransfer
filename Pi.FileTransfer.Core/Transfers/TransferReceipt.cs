namespace Pi.FileTransfer.Core.Transfers;
public record TransferReceipt(Guid FileId, string RelativePath, int AmountOfSegments, string FolderName, bool IsFileUpdate);
