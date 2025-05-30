using KayanHRAttendanceService.Entities.Sqlite;
using Microsoft.Data.Sqlite;
using System.Data;

namespace KayanHRAttendanceService.Storage.SqliteDataAccess;
public class SqliteStorage : ISqliteStorage
{
    public int ErrorID { get; private set; }

    private readonly SqliteParameterCollection _sqlParameters;

    private readonly string _connectionString;
    private SqliteStorage() { _connectionString = "Data Source=SqlLiteLocalDb/Attendance.db"; }

    public static async Task<SqliteStorage> CreateSqliteStorageAsync()
    {
        var storage = new SqliteStorage();
        await storage.InitializeDatabaseAsync();
        return storage;
    }

    public async Task PushDataAsync(string _commandText, CommandType _commandType = CommandType.StoredProcedure)
    => await ExecuteAsync<int>(_commandText, _commandType);

    public async Task<List<T>> GetDataAsync<T>(string _commandText, CommandType _commandType = CommandType.StoredProcedure)
    => await ExecuteAsync(_commandText, _commandType, options => options.DataReaderMapToList<T>(true));

    public void AddParameter(string name, object value, ParameterDirection direction = ParameterDirection.Input, SqliteType? dbType = null)
    {
        var parameter = new SqliteParameter(name, value ?? DBNull.Value)
        {
            Direction = direction
        };
        if (dbType.HasValue)
        {
            parameter.SqliteType = dbType.Value;
        }
        _sqlParameters.Add(parameter);
    }
    #region Private Methods

    private async Task<T> ExecuteAsync<T>(string commandText, CommandType commandType, Func<SqlDataReader, T> mapFunction = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var sqlCommand = new SqlCommand(commandText, connection);
        {
            sqlCommand.CommandType = commandType;
        }
        ;

        PrepareCommand(sqlCommand);

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            if (mapFunction != null)
            {
                using var reader = await sqlCommand.ExecuteReaderAsync();
                ErrorID = (int)(sqlCommand.Parameters["@ReturnVal"].Value ?? 0);
                return ErrorID == 0 ? mapFunction(reader) : default;
            }
            else
            {
                if (typeof(T) == typeof(int))
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                    ErrorID = (int)(sqlCommand.Parameters["@ReturnVal"].Value ?? 0);
                    return default;
                }
                else
                {
                    var result = await sqlCommand.ExecuteScalarAsync();
                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }
        catch (Exception ex)
        {
            KayanAdmin.Core.Logger.Logger.Error(ex.Message);
            ErrorID = -999;
            throw new Exception($"Data access error: {ex.Message}", ex);
        }
        finally
        {
            RetrieveOutputParameters(sqlCommand);
            await connection.CloseAsync();
            _sqlParameters.Clear();
        }
    }

    private void PrepareCommand(SqliteCommand sqlCommand)
    {
        sqlCommand.Parameters.Clear();

        if (_sqlParameters?.Count > 0)
            sqlCommand.Parameters.AddRange([.. _sqlParameters]);

        var returnParameter = sqlCommand.Parameters.Add("@ReturnVal", SqliteType.Integer);
        returnParameter.Direction = ParameterDirection.ReturnValue;
    }



    #endregion Private Methods

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

    public async Task<List<AttendanceRecord>> GetDataAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<AttendanceRecord>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT ID,Status FROM AttendanceData";
        command.CommandType = CommandType.Text;
        command.Parameters.Add
        using var reader = await command.ExecuteReaderAsync(cancellationToken);


        return results;
    }

    public async Task PushDataAsync(List<AttendanceRecord> model, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO AttendanceData(ID,EmployeeCode,PunchTime,Function,Machine,Status,TId)";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}