using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests;

public class TestMySQLConnector : MySQLConnector
{
    private readonly DbCommand _command;
    private readonly DbConnection _connection;

    public TestMySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger, DbCommand command, DbConnection connection = null)
        : base(settings, logger)
    {
        _command = command;
        _connection = connection;
    }

    protected override Task<DbConnection> CreateDbConnection()
    {
        if (_connection != null)
            return Task.FromResult(_connection);
        else
            return Task.FromResult<DbConnection>(new FakeDbConnection(_command));
    }

    public async Task<List<AttendanceRecord>> FetchAttendanceDataFromSqlAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is not initialized");

        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM AttendanceRecords";
        command.CommandType = System.Data.CommandType.Text;

        var records = new List<AttendanceRecord>();
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                records.Add(new AttendanceRecord
                {
                    EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    PunchTime = reader.GetString(reader.GetOrdinal("PunchTime")),
                    Function = reader.GetString(reader.GetOrdinal("Function")),
                    MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                    MachineSerialNo = reader.GetString(reader.GetOrdinal("MachineSerialNo")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    TId = reader.GetString(reader.GetOrdinal("TId")),
                });
            }
        }
        return records;
    }
}
class FakeDbConnection : DbConnection
{
    private readonly DbCommand _command;

    public FakeDbConnection()
    {

    }

    public FakeDbConnection(DbCommand command)
    {
        _command = command;
    }

    public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override string Database => throw new NotImplementedException();

    public override string DataSource => throw new NotImplementedException();

    public override string ServerVersion => throw new NotImplementedException();

    public override ConnectionState State => throw new NotImplementedException();

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }

    public override void Close()
    {
        throw new NotImplementedException();
    }

    public override void Open()
    {
        throw new NotImplementedException();
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        if (_command != null)
            return _command;

        throw new InvalidOperationException("No command provided.");
    }

}
