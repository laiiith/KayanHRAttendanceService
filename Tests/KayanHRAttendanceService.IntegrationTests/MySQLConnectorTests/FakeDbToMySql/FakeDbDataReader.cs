using System.Data.Common;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

public class FakeDbDataReader : DbDataReader
{
    private readonly IEnumerator<AttendanceRecord> _enumerator;

    public FakeDbDataReader(IEnumerable<AttendanceRecord> data)
    {
        _enumerator = data.GetEnumerator();
    }

    public override bool Read() => _enumerator.MoveNext();

    public override int FieldCount => 5;

    public override object GetValue(int ordinal)
    {
        var current = _enumerator.Current;
        return ordinal switch
        {
            0 => current.TId,
            1 => current.EmployeeCode,
            2 => current.PunchTime,
            3 => current.Function,
            4 => current.MachineName,
            _ => null
        };
    }

    public override string GetName(int ordinal)
    {
        return ordinal switch
        {
            0 => "TId",
            1 => "EmployeeCode",
            2 => "PunchTime",
            3 => "Function",
            4 => "MachineName",
            _ => string.Empty
        };
    }

    public override int GetOrdinal(string name)
    {
        return name switch
        {
            "TId" => 0,
            "EmployeeCode" => 1,
            "PunchTime" => 2,
            "Function" => 3,
            "MachineName" => 4,
            _ => -1
        };
    }

    public override bool IsDBNull(int ordinal)
    {
        return GetValue(ordinal) == null;
    }

    public override string GetString(int ordinal)
    {
        var val = GetValue(ordinal);
        return val?.ToString() ?? string.Empty;
    }


    public override bool HasRows => true;

    public override bool NextResult() => false;

    public override int RecordsAffected => 0;

    public override bool IsClosed => false;

    public override int Depth => 0;

    public override void Close() { }

    public override bool GetBoolean(int ordinal) => throw new NotImplementedException();
    public override byte GetByte(int ordinal) => throw new NotImplementedException();
    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override char GetChar(int ordinal) => throw new NotImplementedException();
    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override string GetDataTypeName(int ordinal) => throw new NotImplementedException();
    public override DateTime GetDateTime(int ordinal) => throw new NotImplementedException();
    public override decimal GetDecimal(int ordinal) => throw new NotImplementedException();
    public override double GetDouble(int ordinal) => throw new NotImplementedException();
    public override System.Collections.IEnumerator GetEnumerator() => throw new NotImplementedException();
    public override Type GetFieldType(int ordinal) => typeof(string);
    public override float GetFloat(int ordinal) => throw new NotImplementedException();
    public override Guid GetGuid(int ordinal) => throw new NotImplementedException();
    public override short GetInt16(int ordinal) => throw new NotImplementedException();
    public override int GetInt32(int ordinal) => throw new NotImplementedException();
    public override long GetInt64(int ordinal) => throw new NotImplementedException();

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override object this[int ordinal] => GetValue(ordinal);

    public override int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }
        return count;
    }
}
