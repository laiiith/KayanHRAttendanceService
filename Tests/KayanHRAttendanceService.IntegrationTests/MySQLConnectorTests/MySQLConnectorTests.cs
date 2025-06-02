using System.Collections;
using System.Data;
using System.Data.Common;
using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Tests
{
    public class MySQLConnectorUnitTests
    {
        class TestMySQLConnector : MySQLConnector
        {
            private readonly DbCommand _command;

            public TestMySQLConnector(IOptions<IntegrationSettings> settings, ILogger<MySQLConnector> logger, DbCommand command)
                : base(settings, logger)
            {
                _command = command;
            }

            protected override Task<DbConnection> CreateDbConnection()
            {
                return Task.FromResult<DbConnection>(new FakeDbConnection(_command));
            }
        }

        [Fact]
        public async Task FetchAttendanceDataAsync_ShouldReturnMappedRecords()
        {
            var settings = Options.Create(new IntegrationSettings
            {
                GetDataProcedure = "sp_get_attendance_data",
                UpdateProcedure = "sp_update_flag",
                ConnectionString = "FakeConnectionString"
            });

            var logger = new LoggerFactory().CreateLogger<MySQLConnector>();

            var fakeDataReader = new FakeDbDataReader();
            var fakeCommand = new FakeDbCommand(fakeDataReader)
            {
                CommandText = "sp_get_attendance_data",
                CommandType = CommandType.StoredProcedure
            };

            var connector = new TestMySQLConnector(settings, logger, fakeCommand);

            var result = await connector.FetchAttendanceDataAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("E001", result[0].EmployeeCode);
            Assert.Equal("E002", result[1].EmployeeCode);
        }

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

            // ❗ Hide the base method instead of overriding
            public new Task<DbDataReader> ExecuteReaderAsync(System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_reader);
            }
        }

        class FakeDbDataReader : DbDataReader
        {
            private int _index = -1;
            private readonly List<Dictionary<string, object>> _rows = new()
{
    new Dictionary<string, object>
    {
        { "EmployeeCode", "E001" },
        { "PunchTime", DateTime.Parse("2025-06-02T12:00:00") },
        { "Function", "IN" },
        { "MachineName", "M1" },
        { "MachineSerialNo", "S1" },
        { "Status", "OK" },
        { "TId", 1 }
    },
    new Dictionary<string, object>
    {
        { "EmployeeCode", "E002" },
        { "PunchTime", DateTime.Parse("2025-06-02T12:10:00") },
        { "Function", "OUT" },
        { "MachineName", "M2" },
        { "MachineSerialNo", "S2" },
        { "Status", "OK" },
        { "TId", 2 }
    }
};


            public override bool Read() => ++_index < _rows.Count;
            public override Task<bool> ReadAsync(System.Threading.CancellationToken cancellationToken) => Task.FromResult(Read());

            public override string GetString(int ordinal)
            {
                var key = GetName(ordinal);
                var value = _rows[_index][key];

                if (value is string s)
                    return s;

                if (value is DateTime dt)
                    return dt.ToString("yyyy-MM-dd HH:mm:ss");

                return value?.ToString() ?? string.Empty;
            }




            public override int GetOrdinal(string name)
            {
                return name switch
                {
                    "EmployeeCode" => 0,
                    "PunchTime" => 1,
                    "Function" => 2,
                    "MachineName" => 3,
                    "MachineSerialNo" => 4,
                    "Status" => 5,
                    "TId" => 6,
                    _ => throw new IndexOutOfRangeException(name)
                };
            }

            public override string GetName(int ordinal)
            {
                return ordinal switch
                {
                    0 => "EmployeeCode",
                    1 => "PunchTime",
                    2 => "Function",
                    3 => "MachineName",
                    4 => "MachineSerialNo",
                    5 => "Status",
                    6 => "TId",
                    _ => throw new IndexOutOfRangeException(nameof(ordinal))
                };
            }

            public override object GetValue(int ordinal)
            {
                var key = GetName(ordinal);
                var value = _rows[_index][key];

                return key switch
                {
                    "PunchTime" => value,
                    "TId" => value,
                    _ => value
                };


            }


            public override DateTime GetDateTime(int ordinal)
            {
                var value = GetValue(ordinal);
                return value is DateTime dt ? dt : DateTime.Parse(value.ToString());
            }

            public override int GetInt32(int ordinal)
            {
                return int.Parse(GetString(ordinal));
            }

            public override bool HasRows => true;
            public override int FieldCount => 7;
            public override bool IsDBNull(int ordinal) => false;
            public override IEnumerator<object> GetEnumerator() => throw new NotImplementedException();

            #region NotImplemented
            public override int Depth => throw new NotImplementedException();
            public override bool IsClosed => false;
            public override int RecordsAffected => throw new NotImplementedException();
            public override object this[string name] => _rows[_index][name];
            public override object this[int ordinal] => GetValue(ordinal);

            public override bool NextResult() => false;
            public override bool GetBoolean(int ordinal) => throw new NotImplementedException();
            public override byte GetByte(int ordinal) => throw new NotImplementedException();
            public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
            public override char GetChar(int ordinal) => throw new NotImplementedException();
            public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
            public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();
            public override decimal GetDecimal(int ordinal) => throw new NotImplementedException();
            public override double GetDouble(int ordinal) => throw new NotImplementedException();
            public override Type GetFieldType(int ordinal)
            {
                return ordinal switch
                {
                    0 => typeof(string), // EmployeeCode
                    1 => typeof(DateTime), // PunchTime
                    2 => typeof(string), // Function
                    3 => typeof(string), // MachineName
                    4 => typeof(string), // MachineSerialNo
                    5 => typeof(string), // Status
                    6 => typeof(int), // TId
                    _ => typeof(string)
                };
            }

            public override float GetFloat(int ordinal) => throw new NotImplementedException();
            public override Guid GetGuid(int ordinal) => throw new NotImplementedException();
            public override short GetInt16(int ordinal) => throw new NotImplementedException();
            public override long GetInt64(int ordinal) => throw new NotImplementedException();
            public override int GetValues(object[] values) => throw new NotImplementedException();
            #endregion
        }

        class FakeDbParameterCollection : DbParameterCollection
        {
            private readonly List<DbParameter> _parameters = new();

            public override int Count => _parameters.Count;
            public override object SyncRoot => new object();
            public override int Add(object value) { _parameters.Add((DbParameter)value); return _parameters.Count - 1; }
            public override void AddRange(Array values) => throw new NotImplementedException();
            public override void Clear() => _parameters.Clear();
            public override bool Contains(string value) => throw new NotImplementedException();
            public override bool Contains(object value) => _parameters.Contains((DbParameter)value);
            public override void CopyTo(Array array, int index) => throw new NotImplementedException();
            public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();
            public override int IndexOf(string parameterName) => throw new NotImplementedException();
            public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);
            public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);
            public override void Remove(object value) => _parameters.Remove((DbParameter)value);
            public override void RemoveAt(string parameterName) => throw new NotImplementedException();
            public override void RemoveAt(int index) => _parameters.RemoveAt(index);
            protected override DbParameter GetParameter(string parameterName) => throw new NotImplementedException();
            protected override DbParameter GetParameter(int index) => (DbParameter)_parameters[index];
            protected override void SetParameter(string parameterName, DbParameter value) => throw new NotImplementedException();
            protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
        }
    }
}
