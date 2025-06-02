using System.Collections;
using System.Data.Common;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.IntegrationTests.MSSqlServerTests.FakeDb
{
    public class FakeDbDataReader : DbDataReader
    {
        private readonly IList<AttendanceRecord> _data;
        private int _currentIndex = -1;

        public FakeDbDataReader(IList<AttendanceRecord> data)
        {
            _data = data;
        }

        public override object this[int ordinal] => GetValue(ordinal);
        public override object this[string name] => GetValue(GetOrdinal(name));
        public override int Depth => 0;
        public override int FieldCount => 5;
        public override bool HasRows => _data.Any();
        public override bool IsClosed => false;
        public override int RecordsAffected => -1;

        public override bool Read()
        {
            if (_currentIndex < _data.Count - 1)
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        public override object GetValue(int ordinal)
        {
            var record = _data[_currentIndex];
            return ordinal switch
            {
                0 => record.TId,
                1 => record.EmployeeCode,
                2 => record.PunchTime,
                3 => record.Function,
                4 => record.MachineName,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public override string GetName(int ordinal) => ordinal switch
        {
            0 => "TId",
            1 => "EmployeeCode",
            2 => "PunchTime",
            3 => "Function",
            4 => "MachineName",
            _ => throw new IndexOutOfRangeException()
        };

        public override int GetOrdinal(string name) => name switch
        {
            "TId" => 0,
            "EmployeeCode" => 1,
            "PunchTime" => 2,
            "Function" => 3,
            "MachineName" => 4,
            _ => throw new IndexOutOfRangeException()
        };

        public override string GetString(int ordinal) => (string)GetValue(ordinal);
        public override bool IsDBNull(int ordinal) => GetValue(ordinal) == null;

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            return ordinal switch
            {
                0 => typeof(int),
                1 => typeof(string),
                2 => typeof(DateTime),
                3 => typeof(string),
                4 => typeof(string),
                _ => throw new IndexOutOfRangeException()
            };
        }


        public override IEnumerator GetEnumerator() => throw new NotImplementedException();
        public override bool NextResult() => false;

        public override int GetValues(object[] values)
        {
            int count = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < count; i++)
            {
                values[i] = GetValue(i);
            }
            return count;
        }

        public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);

        public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => 0;

        public override char GetChar(int ordinal) => (char)GetValue(ordinal);

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => 0;

        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

        public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);

        public override double GetDouble(int ordinal) => (double)GetValue(ordinal);

        public override float GetFloat(int ordinal) => (float)GetValue(ordinal);

        public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);

        public override short GetInt16(int ordinal) => (short)GetValue(ordinal);

        public override long GetInt64(int ordinal) => (long)GetValue(ordinal);

    }
}
