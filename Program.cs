namespace StaffAttendanceManager;
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try { LocalStorage.Initialize(); Application.Run(new MainForm()); }
        catch (Exception ex) { MessageBox.Show("The application could not start.\n\n" + ex.Message, "Staff Attendance Manager", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}