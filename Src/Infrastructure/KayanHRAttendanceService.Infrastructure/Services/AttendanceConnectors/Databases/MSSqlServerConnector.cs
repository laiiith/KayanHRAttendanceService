using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.Databases;

public class MSSqlServerConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }
}
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using Microsoft.Extensions.Logging;
//using KayanHRAttendanceService.Domain.Entities.Sqlite;
//using KayanHRAttendanceService.Domain.Interfaces;

//namespace KayanHRAttendanceService.Infrastructure.DAL
//{
//    public class MSSQL : IDAL
//    {
//        private readonly string _connectionString;
//        private readonly string _getProcedure;
//        private readonly string _updateProcedure;
//        private readonly Dictionary<string, string> _functionMapping;
//        private readonly ILogger<MSSQL> _logger;

//        public MSSQL(IConfiguration config, ILogger<MSSQL> logger)
//        {
//            _logger = logger;
//            _connectionString = config["Integration:Server"];
//            _getProcedure = config["Integration:Get_Data_Procedure"];
//            _updateProcedure = config["Integration:Update_Procedure"];
//            _functionMapping = config.GetSection("Function_Mapping").Get<Dictionary<string, string>>() ?? new();
//        }

//        public List<AttendanceRecord> FetchAttendance()
//        {
//            var data = new List<AttendanceRecord>();

//            using var conn = new SqlConnection(_connectionString);
//            using var cmd = new SqlCommand(_getProcedure, conn)
//            {
//                CommandType = CommandType.StoredProcedure
//            };

//            conn.Open();
//            using var reader = cmd.ExecuteReader();

//            while (reader.Read())
//            {
//                var record = new AttendanceRecord
//                {
//                    TId = reader["tid"].ToString(),
//                    EmployeeCode = reader["EmployeeCardNumber"].ToString(),
//                    PunchTime = reader["AttendanceDate"].ToString(),
//                    Function = reader["FunctionType"].ToString(),
//                    MachineName = reader["MachineName"].ToString()
//                };

//                data.Add(record);
//                _logger.LogInformation("Fetched Punch: {TId}", record.TId);
//            }

//            return data;
//        }

//        public void UpdateSql(List<AttendanceRecord> data, int status)
//        {
//            using var conn = new SqlConnection(_connectionString);
//            conn.Open();

//            using var cmd = conn.CreateCommand();
//            cmd.CommandText = @"
//                IF OBJECT_ID('tempdb..#TempTVP') IS NOT NULL DROP TABLE #TempTVP;
//                CREATE TABLE #TempTVP (
//                    tid INT,
//                    flag INT
//                );";
//            cmd.ExecuteNonQuery();

//            using var bulkInsert = conn.CreateCommand();
//            bulkInsert.CommandText = "INSERT INTO #TempTVP (tid, flag) VALUES (@tid, @flag)";

//            foreach (var record in data)
//            {
//                bulkInsert.Parameters.Clear();
//                bulkInsert.Parameters.AddWithValue("@tid", int.Parse(record.TId));
//                bulkInsert.Parameters.AddWithValue("@flag", status);
//                bulkInsert.ExecuteNonQuery();
//            }

//            using var execUpdate = new SqlCommand(_updateProcedure, conn)
//            {
//                CommandType = CommandType.StoredProcedure
//            };
//            execUpdate.ExecuteNonQuery();
//        }
//    }
//}












//﻿using System.Data;
//using Core.Interfaces;
//using KayanAttendanceService.Core.Entities;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace KayanAttendanceService.Infrastructure.AttendanceDestinations
//{
//    public class MSSQLService : IAttendanceDestination
//    {
//        private readonly string _connectionString;
//        private readonly string _storedProcedure;
//        private readonly ILogger<MSSQLService> _logger;

//        public MSSQLService(IConfiguration config, ILogger<MSSQLService> logger)
//        {
//            _connectionString = config["Integration:Server"] ?? throw new ArgumentNullException("Integration:Server connection string is missing.");
//            _storedProcedure = config["Integration:Update_Procedure"] ?? throw new ArgumentNullException("Integration:Update_Procedure is missing.");
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public async Task UpdateAttendanceAsync(List<AttendanceData> data, int status)
//        {
//            if (data == null || data.Count == 0)
//            {
//                _logger.LogWarning("No attendance data provided to update.");
//                return;
//            }

//            var dataTable = CreateDataTable(data, status);

//            try
//            {
//                await using var connection = new SqlConnection(_connectionString);
//                await connection.OpenAsync();

//                await using var command = new SqlCommand(_storedProcedure, connection)
//                {
//                    CommandType = CommandType.StoredProcedure
//                };

//                var param = command.Parameters.AddWithValue("@TempTVP", dataTable);
//                param.SqlDbType = SqlDbType.Structured;
//                param.TypeName = "dbo.TempTVP";

//                var rowsAffected = await command.ExecuteNonQueryAsync();

//                _logger.LogInformation("Updated {Count} attendance records with status {Status}. Rows affected: {RowsAffected}", data.Count, status, rowsAffected);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating attendance data.");
//                throw;
//            }
//        }

//        private static DataTable CreateDataTable(List<AttendanceData> data, int status)
//        {
//            var table = new DataTable();
//            table.Columns.Add("tid", typeof(int));
//            table.Columns.Add("flag", typeof(int));

//            foreach (var item in data)
//            {
//                if (!int.TryParse(item.Tid, out int tid))
//                {

//                    continue;
//                }

//                table.Rows.Add(tid, status);
//            }

//            return table;
//        }
//    }
//}