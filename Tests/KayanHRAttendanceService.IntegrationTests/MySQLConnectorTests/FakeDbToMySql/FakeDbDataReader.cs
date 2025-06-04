using KayanHRAttendanceService.Domain.Entities.Sqlite;
using System.Collections;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

public class FakeDbDataReader : DbDataReader
{
    private readonly List<AttendanceRecord> _data;
    private int _index = -1;

    public FakeDbDataReader(List<AttendanceRecord> data)
    {
        _data = data;
    }

    public override bool Read() => ++_index < _data.Count;

    public override string GetString(int ordinal) => ordinal switch
    {
        0 => _data[_index].EmployeeCode,
        1 => _data[_index].PunchTime,
        2 => _data[_index].Function,
        3 => _data[_index].MachineName,
        4 => _data[_index].MachineSerialNo,
        5 => _data[_index].Status,
        6 => _data[_index].TId,
        _ => throw new IndexOutOfRangeException()
    };

    public override int GetOrdinal(string name) => name switch
    {
        "EmployeeCode" => 0,
        "PunchTime" => 1,
        "Function" => 2,
        "MachineName" => 3,
        "MachineSerialNo" => 4,
        "Status" => 5,
        "TId" => 6,
        _ => throw new IndexOutOfRangeException()
    };

    public override bool HasRows => _data.Any();
    public override bool IsClosed => false;
    public override int FieldCount => 7;
    public override object this[string name] => GetString(GetOrdinal(name));
    public override object this[int ordinal] => GetString(ordinal);

    // باقي الدوال يمكن تركها بـ NotImplementedException
    public override bool NextResult() => false;

    public override int Depth => throw new NotImplementedException();

    public override int RecordsAffected => throw new NotImplementedException();

    public override IEnumerator GetEnumerator() => throw new NotImplementedException();

    public override string GetName(int ordinal) => throw new NotImplementedException();

    public override Type GetFieldType(int ordinal) => typeof(string);

    public override object GetValue(int ordinal) => GetString(ordinal);

    public override bool IsDBNull(int ordinal) => false;

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override float GetFloat(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override Guid GetGuid(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override short GetInt16(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetInt32(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetInt64(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    // باقي الدوال يمكن تكملتها لاحقًا عند الحاجة
}