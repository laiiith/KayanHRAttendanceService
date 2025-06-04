using KayanHRAttendanceService.Domain.Entities.Sqlite;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.PostgreSqlConnectorTests.FakeDbToPostgreSql
{
    public class FakeDbCommand : DbCommand
    {
        private readonly IEnumerable<AttendanceRecord> _data;
        private DbConnection? connection;

        public FakeDbCommand(IEnumerable<AttendanceRecord> data, FakeDbConnection fakeDbConnection)
        {
            _data = data;
            DbConnection = connection;
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }

        public override void Cancel()
        { }

        public override int ExecuteNonQuery() => 0;

        public override object ExecuteScalar() => null;

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return new FakeDbDataReader(_data);
        }

        protected override DbParameter CreateDbParameter() => new FakeDbParameter();

        public override void Prepare()
        { }
    }
}