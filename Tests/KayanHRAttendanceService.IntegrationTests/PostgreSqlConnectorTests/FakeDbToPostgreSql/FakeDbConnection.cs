using KayanHRAttendanceService.Domain.Entities.Sqlite;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.PostgreSqlConnectorTests.FakeDbToPostgreSql
{
    public class FakeDbConnection : DbConnection
    {
        private readonly IEnumerable<AttendanceRecord> _data;
        private ConnectionState _state = ConnectionState.Closed;

        public FakeDbConnection(IEnumerable<AttendanceRecord> data)
        {
            _data = data;
        }

        public override string ConnectionString { get; set; }
        public override string Database => "FakeDb";
        public override string DataSource => "FakeDataSource";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() => _state = ConnectionState.Closed;
        public override void Open() => _state = ConnectionState.Open;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return new FakeDbCommand(_data, this);
        }

    }
}
