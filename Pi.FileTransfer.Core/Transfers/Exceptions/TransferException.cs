using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Transfers.Exceptions;
public class TransferException : Exception
{
    public TransferException()
    {
    }

    public TransferException(string? message) : base(message)
    {
    }

    public TransferException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected TransferException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
