using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TraceLock.Desktop.Models;

namespace TraceLock.Desktop.Data;

public static class Database
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=TraceLockDigitalEvidence;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("TRACELOCK_SQL_CONNECTION_STRING") is { Length: > 0 } value
            ? value
            : DefaultConnectionString;

    public static EvidenceDbContext Create()
    {
        var options = new DbContextOptionsBuilder<EvidenceDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new EvidenceDbContext(options);
    }

    public static void Initialize()
    {
        using var db = Create();
        db.Database.EnsureCreated();
        Seed(db);
    }

    private static void Seed(EvidenceDbContext db)
    {
        var hasher = new PasswordHasher<AppUser>();
        AddUser(db, hasher, "admin", "Wasi Ahbab", Roles.Administrator, "Admin@123!");
        AddUser(db, hasher, "investigator", "Sudipto", Roles.Investigator, "Investigator@123!");
        AddUser(db, hasher, "evidence", "Hrithik", Roles.Custodian, "Evidence@123!");
        AddUser(db, hasher, "auditor", "Jihan", Roles.Auditor, "Auditor@123!");
        AddUser(db, hasher, "forensic", "Dexter Morgan", Roles.Analyst, "Forensic@123!");
        db.SaveChanges();

        if (!db.Cases.Any())
        {
            var cases = new[]
            {
                new CaseFile { CaseNumber="CF-2026-001", Title="Cyber Fraud Investigation", CaseType="Cybercrime", Description="Financial cyber-fraud investigation.", Status="In Progress", Priority="High", Investigator="Wasi Ahbab", OpenedOn=DateTime.Today.AddDays(-12) },
                new CaseFile { CaseNumber="CF-2026-002", Title="Mobile Device Analysis", CaseType="Device Examination", Description="Mobile device evidence review.", Status="Open", Priority="Medium", Investigator="Wasi Ahbab", OpenedOn=DateTime.Today.AddDays(-7) },
                new CaseFile { CaseNumber="CF-2026-003", Title="Data Breach Review", CaseType="Incident Response", Description="Review of a reported information security incident.", Status="Pending Review", Priority="High", Investigator="Wasi Ahbab", OpenedOn=DateTime.Today.AddDays(-3) }
            };
            db.Cases.AddRange(cases);
            db.SaveChanges();
            db.Evidence.AddRange(
                new EvidenceItem { EvidenceNumber="EV-001", Name="Laptop disk image", Type="Hard drive", Status="In Examination", StorageLocation="Locker A-04", CurrentCustodian="Wasi Ahbab", Collector="Intake Unit", CaseFileId=cases[0].Id, CollectedOn=DateTime.Today.AddDays(-11) },
                new EvidenceItem { EvidenceNumber="EV-002", Name="Mobile phone extraction", Type="Mobile phone", Status="Received", StorageLocation="Locker B-02", CurrentCustodian="Evidence Custodian", Collector="Intake Unit", CaseFileId=cases[1].Id, CollectedOn=DateTime.Today.AddDays(-6) },
                new EvidenceItem { EvidenceNumber="EV-003", Name="Network log archive", Type="Network capture", Status="Reviewed", StorageLocation="Digital Vault", CurrentCustodian="Forensic Analyst", Collector="SOC Team", CaseFileId=cases[2].Id, CollectedOn=DateTime.Today.AddDays(-2) }
            );
            db.SaveChanges();
            db.ForensicAnalyses.Add(new ForensicAnalysis { EvidenceItemId=1, Analyst="Wasi Ahbab", Status="In Progress", ToolsUsed="Autopsy / FTK Imager", Findings="Initial acquisition completed; review in progress." });
            db.Notifications.AddRange(
                new Notification { Recipient="Wasi Ahbab", Title="Evidence awaiting analysis", Message="EV-002 is ready to be assigned for forensic examination.", Severity="Info" },
                new Notification { Recipient="Wasi Ahbab", Title="Priority case", Message="CF-2026-001 is a high-priority active investigation.", Severity="Warning" }
            );
            db.AuditLogs.Add(new AuditLog { Actor="System", Action="System initialized", Entity="System", Details="TraceLock desktop evidence workspace initialized with local records." });
            db.SaveChanges();
        }
    }

    private static void AddUser(EvidenceDbContext db, PasswordHasher<AppUser> hasher, string username, string name, string role, string password)
    {
        if (db.Users.Any(x => x.Username == username)) return;
        var user = new AppUser { Username=username, FullName=name, Role=role, IsActive=true };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
    }
}
