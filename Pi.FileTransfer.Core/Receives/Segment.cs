namespace Pi.FileTransfer.Core.Receives;
public record Segment(int Sequencenumber, byte[] Buffer, Guid FileId, string FolderName);

