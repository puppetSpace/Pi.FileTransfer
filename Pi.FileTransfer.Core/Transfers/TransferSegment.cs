namespace Pi.FileTransfer.Core.Transfers;
public record TransferSegment(int Sequencenumber, byte[] Buffer, Guid FileId, string FolderName);

