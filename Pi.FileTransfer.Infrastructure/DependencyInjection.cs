using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
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

        services.AddDbContext<FileContext>((provider, options) =>
            {
                var appsettings = provider.GetService<IOptions<AppSettings>>();
                var dataDir = System.IO.Directory.CreateDirectory(Path.Combine(appsettings.Value.BasePath, ".data"));

                options.UseSqlite($"DataSource={System.IO.Path.Combine(dataDir.FullName,"Local.db")}");
            }, ServiceLifetime.Transient);

        services.AddTransient<IFolderRepository, FolderRepository>();
        services.AddTransient<IFileTransferRepository, FileTransferRepository>();
        services.AddTransient<IFileReceiveRepository, FileReceiveRepository>();
        services.AddTransient<IFileSystem, FileSystem>();
    }


    public static void AddMigration(this IHost host)
    {
        using var dbContext = host.Services.GetRequiredService<FileContext>();
        dbContext.Database.Migrate();
    }
}
