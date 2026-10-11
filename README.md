# Staff Attendance Manager

Windows desktop staff attendance application using C# / .NET 8 WinForms.

## Features
- Add, edit and delete staff; auto-generated staff codes.
- Daily attendance, status, optional in/out times, reason, supervisor and remarks.
- Edit saved attendance in a separate dialog; blank times can remain blank.
- Duplicate staff/date protection while editing.
- Department sorting with Incharge first.
- Dashboard, date-range reports, Excel export, backup and restore.
- Local JSON storage.

## Requirements
- Windows 10/11
- .NET 8 SDK
- Internet connection for first NuGet restore (ClosedXML package)

## Build / run
Open PowerShell in this folder:
```powershell
dotnet restore
dotnet build -c Release
dotnet run
```

## Publish self-contained Windows x64
```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```
Output: `bin\Release\net8.0-windows\win-x64\publish\`

## Data and backups
Data: `%LOCALAPPDATA%\StaffAttendanceManager\staff.json` and `attendance.json`.
Backups: `Documents\StaffAttendanceBackups`.

## Notes
This project stores data locally. It does not include login, roles, cloud sync, encryption, or an audit log. Deleting a staff member also removes their attendance. Make backups regularly.