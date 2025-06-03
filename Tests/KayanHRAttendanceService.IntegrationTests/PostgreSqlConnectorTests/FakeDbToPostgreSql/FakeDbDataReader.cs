using KayanHRAttendanceService.Domain.Entities.Sqlite;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.PostgreSqlConnectorTests.FakeDbToPostgreSql
{
    public class FakeDbDataReader : DbDataReader
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
