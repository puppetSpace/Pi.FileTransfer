using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure.DbModels;
internal class Destination
{
    public string Folder { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }
}
