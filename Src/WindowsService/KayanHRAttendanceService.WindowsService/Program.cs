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
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace KayanHRAttendanceService.WindowsService;

class Program
{
    #region Data
    private static readonly string AppDataPath = Path.Combine(AppContext.BaseDirectory, "AppData");
    private static readonly string DefaultLinuxDbFolder = "/var/lib/kayanhrattendanceservice/database";
    private static readonly string WindowsDbFolder = Path.Combine(AppContext.BaseDirectory, "Database");
    private static readonly string DatabasePath = GetDatabasePath();
    private static readonly string DatabaseFile = Path.Combine(DatabasePath, "attendance.db");
    #endregion Data

    public static async Task Main(string[] args)
    {
        ConfigureLogger();
        try
        {
            var host = CreateHostBuilder(args).Build();

            ApplyPendingMigrations(host);

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
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
            .ConfigureAppConfiguration(ConfigureAppSettings)
            .ConfigureServices(ConfigureServices)
            .UsePlatformSpecificHosting();
    }

    private static void ConfigureAppSettings(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.SetBasePath(AppDataPath);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var tempConfig = config.Build();
        int typeID = tempConfig.GetSection("Settings").Get<IntegrationSettings>()?.Type ?? 0;

        string integrationSettingsFile = typeID switch
        {
            1 => "appsettings.biostar.json",
            2 => "appsettings.biotime.json",
            3 or 4 or 5 => "appsettings.database.json",
            6 => "appsettings.keytech.json",
            _ => throw new InvalidOperationException("Unsupported integration type")
        };

        config.AddJsonFile(integrationSettingsFile, optional: false, reloadOnChange: true);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var config = context.Configuration;
        var integrationSettings = config.GetSection("Settings").Get<IntegrationSettings>();
        int typeID = integrationSettings?.Type ?? 0;

        EnsureDatabaseFolderExists();

        services.Configure<IntegrationSettings>(config.GetSection("Settings"));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={DatabaseFile}"));

        RegisterAttendanceConnector(services, typeID);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAttendanceFetcherService, AttendanceFetcherService>();
        services.AddScoped<IDataPusherService, DataPusherService>();
        services.AddScoped<ISyncAttendanceData, SyncAttendanceData>();
        services.AddScoped<IKayanConnectorService, KayanConnectorService>();
        services.AddScoped<IHttpService, HttpService>();

        services.AddHostedService<AttendanceWorker>();
        services.AddHttpClient();
    }

    private static void RegisterAttendanceConnector(IServiceCollection services, int typeID)
    {
        switch (typeID)
        {
            case 1: services.AddScoped<IAttendanceConnector, BioStarConnector>(); break;
            case 2: services.AddScoped<IAttendanceConnector, BioTimeConnector>(); break;
            case 3: services.AddScoped<IAttendanceConnector, MSSqlServerConnector>(); break;
            case 4: services.AddScoped<IAttendanceConnector, PostgreSqlConnector>(); break;
            case 5: services.AddScoped<IAttendanceConnector, MySQLConnector>(); break;
            case 6: services.AddScoped<IAttendanceConnector, KeyTechConnector>(); break;
            default:
                throw new InvalidOperationException("Unsupported integration type");
        }
    }

    private static void EnsureDatabaseFolderExists()
    {
        if (!Directory.Exists(DatabasePath))
        {
            Directory.CreateDirectory(DatabasePath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var directoryInfo = new DirectoryInfo(DatabasePath);
                var directorySecurity = directoryInfo.GetAccessControl();

                var currentUser = WindowsIdentity.GetCurrent().User;
                if (currentUser != null)
                {
                    directorySecurity.AddAccessRule(new FileSystemAccessRule(
                        currentUser,
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));

                    directoryInfo.SetAccessControl(directorySecurity);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var dirInfo = new System.IO.DirectoryInfo(DatabasePath);
                dirInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;
            }
            else
            {
                throw new PlatformNotSupportedException("This service can only run on Windows or Linux.");
            }
        }
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

    private static string GetDatabasePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsDbFolder;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return DefaultLinuxDbFolder;
        }

        throw new PlatformNotSupportedException("This service can only run on Windows or Linux.");
    }
}

static class HostBuilderExtensions
{
    public static IHostBuilder UsePlatformSpecificHosting(this IHostBuilder builder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return builder.UseWindowsService();
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return builder.UseSystemd();
        }
        throw new PlatformNotSupportedException("This service can only run on Windows or Linux.");
    }
}