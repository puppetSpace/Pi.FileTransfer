namespace Pi.FileTransfer.Core.Entities;
public record TransferReceipt(Guid FileId, string RelativePath, int AmountOfSegments, string FolderName);
