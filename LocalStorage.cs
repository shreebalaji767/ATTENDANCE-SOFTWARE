using System.Text.Json;
namespace StaffAttendanceManager;
public static class LocalStorage
{
    private static readonly object Sync = new();
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public static string DataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StaffAttendanceManager");
    private static string StaffFile => Path.Combine(DataDirectory, "staff.json");
    private static string AttendanceFile => Path.Combine(DataDirectory, "attendance.json");
    public static void Initialize()
    {
        lock (Sync) { Directory.CreateDirectory(DataDirectory); if (!File.Exists(StaffFile)) Write(StaffFile, new List<Staff>()); if (!File.Exists(AttendanceFile)) Write(AttendanceFile, new List<Attendance>()); }
    }
    public static List<Staff> GetStaff() { lock (Sync) { Initialize(); return Read<List<Staff>>(StaffFile) ?? new(); } }
    public static List<Attendance> GetAttendance() { lock (Sync) { Initialize(); return Read<List<Attendance>>(AttendanceFile) ?? new(); } }
    public static void SaveStaff(List<Staff> items) { ArgumentNullException.ThrowIfNull(items); lock (Sync) { Initialize(); Write(StaffFile, items); } }
    public static void SaveAttendance(List<Attendance> items) { ArgumentNullException.ThrowIfNull(items); lock (Sync) { Initialize(); Write(AttendanceFile, items); } }
    public static string CreateBackup()
    {
        lock (Sync) { Initialize(); string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StaffAttendanceBackups"); Directory.CreateDirectory(root); string folder = Path.Combine(root, "Backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")); Directory.CreateDirectory(folder); File.Copy(StaffFile, Path.Combine(folder, "staff.json"), true); File.Copy(AttendanceFile, Path.Combine(folder, "attendance.json"), true); return folder; }
    }
    public static void RestoreBackup(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) throw new DirectoryNotFoundException("The selected backup folder does not exist.");
        string sf = Path.Combine(folder, "staff.json"), af = Path.Combine(folder, "attendance.json");
        if (!File.Exists(sf) || !File.Exists(af)) throw new InvalidDataException("Backup must contain staff.json and attendance.json.");
        _ = Read<List<Staff>>(sf) ?? new(); _ = Read<List<Attendance>>(af) ?? new();
        lock (Sync) { Initialize(); CreateBackup(); File.Copy(sf, StaffFile, true); File.Copy(af, AttendanceFile, true); }
    }
    private static T? Read<T>(string path) => JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options);
    private static void Write<T>(string path, T value) { string tmp = path + ".tmp"; File.WriteAllText(tmp, JsonSerializer.Serialize(value, Options)); File.Move(tmp, path, true); }
}