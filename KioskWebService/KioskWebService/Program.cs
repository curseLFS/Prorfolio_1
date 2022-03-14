using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using System;
using System.IO;
using System.Security.Authentication;

namespace KioskWebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Application Starting Up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start correctly.");
            }
            finally 
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()                
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //***Config for Linux Server

                    //webBuilder.ConfigureKestrel(serverOptions =>
                    //{
                    //    serverOptions.ConfigureHttpsDefaults(listenOptions =>
                    //    {
                    //        listenOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
                    //        listenOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    //    });
                    //});
                    // **********************
                    webBuilder.UseStartup<Startup>().UseUrls("http://localhost:2660");

                    // Config for Linux Server
                   //webBuilder.UseKestrel();
                });
        }
}
}
