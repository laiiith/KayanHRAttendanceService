using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

public class FakeDbCommand : DbCommand
{
    private readonly DbDataReader _reader;

    public FakeDbCommand(DbDataReader reader)
    {
        _reader = reader;
    }

    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection => new FakeDbParameterCollection();
    protected override DbTransaction DbTransaction { get; set; }

    public override bool DesignTimeVisible { get; set; }

    public override void Cancel() => throw new NotImplementedException();

    public override int ExecuteNonQuery() => throw new NotImplementedException();

    public override object ExecuteScalar() => throw new NotImplementedException();

    public override void Prepare() => throw new NotImplementedException();

    protected override DbParameter CreateDbParameter() => throw new NotImplementedException();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _reader;
}