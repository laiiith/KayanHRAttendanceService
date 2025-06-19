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
