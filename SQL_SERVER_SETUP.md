# TraceLock - Microsoft SQL Server Setup

This project now uses **Microsoft SQL Server through Entity Framework Core 8**. SQLite is no longer used.

## Default database

The application uses Microsoft SQL Server LocalDB by default:

```text
Server=(localdb)\MSSQLLocalDB
Database=TraceLockDigitalEvidence
Authentication=Windows Authentication
```

The database is created automatically by Entity Framework Core the first time the application runs.

## View the database in SQL Server Management Studio (SSMS)

1. Open SSMS.
2. In **Server name**, enter:

```text
(localdb)\MSSQLLocalDB
```

3. Select **Windows Authentication**.
4. Click **Connect**.
5. Expand **Databases**.
6. Open **TraceLockDigitalEvidence**.
7. Expand **Tables**.

The application creates and uses tables for Users, Cases, CaseNotes, CasePeople, Evidence, CustodyRecords, ForensicAnalyses, EvidenceFiles, Notifications, and AuditLogs.

## If your SQL Server uses another instance

Set the Windows environment variable `TRACELOCK_SQL_CONNECTION_STRING` before starting TraceLock. For example:

```text
Server=.\SQLEXPRESS;Database=TraceLockDigitalEvidence;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

The application will use that connection string instead of LocalDB.
