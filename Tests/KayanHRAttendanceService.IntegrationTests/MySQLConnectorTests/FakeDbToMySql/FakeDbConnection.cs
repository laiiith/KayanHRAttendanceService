using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

class FakeDbConnection : DbConnection
{
    private readonly DbCommand _command;

    public FakeDbConnection(DbCommand command) => _command = command;

    public override string ConnectionString { get; set; }
    public override string Database => "FakeDatabase";
    public override string DataSource => "FakeDataSource";
    public override string ServerVersion => "FakeVersion";
    public override ConnectionState State => ConnectionState.Open;

    public override void ChangeDatabase(string databaseName) { }
    public override void Close() { }
    public override void Open() { }
    public override Task OpenAsync(System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null;
    protected override DbCommand CreateDbCommand() => _command;
}
