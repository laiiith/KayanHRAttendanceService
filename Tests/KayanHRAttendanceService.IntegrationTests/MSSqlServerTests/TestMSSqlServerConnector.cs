using System.Data.Common;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests
{
    public class TestMSSqlServerConnector : MSSqlServerConnector
    {
        private readonly DbConnection testConnection;

        public TestMSSqlServerConnector(IOptions<IntegrationSettings> settings, ILogger<MSSqlServerConnector> logger, DbConnection connection)
            : base(settings, logger)
        {
            testConnection = connection;
        }

        protected override Task<DbConnection> CreateDbConnection() => Task.FromResult(testConnection);
    }
}
