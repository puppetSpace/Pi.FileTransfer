using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Common;
public class EntityBase
{
    public List<INotification> Events { get; set; } = new();


    public void ClearEvents() => Events.Clear();
}
