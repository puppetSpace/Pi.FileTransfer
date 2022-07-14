using Microsoft.Extensions.DependencyInjection;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IFolderRepository,FolderRepository>();
        services.AddTransient<IFileSystem,FileSystem>();
    }
}
