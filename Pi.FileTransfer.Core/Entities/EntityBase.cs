using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Entities;
public class EntityBase
{
    public List<IRequest<Unit>> Events { get; set; } = new();
}
