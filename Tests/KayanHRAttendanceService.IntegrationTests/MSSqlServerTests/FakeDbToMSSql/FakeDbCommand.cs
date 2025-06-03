using KayanHRAttendanceService.Domain.Entities.Sqlite;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests.FakeDb
{
    public class FakeDbCommand : DbCommand
    {
        private readonly List<string> _executedCommands;
        private readonly FakeDbParameterCollection _parameters = new FakeDbParameterCollection();

        public FakeDbCommand(List<string> executedCommands)
        {
            _executedCommands = executedCommands;
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        public override void Cancel() { }

        public override int ExecuteNonQuery()
        {
            _executedCommands.Add(CommandText);
            return 1;
        }

        public override object ExecuteScalar()
        {
            _executedCommands.Add(CommandText);
            return 1;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var data = new List<AttendanceRecord>
            {
                new AttendanceRecord
                {
                    TId = "1",
                    EmployeeCode = "EMP001",
                    PunchTime = "2024-01-01T08:00:00Z",
                    Function = "IN",
                    MachineName = "DeviceA"
                }
            };

            return new FakeDbDataReader(data);
        }

        public override void Prepare() { }

        protected override DbParameter CreateDbParameter() => new FakeDbParameter();

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            _executedCommands.Add(CommandText);
            return Task.FromResult(1);
        }
    }
}
