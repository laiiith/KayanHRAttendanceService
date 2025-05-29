using Microsoft.Data.Sqlite;

namespace KayanHRAttendanceService.WindowsService.Storage;

public class SqliteStorage : ISqliteStorage
{
    private readonly string _connectionString = "Data Source=SqlLiteLocalDb/Attendance.db";

    public SqliteStorage()
    {
        if (!Directory.Exists("SqlLiteLocalDb"))
        {
            Directory.CreateDirectory("SqlLiteLocalDb");
        }
    }
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS AttendanceData(ID INTEGER PRIMARY KEY AUTOINCREMENT,EmployeeCode TEXT NOT NULL,PunchTime TEXT NOT NULL,Function TEXT NOT NULL,Machine TEXT NULL,Status TEXT NULL,TId TEXT NOT NULL);
            ";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<T>> GetDataAsync<T>(CancellationToken cancellationToken = default)
    {
        var results = new List<(int, DateTime)>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT ID,Status FROM AttendanceData";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);


        return results;
    }

    public async Task PushDataAsync(string message, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO AttendanceData(ID,EmployeeCode,PunchTime,Function,Machine,Status,TId)";
        command.Parameters.AddWithValue("$msg", message);
        command.Parameters.AddWithValue("$createdAt", createdAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
