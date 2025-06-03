//using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
//using KayanHRAttendanceService.Domain.Entities.General;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System.Data;
//using System.Data.Common;

//namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests;

//public class MySQLConnectorUnitTests
//{
//    class TestMySQLConnector : MySQLConnector
//    {
//        private readonly DbCommand _command;

//        public TestMySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger, DbCommand command)
//            : base(settings, logger)
//        {
//            _command = command;
//        }

//        protected override Task<DbConnection> CreateDbConnection()
//        {
//            return Task.FromResult<DbConnection>(new FakeDbConnection(_command));
//        }
//    }

//    [Fact]
//    public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords()
//    {
//        var settings = Options.Create(new IntegrationSettings
//        {
//            GetDataProcedure = "sp_get_attendance_data",
//            UpdateProcedure = "sp_update_flag",
//            ConnectionString = "FakeConnectionString"
//        });

//        var logger = new LoggerFactory().CreateLogger<MySQLConnector>();

//        var fakeDataReader = new KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql.FakeDbDataReader();
//        var fakeCommand = new KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql.FakeDbCommand(fakeDataReader)
//        {
//            CommandText = "sp_get_attendance_data",
//            CommandType = CommandType.StoredProcedure
//        };

//        var connector = new TestMySQLConnector(settings, logger, fakeCommand);

//        var result = await connector.FetchAttendanceDataAsync();

//        Assert.NotNull(result);
//        Assert.Equal(2, result.Count);
//        Assert.Equal("E001", result[0].EmployeeCode);
//        Assert.Equal("E002", result[1].EmployeeCode);
//    }
//}
