using KayanHRAttendanceService.Application.Implementation.Services;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.ApiBased;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Application.Interfaces.Data;
using KayanHRAttendanceService.Application.Interfaces.Services;
using KayanHRAttendanceService.Application.Interfaces.Services.AttendanceConnectors;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Infrastructure.Data.ApplicationDbContext;
using KayanHRAttendanceService.Infrastructure.Data.Persistence.Sqlite;
using KayanHRAttendanceService.Infrastructure.Services;
using KayanHRAttendanceService.WindowsService.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KayanHRAttendanceService.WindowsService;

class Program
{
    public static void Main(string[] args)
    {
        try
        {
            ConfigureLogger();

            var host = CreateHostBuilder(args).Build();

            ApplyPendingMigrations(host);

            host.Run();
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
                var configs = context.Configuration;
                var typeID = configs.GetSection("Settings").Get<IntegrationSettings>().Type;

                config.SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData"));

                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                switch (typeID)
                {
                    case 1: config.AddJsonFile("appsettings.biostar.json", optional: false, reloadOnChange: true); break;
                    case 2: config.AddJsonFile("appsettings.biotime.json", optional: false, reloadOnChange: true); break;
                    case 3: case 4: case 5: config.AddJsonFile("appsettings.database.json", optional: false, reloadOnChange: true); break;
                    default:
                        throw new InvalidOperationException("Unsupported integration type");
                }

            })
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                services.Configure<IntegrationSettings>(config.GetSection("Settings"));
                var typeID = config.GetSection("Settings").Get<IntegrationSettings>().Type;

                var basePath = Path.Combine(AppContext.BaseDirectory, "AppData");
                var dbDirectory = Path.Combine(basePath, "Database");
                if (!Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=Database/attendance.db"));

                switch (typeID)
                {
                    case 1: services.AddSingleton<IAttendanceConnector, BioStarConnector>(); break;
                    case 2: services.AddSingleton<IAttendanceConnector, BioTimeConnector>(); break;
                    case 3: services.AddSingleton<IAttendanceConnector, MSSqlServerConnector>(); break;
                    case 4: services.AddSingleton<IAttendanceConnector, PostgreSqlConnector>(); break;
                    case 5: services.AddSingleton<IAttendanceConnector, MySQLConnector>(); break;
                    default:
                        throw new InvalidOperationException("Unsupported integration type");
                }

                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IAttendanceFetcherService, AttendanceFetcherService>();
                services.AddScoped<IDataPusherService, DataPusherService>();
                services.AddScoped<ISyncAttendanceData, SyncAttendanceData>();
                services.AddScoped<IHttpService, HttpService>();
                services.AddHostedService<AttendanceWorker>();
                services.AddHttpClient();
            });
    }

    private static void ApplyPendingMigrations(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (db.Database.GetPendingMigrations().Any())
        {
            Log.Information("Applying pending migrations...");
            db.Database.Migrate();
            Log.Information("Migrations applied successfully.");
        }
        else
        {
            Log.Information("No pending migrations found.");
        }
    }

    private static void ConfigureLogger()
    {
        var config = new ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData"))
            .AddJsonFile("loggerconfig.json", optional: false, reloadOnChange: true).Build();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config)
            .Enrich.FromLogContext().CreateLogger();
    }
}