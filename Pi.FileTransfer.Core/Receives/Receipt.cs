namespace Pi.FileTransfer.Core.Receives;
public record Receipt(Guid FileId, string Folder,string RelativePath, int AmountOfSegments,int Version, bool IsFileUpdate);
