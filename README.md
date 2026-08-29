# TraceLock — Digital Evidence Management System

TraceLock is a local Windows desktop application for managing digital-evidence cases, evidence records, chain of custody, forensic analysis, integrity verification, audit history, notifications, reports, and users.

## Technology

- C#
- .NET 8
- Windows Forms
- Entity Framework Core 8
- Microsoft SQL Server

## Requirements

- Windows 10/11
- Visual Studio Community/Professional 2026 (or a compatible Visual Studio version)
- Visual Studio workload: **.NET desktop development**
- .NET 8 SDK / targeting pack

The project targets `net8.0-windows`. Visual Studio's own version can be newer than .NET 8; that does not change the project's target framework.

## Open the project

1. Extract the project ZIP.
2. Open `TraceLock.Desktop.sln` in Visual Studio.
3. Select **Build → Rebuild Solution**.
4. Run the `TraceLock` project with the green Start button.

## Demo data

A local Microsoft SQL Server database is included under `App_Data`. Evidence files used by the demo are also included under `App_Data/EvidenceStorage`.

Demo administrator account:

- Username: `admin`
- Password: `Admin@123!`

Other seeded accounts:

- `investigator` / `Investigator@123!`
- `evidence` / `Evidence@123!`
- `auditor` / `Auditor@123!`
- `forensic` / `Forensic@123!`

## Publish a standalone Windows application

For a normal Intel/AMD Windows desktop PC, publish for `win-x64`.

For Windows 11 ARM running on an Apple Silicon Mac through virtualization, publish for `win-arm64`.

A self-contained publish can be used when the target PC should not need a separate .NET runtime.

Example command for x64:

```powershell
dotnet publish .\TraceLock.Desktop.csproj -c Release -r win-x64 --self-contained true
```

The published application can be launched directly from its executable.

## Data storage

On first run, the bundled Microsoft SQL Server database and evidence storage are copied to the current Windows user's Local AppData `TraceLock` folder. This avoids login and database write problems when the application is opened from a protected folder. The original bundled files remain unchanged.
