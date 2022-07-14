namespace Pi.FileTransfer.Core.Entities;
public record SegmentRange(int Start, int End)
{
    public int Length => End - Start;
}
