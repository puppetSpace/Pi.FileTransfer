using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.BackgroundServices;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;
using System.Reflection;

namespace Pi.FileTransfer.Core;
public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient<FileSegmentation>();
        services.AddTransient<DeltaSegmentation>();
        services.AddTransient<TransferService>();
        services.AddTransient<DeltaService>();
        services.AddTransient<DataStore>();
        services.AddSingleton<FolderState>(x =>
        {
            var folderRepository = x.GetRequiredService<IFolderRepository>();
            return new FolderState( folderRepository.GetFolders().GetAwaiter().GetResult());
        });
        services.AddHttpClient(TransferService.HttpClientName, o =>
        {
            o.Timeout = TimeSpan.FromSeconds(5);
        });


        services.AddHostedService<IndexerService>();
        services.AddHostedService<RetryService>();
        services.AddHostedService<FileAssemblerService>();
    }
}
