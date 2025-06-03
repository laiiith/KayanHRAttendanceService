using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests.FakeDb
{
    public class FakeDbTransaction : DbTransaction
    {
        private readonly FakeDbConnection _connection;

        public FakeDbTransaction(FakeDbConnection connection)
        {
            _connection = connection;
        }

        public override void Commit()
        { }

        public override void Rollback()
        { }

        protected override DbConnection DbConnection => _connection;
        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
    }
}