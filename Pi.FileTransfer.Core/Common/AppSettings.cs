using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Common;
public class AppSettings
{
    public string BasePath { get; set; }
}


public class AppSettingsValidation : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string name, AppSettings options)
    {
        if (options is null || string.IsNullOrWhiteSpace(options.BasePath) || !Directory.Exists(options.BasePath))
            return ValidateOptionsResult.Fail("Basepath must be provided in appsettings and should be an existing directory");
        else
            return ValidateOptionsResult.Success;
    }
}
