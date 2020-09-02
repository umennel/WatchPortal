using System;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", true)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
               .AddEnvironmentVariables()
               .Build();
 
            builder.Services.AddSingleton<IConfiguration>(config);
  
            // Example to access secrets
            Configuration.MySecrets mySecretSettings = new Configuration.MySecrets();
            config.GetSection("MySecrets").Bind(mySecretSettings);

            Configuration.AuthSettings authSettings = new Configuration.AuthSettings();
            config.GetSection("Authentication").Bind(authSettings);    
        
            builder.Services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>(
                s => new AccessTokenProvider(authSettings.Audience, authSettings.Authority));
        }
    }
}