using KayanHRAttendanceService.Application.AttendanceConnector.Interfaces;
using KayanHRAttendanceService.Application.Common.Interfaces;
using KayanHRAttendanceService.Application.Services.Interfaces;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Infrastructure.AttendanceConnector.BioStar;
using KayanHRAttendanceService.Infrastructure.AttendanceConnector.BioTime;
using KayanHRAttendanceService.Infrastructure.AttendanceConnector.Databases;
using KayanHRAttendanceService.Infrastructure.Repository;
using KayanHRAttendanceService.Infrastructure.Services;
using KayanHRAttendanceService.WindowsService.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KayanHRAttendanceService.WindowsService
{
    class Program
    {
        // Example workflow
        // 1. Retrieve data from device and push to RabbitMQ
        // 2. Save to local SQLite
        // 3. Read from SQLite
        // 4. Push to DeveloperAPI via RabbitMQ
        public static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(new ConfigurationBuilder()
                        .SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData"))
                        .AddJsonFile("loggerconfig.json", optional: false, reloadOnChange: true)
                        .Build())
                    .Enrich.FromLogContext()
                    .CreateLogger();

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData"));
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    var config = context.Configuration;

                    services.Configure<IntegrationSettings>(config.GetSection("Integration"));

                    var integrationSettings = config.GetSection("Integration").Get<IntegrationSettings>();

                    switch (integrationSettings.Type)
                    {
                        case 1: services.AddSingleton<IAttendanceConnector, BioStarConnector>(); break;
                        case 2: services.AddSingleton<IAttendanceConnector, BioTimeConnector>(); break;
                        case 3: services.AddSingleton<IAttendanceConnector, MSSqlServerConnector>(); break;
                        case 4: services.AddSingleton<IAttendanceConnector, PostgreSqlConnector>(); break;
                        case 5: services.AddSingleton<IAttendanceConnector, MySQLConnector>(); break;
                        default:
                            throw new InvalidOperationException("Unsupported integration type");
                    }

                    services.AddSingleton<IAttendanceFetcherService, AttendanceFetcherService>();
                    services.AddSingleton<IHttpService, HttpService>();
                    services.AddHttpClient();
                    services.AddHostedService<AttendanceWorker>();
                });
        }
    }
}
