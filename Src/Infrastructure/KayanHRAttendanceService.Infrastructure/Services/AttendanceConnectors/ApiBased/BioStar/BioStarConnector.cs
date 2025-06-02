using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.General;
using KayanHRAttendanceService.Domain.Entities.Sqlite;
using KayanHRAttendanceService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.ApiBased.BioStar;

public class BioStarConnector(IHttpService httpService, IOptions<IntegrationSettings> settings, ILogger<BioStarConnector> logger) : IAttendanceConnector
{
    public async Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
        //var records = new List<AttendanceRecord>();
        //string fromDate = _startDate;

        //if (_dynamicDate)
        //{
        //    string? lastPunch = await GetLastPunchTimeAsync();

        //    if (!string.IsNullOrWhiteSpace(lastPunch))
        //    {
        //        fromDate = lastPunch;
        //        logger.LogInformation("Dynamic date enabled. Using last punch time: {Time}", fromDate);
        //    }
        //    else
        //    {
        //        logger.LogInformation("DB empty. Using config start_date: {Start}", fromDate);
        //    }
        //}

        //while (true)
        //{
        //    var batch = await RequestBatchAsync(fromDate);
        //    if (batch == null || batch.Count == 0)
        //        break;

        //    foreach (var item in batch)
        //    {
        //        string? functionCode = null;
        //        if (item.TryGetProperty("event_type_id", out var eventType) && eventType.TryGetProperty("code", out var codeProp))
        //        {
        //            functionCode = codeProp.GetString();
        //        }
        //        else if (item.TryGetProperty("tna_key", out var tnaKey))
        //        {
        //            functionCode = tnaKey.GetString();
        //        }

        //        records.Add(new AttendanceRecord
        //        {
        //            TId = item.GetProperty("index").ToString(),
        //            EmployeeCode = item.GetProperty("user_id").GetProperty("user_id").GetString() ?? string.Empty,
        //            PunchTime = item.GetProperty("datetime").GetString() ?? string.Empty,
        //            Function = functionCode ?? string.Empty,
        //            MachineName = item.GetProperty("device_id").GetProperty("name").GetString() ?? string.Empty,
        //            MachineSerialNo = string.Empty
        //        });

        //        logger.LogInformation("Fetched Punch: {Tid}", item.GetProperty("index").ToString());
        //    }

        //    if (batch.Count < _limit)
        //        break;

        //    fromDate = batch.Last().GetProperty("datetime").GetString() ?? fromDate;
        //}

        //return records;
    }

    //private async Task<string> AuthenticateAsync()
    //{
    //    var payload = new
    //    {
    //        User = new
    //        {
    //            login_id = _username,
    //            password = _password
    //        }
    //    };


    //    var response = await httpService.SendAsync<object>(new APIRequest
    //    {
    //        Method = HttpMethod.Post,
    //        Url = $"{_server}/api/login",
    //        Data = payload
    //    });

    //}

    //private async Task<List<JsonElement>> RequestBatchAsync(string dateStr)
    //{
    //    string iso = DateTime.Parse(dateStr).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    //    string sessionId = await AuthenticateAsync();

    //    var body = new
    //    {
    //        Query = new
    //        {
    //            limit = _limit,
    //            conditions = new object[]
    //              {
    //                  new { column = "datetime", @operator = 5, values = new string[] { iso } },
    //                  new { column = "user_id", @operator = 1, values = new object[] { new { user_id = "" } } }
    //              },
    //            orders = new[]
    //            {
    //                new { column = "datetime", descending = false }
    //            }
    //        }
    //    };

    //    var response = await httpService.SendAsync<JsonElement>(new APIRequest
    //    {
    //        Method = HttpMethod.Post,
    //        Url = $"{_server}/api/events/search",
    //        Data = body,

    //    });

    //    if (response.ValueKind == JsonValueKind.Object &&
    //        response.TryGetProperty("EventCollection", out var eventCollection) &&
    //        eventCollection.TryGetProperty("rows", out var rows) &&
    //        rows.ValueKind == JsonValueKind.Array)
    //    {
    //        return rows.EnumerateArray().ToList();
    //    }

    //    logger.LogWarning("No rows returned from BioStar search.");
    //    return new List<JsonElement>();
    //}
}