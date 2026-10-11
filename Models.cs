namespace StaffAttendanceManager;
public sealed class Staff
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Shift { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool Active { get; set; } = true;
}
public sealed class Attendance
{
    public string StaffId { get; set; } = "";
    public string WorkDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public string Status { get; set; } = "Present";
    public string InTime { get; set; } = "";
    public string OutTime { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Supervisor { get; set; } = "";
    public string Remarks { get; set; } = "";
}
public sealed class StaffOption
{
    public string Id { get; set; } = "";
    public string Display { get; set; } = "";
    public override string ToString() => Display;
}
public sealed class AttendanceView
{
    public string WorkDate { get; set; } = "";
    public string StaffId { get; set; } = "";
    public string StaffCode { get; set; } = "";
    public string StaffName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Status { get; set; } = "";
    public string InTime { get; set; } = "";
    public string OutTime { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Supervisor { get; set; } = "";
    public string Remarks { get; set; } = "";
}