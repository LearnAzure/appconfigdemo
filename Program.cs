using System;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace AppConfigDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.ConfigureAppConfiguration(config => {
                        var settings = config.Build();
                        var connection = settings.GetConnectionString("AppConfiguration");
                        config.AddAzureAppConfiguration(options => 
                            options.Connect(connection)
                                   .Select(KeyFilter.Any, LabelFilter.Null)
                                   .Select(KeyFilter.Any, settings.GetValue<string>("Environment"))
                                   .ConfigureKeyVault(keyVaultOptions => {
                                       keyVaultOptions.SetCredential(new ClientSecretCredential(
                                           settings.GetValue<string>("KeyVault:TenantId"),
                                           settings.GetValue<string>("KeyVault:ClientId"),
                                           settings.GetValue<string>("KeyVault:ClientSecret")
                                       ));
                                   })
                                   .ConfigureRefresh(refreshOptions => {
                                        refreshOptions.Register(key: "AppConfigRefresh:AlwaysChanging", label: LabelFilter.Null, refreshAll: false)
                                            .SetCacheExpiration(new TimeSpan(0, 0, 5));
                                   })
                        );                                   
                    })
                    .UseStartup<Startup>());
    }
}
