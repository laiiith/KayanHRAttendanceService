using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

public class FakeDbConnection : DbConnection
{
    private readonly DbCommand _command;

    public FakeDbConnection(DbCommand command)
    {
        _command = command;
    }

    public override string ConnectionString { get; set; }
    public override string Database => "FakeDatabase";
    public override string DataSource => "FakeSource";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => ConnectionState.Open;

    public override Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override void Close() { }
    public override void Open() { }
    public override void ChangeDatabase(string databaseName) { }
    protected override DbCommand CreateDbCommand() => _command;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }
}
