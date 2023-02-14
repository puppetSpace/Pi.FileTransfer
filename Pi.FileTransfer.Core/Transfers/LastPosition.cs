using Pi.FileTransfer.Core.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Transfers;
public record LastPosition
{

    public LastPosition(Core.Files.File file, Destination destination, int readBytes)
    {
        File = file;
        Destination = destination;
        ReadBytes = readBytes;
    }

    public int ReadBytes { get; set; }
    public Files.File File { get; }
    public Destination Destination { get; }
}
