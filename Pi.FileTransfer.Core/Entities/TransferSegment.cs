namespace Pi.FileTransfer.Core.Entities;
public record TransferSegment(int Sequencenumber, byte[] Buffer, Guid FileId, string FolderName);

