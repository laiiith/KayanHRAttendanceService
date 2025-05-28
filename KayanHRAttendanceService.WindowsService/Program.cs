using KayanHRAttendanceService.WindowsService.AttendanceType;
using KayanHRAttendanceService.WindowsService.AttendanceType.IAttendanceType;
using KayanHRAttendanceService.WindowsService.Entities;
using KayanHRAttendanceService.WindowsService.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KayanHRAttendanceService.WindowsService;

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
            Log.Logger = new LoggerConfiguration().ReadFrom.
                Configuration(new ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData")).
                AddJsonFile("loggerconfig.json", optional: false, reloadOnChange: true).Build()).
                Enrich.FromLogContext().CreateLogger();

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
        return Host.CreateDefaultBuilder(args).UseSerilog().ConfigureAppConfiguration((context, config) =>
              {
                  config.SetBasePath(Path.Combine(AppContext.BaseDirectory, "AppData"));
                  config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              })
              .ConfigureServices((context, services) =>
              {
                  var config = context.Configuration;
                  services.Configure<IntegrationSettings>(config.GetSection("Integration"));

                  var integrationSettings = config.GetSection("Integration").Get<IntegrationSettings>();

                  switch (integrationSettings.Type)
                  {
                      case 1: services.AddSingleton<IAttendanceType, BioStar>(); break;
                      case 2: services.AddSingleton<IAttendanceType, BioTime>(); break;
                      case 3: services.AddSingleton<IAttendanceType, MSSqlServer>(); break;
                      case 4: services.AddSingleton<IAttendanceType, PostgreSQL>(); break;
                      case 5: services.AddSingleton<IAttendanceType, MySQL>(); break;
                      default:
                          throw new InvalidOperationException("Unsupported integration type");
                  }
                  services.AddHostedService<AttendanceWorker>();
              });

        throw new NotImplementedException();

    }
}