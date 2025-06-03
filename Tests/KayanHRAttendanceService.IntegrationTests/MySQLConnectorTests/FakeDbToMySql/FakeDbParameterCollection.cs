using System.Collections;
using System.Data.Common;

namespace KayanHRAttendanceService.IntegrationTests.MySQLConnectorTests.FakeDbToMySql;

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
