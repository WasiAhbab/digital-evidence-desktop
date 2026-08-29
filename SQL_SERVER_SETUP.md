# TraceLock - Microsoft SQL Server Setup

TraceLock uses **Microsoft SQL Server with Entity Framework Core 8** for database management. The application no longer uses SQLite.

---

## Default Database Configuration

By default, TraceLock uses **Microsoft SQL Server LocalDB**.

```text
Server=(localdb)\MSSQLLocalDB
Database=TraceLockDigitalEvidence
Authentication=Windows Authentication