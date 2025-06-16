using System.Data;
using System.Data.Common;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

public class FakeDbConnection : DbConnection
{
    private readonly IEnumerable<AttendanceRecord> _mockData;

    public FakeDbConnection(IEnumerable<AttendanceRecord> mockData)
    {
        _mockData = mockData;
    }

    public override string ConnectionString { get; set; }
    public override string Database => "FakeDb";
    public override string DataSource => "FakeDataSource";
    public override string ServerVersion => "FakeVersion";
    public override ConnectionState State => ConnectionState.Open;

    public override void ChangeDatabase(string databaseName) { }
    public override void Close() { }
    public override void Open() { }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        => throw new NotImplementedException();

    protected override DbCommand CreateDbCommand()
        => new FakeDbCommand(_mockData);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
