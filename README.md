<h1 align="center">Hi 👋, I'm Laith</h1>
<h3 align="center">A passionate full-stack developer from Jordan</h3>

- 🔭 I’m currently working on **KayanHrKayanHRAttendanceService**
  - PostgreSQL, MySQL, MS SQL Server Integration
  - Backend Functions, Stored Procedures, Attendance System

- 👯 I’m looking to collaborate on **full-stack and database-intensive applications**

- 🤝 I’m looking for help with **DevOps and real-time systems integration**

- 🌱 I’m currently learning **.NET Core MVC, Entity Framework, PostgreSQL, and DevOps tools**

- 💬 Ask me about **C#, SQL, ASP.NET Core, and PostgreSQL triggers/functions**

- 📫 How to reach me: **mohannadsroor181@gmail.com**

- 👨‍💻 All of my projects are available at: [My GitHub](https://github.com/mohannadsroor181)

- 📄 Know about my experiences: [My Resume (PDF)](https://your-resume-link.com)

- ⚡ Fun fact: I used to be a carpenter before becoming a developer!

---

## 📦 KayanAttendance DB Setup Scripts

The following scripts are used to create and initialize the `KayanAttendance` database across **MS SQL Server**, **MySQL**, and **PostgreSQL** environments. They include table creation, sample data, and stored procedures for fetching and updating attendance.

### 📌 MSSQL Server Script

```sql
-- Helper MSSqlServerConnector Script: Setup KayanAttendance DB

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


### 📌  MySQL Script
```sql
-- Helper MySQLConnector Script: Setup KayanAttendance DB

CREATE DATABASE IF NOT EXISTS kayan_db;
USE kayan_db;

DROP TABLE IF EXISTS KayanAttendance;

CREATE TABLE KayanAttendance (
TID INT PRIMARY KEY,
EmployeeCardNumber VARCHAR(50),
AttendanceDate DATETIME,
FunctionType VARCHAR(20),
MachineName VARCHAR(50),
Flag INT DEFAULT 0
);

INSERT INTO KayanAttendance (TID, EmployeeCardNumber, AttendanceDate, FunctionType, MachineName)
VALUES
(1, 'EMP001', '2025-06-04 08:01:00', 'CheckIn', 'Machine_A'),
-- ... more rows ...
(10, 'EMP005', '2025-06-04 16:59:00', 'CheckOut', 'Machine_A');

DROP PROCEDURE IF EXISTS sp_FetchAttendance;
DELIMITER //
CREATE PROCEDURE sp_FetchAttendance()
BEGIN
SELECT
TID AS TId,
EmployeeCardNumber AS EmployeeCode,
CAST(AttendanceDate AS CHAR) AS PunchTime,
FunctionType AS Function,
MachineName
FROM KayanAttendance;
END //
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_UpdateFetchedAttendance;
DELIMITER //
CREATE PROCEDURE sp_UpdateFetchedAttendance()
BEGIN
UPDATE KayanAttendance AS ka
JOIN temp_tvp AS temp ON ka.TID = temp.tid
SET ka.Flag = temp.flag;
END //
DELIMITER ;



### 📌  PostgreSQL Script
```sql
-- Helper PostgreSQLConnector Script: Setup KayanAttendance DB
DROP TABLE IF EXISTS KayanAttendance;

CREATE TABLE KayanAttendance (
TID INT PRIMARY KEY,
EmployeeCardNumber VARCHAR(50),
AttendanceDate TIMESTAMP,
FunctionType VARCHAR(50),
MachineName VARCHAR(50),
Flag INT DEFAULT 0
);

INSERT INTO KayanAttendance (TID, EmployeeCardNumber, AttendanceDate, FunctionType, MachineName)
VALUES
(1, 'EMP001', '2025-06-04 08:01:00', 'CheckIn', 'Machine_A'),
-- ... more rows ...
(10, 'EMP005', '2025-06-04 16:59:00', 'CheckOut', 'Machine_A');

DROP FUNCTION IF EXISTS fn_fetch_attendance();

CREATE OR REPLACE FUNCTION fn_fetch_attendance()
RETURNS TABLE (
TId INT,
EmployeeCode VARCHAR,
PunchTime TEXT,
"Function" VARCHAR,
MachineName VARCHAR
)
LANGUAGE plpgsql
AS $$
BEGIN
RETURN QUERY
SELECT
TID,
EmployeeCardNumber,
CAST(AttendanceDate AS TEXT),
FunctionType,
MachineName
FROM KayanAttendance;
END;
$$;

DROP TABLE IF EXISTS temp_tvp;

CREATE TEMP TABLE temp_tvp (
tid INT,
flag INT
);

INSERT INTO temp_tvp (tid, flag)
VALUES (1, 1), (2, 1), (3, 1);

DROP PROCEDURE IF EXISTS sp_update_fetched_attendance();

CREATE OR REPLACE PROCEDURE sp_update_fetched_attendance()
LANGUAGE plpgsql
AS $$
BEGIN
UPDATE KayanAttendance AS A
SET Flag = T.flag
FROM temp_tvp AS T
WHERE A.TID = T.tid;
END;
$$;
