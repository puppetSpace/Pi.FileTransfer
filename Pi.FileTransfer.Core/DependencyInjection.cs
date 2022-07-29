using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.BackgroundServices;
using Pi.FileTransfer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Reflection;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core;
public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient<FileIndexer>();
        services.AddTransient<FileSegmentation>();
        services.AddTransient<TransferService>();
        services.AddTransient<DataStore>();
        services.AddTransient<DeltaService>();
        services.AddHttpClient(TransferService.HttpClientName, o =>
        {
            o.Timeout = TimeSpan.FromSeconds(5);
        });


        services.AddHostedService<IndexerService>();
        services.AddHostedService<RetryService>();
        services.AddHostedService<FileAssemblerService>();
    }
}
