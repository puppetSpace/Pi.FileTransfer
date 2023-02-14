using Pi.FileTransfer.Core.Destinations;

namespace Pi.FileTransfer.Core.Transfers;
public record FailedReceipt(Core.Files.File File, Destination Destination, int TotalAmountOfSegments, bool IsFileUpdate);
