using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Exceptions;
public class DestinationException : Exception
{
    public DestinationException()
    {
    }

    public DestinationException(string? message) : base(message)
    {
    }

    public DestinationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DestinationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
