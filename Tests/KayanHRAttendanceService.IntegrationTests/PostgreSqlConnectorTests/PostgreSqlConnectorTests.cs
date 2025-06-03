using KayanHRAttendanceService.Application.Implementation.Services.AttendanceConnectors.Databases;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Tests
{
    public class PostgreSqlConnectorTests
    {
        private readonly Mock<IOptions<IntegrationSettings>> _mockOptions;
        private readonly Mock<ILogger<PostgreSqlConnector>> _mockLogger;
        private readonly TestPostgreSqlConnector _connector;

        public PostgreSqlConnectorTests()
        {
            _mockOptions = new Mock<IOptions<IntegrationSettings>>();
            _mockLogger = new Mock<ILogger<PostgreSqlConnector>>();

            var integrationSettings = new IntegrationSettings
            {
                ConnectionString = "Host=localhost;Username=test;Password=test;Database=testdb",
                GetDataProcedure = "GetAttendanceData",
                UpdateProcedure = "UpdateAttendanceData"
            };

            _mockOptions.Setup(x => x.Value).Returns(integrationSettings);

            _connector = new TestPostgreSqlConnector(_mockOptions.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchAttendanceDataAsync_ReturnsData()
        {
            var expectedData = new List<AttendanceRecord>
            {
                new AttendanceRecord { TId = "1" },
                new AttendanceRecord { TId = "2" }
            };

            _connector.SetQueryResult(expectedData);

            var result = await _connector.FetchAttendanceDataAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result[0].TId);
            Assert.Equal("2", result[1].TId);
        }

        private class TestPostgreSqlConnector : PostgreSqlConnector
        {
            private IEnumerable<AttendanceRecord> _mockQueryResult = new List<AttendanceRecord>();

            public TestPostgreSqlConnector(IOptions<IntegrationSettings> settings, ILogger<PostgreSqlConnector> logger)
                : base(settings, logger)
            {
            }

            public void SetQueryResult(IEnumerable<AttendanceRecord> data)
            {
                _mockQueryResult = data;
            }

            protected override Task<DbConnection> CreateDbConnection()
            {
                return Task.FromResult<DbConnection>(new FakeDbConnection(_mockQueryResult));
            }
        }

        private class FakeDbConnection : DbConnection
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

            public override void ChangeDatabase(string databaseName)
            { }

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

        private class FakeDbCommand : DbCommand
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

        private class FakeDbParameterCollection : DbParameterCollection
        {
            private readonly List<DbParameter> _parameters = new();

            public override int Count => _parameters.Count;
            public override object SyncRoot => new object();

            public override int Add(object value)
            {
                _parameters.Add((DbParameter)value);
                return _parameters.Count - 1;
            }

            public override void AddRange(Array values)
            {
                foreach (var val in values)
                    _parameters.Add((DbParameter)val);
            }

            public override void Clear() => _parameters.Clear();

            public override bool Contains(object value) => _parameters.Contains((DbParameter)value);

            public override bool Contains(string value) => _parameters.Exists(p => p.ParameterName == value);

            public override void CopyTo(Array array, int index) => _parameters.ToArray().CopyTo(array, index);

            public override System.Collections.IEnumerator GetEnumerator() => _parameters.GetEnumerator();

            public override int IndexOf(object value) => _parameters.IndexOf((DbParameter)value);

            public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);

            public override void Insert(int index, object value) => _parameters.Insert(index, (DbParameter)value);

            public override void Remove(object value) => _parameters.Remove((DbParameter)value);

            public override void RemoveAt(int index) => _parameters.RemoveAt(index);

            public override void RemoveAt(string parameterName)
            {
                var index = _parameters.FindIndex(p => p.ParameterName == parameterName);
                if (index >= 0)
                    _parameters.RemoveAt(index);
            }

            protected override DbParameter GetParameter(int index) => _parameters[index];

            protected override DbParameter GetParameter(string parameterName) => _parameters.Find(p => p.ParameterName == parameterName);

            protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;

            protected override void SetParameter(string parameterName, DbParameter value)
            {
                var index = _parameters.FindIndex(p => p.ParameterName == parameterName);
                if (index >= 0)
                    _parameters[index] = value;
            }
        }

        private class FakeDbParameter : DbParameter
        {
            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override string SourceColumn { get; set; }
            public override object Value { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override int Size { get; set; }

            public override void ResetDbType()
            { }
        }

        private class FakeDbDataReader : DbDataReader
        {
            private readonly IEnumerator<AttendanceRecord> _enumerator;
            private bool _isClosed = false;

            public FakeDbDataReader(IEnumerable<AttendanceRecord> data)
            {
                _enumerator = data.GetEnumerator();
            }

            public override int FieldCount => 1;

            public override object this[int ordinal]
            {
                get
                {
                    if (ordinal == 0)
                        return _enumerator.Current.TId;
                    throw new IndexOutOfRangeException();
                }
            }

            public override object this[string name]
            {
                get
                {
                    if (name == "TId")
                        return _enumerator.Current.TId;
                    throw new IndexOutOfRangeException();
                }
            }

            public override bool Read() => _enumerator.MoveNext();

            public override bool HasRows => true;

            public override int Depth => 0;

            public override bool IsClosed => _isClosed;

            public override int RecordsAffected => 0;

            public override void Close() => _isClosed = true;

            public override bool NextResult() => false;

            public override int GetOrdinal(string name)
            {
                if (name == "TId") return 0;
                throw new IndexOutOfRangeException();
            }

            public override string GetName(int ordinal)
            {
                if (ordinal == 0) return "TId";
                throw new IndexOutOfRangeException();
            }

            public override Type GetFieldType(int ordinal)
            {
                if (ordinal == 0) return typeof(string);
                throw new IndexOutOfRangeException();
            }

            public override object GetValue(int ordinal)
            {
                return this[ordinal];
            }

            public override bool IsDBNull(int ordinal) => false;

            public override int GetValues(object[] values)
            {
                if (values == null) throw new ArgumentNullException(nameof(values));
                int count = Math.Min(values.Length, FieldCount);
                for (int i = 0; i < count; i++)
                    values[i] = GetValue(i);
                return count;
            }

            public override string GetDataTypeName(int ordinal)
            {
                if (ordinal == 0) return "string";
                throw new IndexOutOfRangeException();
            }

            public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal));

            public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal));

            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => 0;

            public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal));

            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => 0;

            public override Guid GetGuid(int ordinal) => Guid.Parse(GetValue(ordinal).ToString());

            public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal));

            public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));

            public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));

            public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal));

            public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));

            public override string GetString(int ordinal) => GetValue(ordinal).ToString();

            public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal));

            public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(GetValue(ordinal));

            public override System.Collections.IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}