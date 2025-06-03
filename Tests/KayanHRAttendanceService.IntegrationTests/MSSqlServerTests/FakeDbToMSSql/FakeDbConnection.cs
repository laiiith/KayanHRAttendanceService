using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests.FakeDb
{
    public class FakeDbConnection : DbConnection
    {
        private readonly List<string> _executedCommands = new List<string>();

        public override string ConnectionString { get; set; } = "Server=fake;Database=fake;Trusted_Connection=True;";
        public override string Database => "fake";
        public override string DataSource => "fake";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
            new FakeDbTransaction(this);

        protected override DbCommand CreateDbCommand() => new FakeDbCommand(_executedCommands);

        public List<string> ExecutedCommands => _executedCommands;
    }

}
