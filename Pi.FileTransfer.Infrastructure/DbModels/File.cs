using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure.DbModels;
internal class File
{
    public string Folder { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Extension { get; set; }

    public string RelativePath { get; set; }

    public DateTime LastModified { get; set; }
}
