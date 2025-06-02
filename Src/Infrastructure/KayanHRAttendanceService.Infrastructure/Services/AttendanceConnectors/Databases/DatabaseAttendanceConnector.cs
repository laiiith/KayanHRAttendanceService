using FastMember;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public abstract class DatabaseAttendanceConnector : AttendanceConnector
{
    protected abstract Task<DbConnection> CreateDbConnection();
    protected void AddDatabaseParameters(SqlCommand command, List<SqlParameter> sqlParameters)
    {
        command.Parameters.Clear();
        foreach (SqlParameter sqlParameter in sqlParameters)
        {
            command.Parameters.Add(sqlParameter);
        }
    }
    protected List<AttendanceRecord> MapToList(IDataReader dataReader)
    {
        var list = new List<AttendanceRecord>();
        while (dataReader.Read())
        {
            var obj = new AttendanceRecord();
            MapDataToObject(dataReader, obj);
            list.Add(obj);
        }
        return list;
    }
    private void MapDataToObject(IDataReader dataReader, AttendanceRecord obj)
    {
        var objectMemberAccessor = TypeAccessor.Create(typeof(AttendanceRecord));
        var propertiesHashSet = objectMemberAccessor.GetMembers().Select(mp => mp.Name).ToHashSet();

        for (int i = 0; i < dataReader.FieldCount; i++)
        {
            string columnName = dataReader.GetName(i);
            if (propertiesHashSet.Contains(columnName))
            {
                var value = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
                objectMemberAccessor[obj, columnName] = value;
            }
        }
    }
}
