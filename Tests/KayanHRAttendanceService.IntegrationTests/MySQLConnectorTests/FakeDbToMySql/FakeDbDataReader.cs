using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

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
