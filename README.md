<h2>📦 KayanAttendance DB Setup Scripts</h2>
<p>The following scripts are used to create and initialize the <code>KayanAttendance</code> database across <strong>MS SQL Server</strong>, <strong>MySQL</strong>, and <strong>PostgreSQL</strong> environments.</p>

---

### 📌 MSSQL Server Script
```sql
IF DB_ID('kayan_db') IS NULL
    CREATE DATABASE kayan_db;
GO

USE kayan_db;
GO

IF OBJECT_ID('KayanAttendance', 'U') IS NOT NULL
    DROP TABLE KayanAttendance;
GO

CREATE TABLE KayanAttendance (
    TID INT PRIMARY KEY,
    EmployeeCardNumber NVARCHAR(50),
    AttendanceDate DATETIME,
    FunctionType NVARCHAR(50),
    MachineName NVARCHAR(50),
    Flag INT DEFAULT 0
);
GO

INSERT INTO KayanAttendance (TID, EmployeeCardNumber, AttendanceDate, FunctionType, MachineName)
VALUES
(1, 'EMP001', '2025-06-04 08:01:00', 'CheckIn', 'Machine_A'),
-- ... more rows ...
(10, 'EMP005', '2025-06-04 16:59:00', 'CheckOut', 'Machine_A');
GO

IF OBJECT_ID('sp_FetchAttendance', 'P') IS NOT NULL
    DROP PROCEDURE sp_FetchAttendance;
GO

CREATE PROCEDURE sp_FetchAttendance
AS
BEGIN
    SELECT 
        TID AS TId,
        EmployeeCardNumber AS EmployeeCode,
        CONVERT(VARCHAR, AttendanceDate, 120) AS PunchTime,
        FunctionType AS [Function],
        MachineName
    FROM KayanAttendance;
END
GO

IF OBJECT_ID('tempdb..#TempTVP') IS NOT NULL
    DROP TABLE #TempTVP;

CREATE TABLE #TempTVP (
    tid INT,
    flag INT
);

IF OBJECT_ID('sp_UpdateFetchedAttendance', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateFetchedAttendance;
GO

CREATE PROCEDURE sp_UpdateFetchedAttendance
AS
BEGIN
    UPDATE ka
    SET ka.Flag = tvp.flag
    FROM KayanAttendance ka
    INNER JOIN #TempTVP tvp ON ka.TID = tvp.tid;
END
GO
