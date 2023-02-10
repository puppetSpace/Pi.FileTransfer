namespace Pi.FileTransfer.Core.Transfers;
public record SegmentRange(int Start, int End)
{
    public int Length => End - Start;
}
