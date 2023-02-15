namespace Pi.FileTransfer.Core.Receives;
public record Receipt(Guid FileId, string Folder,string RelativePath, int AmountOfSegments, bool IsFileUpdate);
