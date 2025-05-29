using KayanHRAttendanceService.WindowsService.Storage.Models;
using Microsoft.Data.Sqlite;

namespace KayanHRAttendanceService.WindowsService.Storage;

public class SqliteStorage : ISqliteStorage
{
    private readonly string _connectionString = "Data Source=SqlLiteLocalDb/Attendance.db";

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists("SqlLiteLocalDb"))
        {
            Directory.CreateDirectory("SqlLiteLocalDb");
        }

        using var connection = new SqliteConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText = "CREATE TABLE IF NOT EXISTS AttendanceData(ID INTEGER PRIMARY KEY AUTOINCREMENT,EmployeeCode TEXT NOT NULL,PunchTime TEXT NOT NULL,Function TEXT NOT NULL,Machine TEXT NULL,Status TEXT NULL,TId TEXT NOT NULL);";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }


    public async Task PushDataAsync(string message, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO AttendanceData(ID,EmployeeCode,PunchTime,Function,Machine,Status,TId)";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<List<AttendanceRecord>> GetDataAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<AttendanceRecord>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT ID,Status FROM AttendanceData";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);


        return results;
    }

    public Task PushDataAsync(List<AttendanceRecord> model, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
