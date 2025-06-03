using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

class FakeDbCommand : DbCommand
{
    private readonly DbDataReader _reader;

    public FakeDbCommand(DbDataReader reader) => _reader = reader;

    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection => new FakeDbParameterCollection();
    protected override DbTransaction DbTransaction { get; set; }

    public override void Cancel() { }
    public override int ExecuteNonQuery() => 0;
    public override object ExecuteScalar() => null;
    public override void Prepare() { }

    protected override DbParameter CreateDbParameter() => null;

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _reader;

    public new Task<DbDataReader> ExecuteReaderAsync(System.Threading.CancellationToken cancellationToken)
    {
        return Task.FromResult(_reader);
    }
}
