
using System.Diagnostics;
using System.Globalization;
using ClosedXML.Excel;

namespace StaffAttendanceManager;

public sealed class MainForm : Form
{
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };

    private readonly Label dashboard = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(20),
        Font = new Font("Segoe UI", 12)
    };

    private readonly DataGridView staffGrid = Grid();
    private readonly DataGridView attendanceGrid = Grid();
    private readonly DataGridView reportGrid = Grid();

    private readonly TextBox code = new()
    {
        ReadOnly = true,
        PlaceholderText = "Generated automatically"
    };

    private readonly TextBox name = new();
    private readonly TextBox department = new();
    private readonly TextBox designation = new();
    private readonly TextBox shift = new();
    private readonly TextBox phone = new();

    private readonly CheckBox active = new()
    {
        Text = "Active",
        Checked = true,
        AutoSize = true
    };

    private string? editingStaffId;

    private readonly DateTimePicker workDate = new()
    {
        Format = DateTimePickerFormat.Short
    };

    private readonly ComboBox staffPicker = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly ComboBox status = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly TextBox inTime = new()
    {
        PlaceholderText = "Optional, e.g. 09:00 AM"
    };

    private readonly TextBox outTime = new()
    {
        PlaceholderText = "Optional, e.g. 05:00 PM"
    };

    private readonly TextBox reason = new();
    private readonly TextBox supervisor = new();
    private readonly TextBox remarks = new();

    private readonly DateTimePicker fromDate = new()
    {
        Format = DateTimePickerFormat.Short
    };

    private readonly DateTimePicker toDate = new()
    {
        Format = DateTimePickerFormat.Short
    };

    public MainForm()
    {
        Text = "Staff Attendance Manager";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1250, 780);
        MinimumSize = new Size(950, 600);
        Font = new Font("Segoe UI", 9F);

        BuildDashboard();
        BuildStaff();
        BuildAttendance();
        BuildReports();
        BuildBackup();

        Controls.Add(tabs);

        RefreshAll();
    }

    private static DataGridView Grid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };
    }

    private static Button Btn(string text, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 32,
            Margin = new Padding(4)
        };

        button.Click += click;
        return button;
    }

    private static void Field(
        TableLayoutPanel table,
        int row,
        string label,
        Control control,
        int labelCol = 0)
    {
        table.Controls.Add(
            new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(4, 8, 4, 4)
            },
            labelCol,
            row);

        control.Dock = DockStyle.Fill;

        table.Controls.Add(control, labelCol + 1, row);
    }

    private void BuildDashboard()
    {
        var page = new TabPage("Dashboard");

        var refresh = Btn("Refresh", (_, _) => RefreshAll());
        refresh.Dock = DockStyle.Top;

        page.Controls.Add(dashboard);
        page.Controls.Add(refresh);

        tabs.TabPages.Add(page);
    }

    private void BuildStaff()
    {
        var page = new TabPage("Staff Management");

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 390,
            FixedPanel = FixedPanel.Panel1
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(8)
        };

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 140));

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        Field(table, 0, "Staff Code", code);
        Field(table, 1, "Full Name *", name);
        Field(table, 2, "Department", department);
        Field(table, 3, "Designation", designation);
        Field(table, 4, "Shift", shift);
        Field(table, 5, "Phone", phone);

        table.Controls.Add(active, 1, 6);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true
        };

        buttons.Controls.Add(
            Btn("Add / Save Staff", (_, _) => SaveStaff()));

        buttons.Controls.Add(
            Btn("Edit Selected", (_, _) => EditStaff()));

        buttons.Controls.Add(
            Btn("Delete Selected", (_, _) => DeleteStaff()));

        buttons.Controls.Add(
            Btn("Clear", (_, _) => ClearStaff()));

        var left = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        left.Controls.Add(buttons);
        left.Controls.Add(table);

        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(staffGrid);

        page.Controls.Add(split);
        tabs.TabPages.Add(page);
    }

    private static string NextCode(List<Staff> all)
    {
        int max = all
            .Select(s => s.Code)
            .Where(c => c.StartsWith(
                "SBH",
                StringComparison.OrdinalIgnoreCase))
            .Select(c =>
                int.TryParse(c[3..], out int number)
                    ? number
                    : 0)
            .DefaultIfEmpty()
            .Max();

        return $"SBH{max + 1:D3}";
    }

    private void SaveStaff()
    {
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            MessageBox.Show("Please enter the staff member's name.");
            name.Focus();
            return;
        }

        try
        {
            var all = LocalStorage.GetStaff();

            var staff = editingStaffId == null
                ? null
                : all.FirstOrDefault(s => s.Id == editingStaffId);

            if (staff == null)
            {
                staff = new Staff
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Code = NextCode(all)
                };

                all.Add(staff);
            }

            staff.Name = name.Text.Trim();
            staff.Department = department.Text.Trim();
            staff.Designation = designation.Text.Trim();
            staff.Shift = shift.Text.Trim();
            staff.Phone = phone.Text.Trim();
            staff.Active = active.Checked;

            LocalStorage.SaveStaff(all);

            string savedCode = staff.Code;

            ClearStaff();
            RefreshAll();

            MessageBox.Show($"Staff saved. Staff code: {savedCode}");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not save staff:\n" + ex.Message);
        }
    }

    private void EditStaff()
    {
        if (staffGrid.CurrentRow == null)
            return;

        string id = Convert.ToString(
            staffGrid.CurrentRow.Cells["Id"].Value) ?? "";

        var staff = LocalStorage.GetStaff()
            .FirstOrDefault(s => s.Id == id);

        if (staff == null)
            return;

        editingStaffId = staff.Id;

        code.Text = staff.Code;
        name.Text = staff.Name;
        department.Text = staff.Department;
        designation.Text = staff.Designation;
        shift.Text = staff.Shift;
        phone.Text = staff.Phone;
        active.Checked = staff.Active;
    }

    private void DeleteStaff()
    {
        if (staffGrid.CurrentRow == null)
            return;

        string id = Convert.ToString(
            staffGrid.CurrentRow.Cells["Id"].Value) ?? "";

        var all = LocalStorage.GetStaff();
        var staff = all.FirstOrDefault(s => s.Id == id);

        if (staff == null)
            return;

        var result = MessageBox.Show(
            $"Delete {staff.Name} and all of their attendance records?",
            "Confirm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
            return;

        all.RemoveAll(s => s.Id == id);

        var attendance = LocalStorage.GetAttendance();
        attendance.RemoveAll(a => a.StaffId == id);

        LocalStorage.SaveStaff(all);
        LocalStorage.SaveAttendance(attendance);

        ClearStaff();
        RefreshAll();
    }

    private void ClearStaff()
    {
        editingStaffId = null;

        code.Clear();
        name.Clear();
        department.Clear();
        designation.Clear();
        shift.Clear();
        phone.Clear();

        active.Checked = true;
        staffGrid.ClearSelection();
    }

    private void BuildAttendance()
    {
        var page = new TabPage("Daily Attendance");

        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 210,
            Padding = new Padding(8)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4
        };

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 85));

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 45));

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 80));

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 55));

        status.Items.AddRange(new object[]
        {
            "Present",
            "Absent",
            "Half Day",
            "Leave",
            "Weekly Off",
            "Holiday",
            "On Duty"
        });

        status.SelectedIndex = 0;

        Field(table, 0, "Work Date", workDate);
        Field(table, 0, "Staff", staffPicker, 2);
        Field(table, 1, "Status", status);
        Field(table, 1, "In Time", inTime, 2);
        Field(table, 2, "Out Time", outTime);
        Field(table, 2, "Reason", reason, 2);
        Field(table, 3, "Supervisor", supervisor);
        Field(table, 3, "Remarks", remarks, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            WrapContents = true,
            AutoScroll = true
        };

        actions.Controls.Add(
            Btn("Save Attendance", (_, _) => SaveAttendance()));

        actions.Controls.Add(
            Btn("Edit Selected", (_, _) => EditAttendance()));

        actions.Controls.Add(
            Btn("Delete Selected", (_, _) => DeleteAttendance()));

        actions.Controls.Add(
            Btn("Clear Form", (_, _) => ClearAttendance()));

        actions.Controls.Add(
            Btn("Refresh", (_, _) => RefreshAttendance()));

        panel.Controls.Add(table);
        panel.Controls.Add(actions);

        var body = new Panel { Dock = DockStyle.Fill };

        body.Controls.Add(attendanceGrid);
        body.Controls.Add(panel);

        page.Controls.Add(body);
        tabs.TabPages.Add(page);
    }

    private void SaveAttendance()
    {
        if (staffPicker.SelectedItem is not StaffOption selected)
        {
            MessageBox.Show("Add and activate staff first.");
            return;
        }

        string date = workDate.Value.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        try
        {
            var list = LocalStorage.GetAttendance();

            var attendance = list.FirstOrDefault(
                a => a.StaffId == selected.Id &&
                     a.WorkDate == date);

            bool added = attendance == null;

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    StaffId = selected.Id,
                    WorkDate = date
                };

                list.Add(attendance);
            }

            FillAttendance(attendance);

            LocalStorage.SaveAttendance(list);

            ClearAttendance();
            RefreshAll();

            MessageBox.Show(
                added
                    ? "Attendance saved."
                    : "This staff member already had a record for that date; it has been updated.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not save attendance:\n" + ex.Message);
        }
    }

    private void FillAttendance(Attendance attendance)
    {
        attendance.Status =
            Convert.ToString(status.SelectedItem) ?? "Present";

        attendance.InTime = inTime.Text.Trim();
        attendance.OutTime = outTime.Text.Trim();
        attendance.Reason = reason.Text.Trim();
        attendance.Supervisor = supervisor.Text.Trim();
        attendance.Remarks = remarks.Text.Trim();
    }

    private void EditAttendance()
    {
        if (attendanceGrid.CurrentRow == null)
            return;

        string id = Convert.ToString(
            attendanceGrid.CurrentRow.Cells["StaffId"].Value) ?? "";

        string oldDate = Convert.ToString(
            attendanceGrid.CurrentRow.Cells["WorkDate"].Value) ?? "";

        var list = LocalStorage.GetAttendance();

        var original = list.FirstOrDefault(
            a => a.StaffId == id && a.WorkDate == oldDate);

        if (original == null)
        {
            MessageBox.Show("Record not found.");
            return;
        }

        var copy = new Attendance
        {
            StaffId = original.StaffId,
            WorkDate = original.WorkDate,
            Status = original.Status,
            InTime = original.InTime,
            OutTime = original.OutTime,
            Reason = original.Reason,
            Supervisor = original.Supervisor,
            Remarks = original.Remarks
        };

        using var dialog = new EditAttendanceForm(copy);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (list.Any(a =>
                a != original &&
                a.StaffId == copy.StaffId &&
                a.WorkDate == copy.WorkDate))
        {
            MessageBox.Show(
                "An attendance record already exists for this staff member on the selected date. No changes were saved.",
                "Duplicate",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        try
        {
            original.WorkDate = copy.WorkDate;
            original.Status = copy.Status;
            original.InTime = copy.InTime;
            original.OutTime = copy.OutTime;
            original.Reason = copy.Reason;
            original.Supervisor = copy.Supervisor;
            original.Remarks = copy.Remarks;

            LocalStorage.SaveAttendance(list);

            RefreshAll();

            MessageBox.Show("Attendance updated successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not update attendance:\n" + ex.Message);
        }
    }

    private void DeleteAttendance()
    {
        if (attendanceGrid.CurrentRow == null)
            return;

        string id = Convert.ToString(
            attendanceGrid.CurrentRow.Cells["StaffId"].Value) ?? "";

        string date = Convert.ToString(
            attendanceGrid.CurrentRow.Cells["WorkDate"].Value) ?? "";

        var result = MessageBox.Show(
            $"Delete attendance for {date}?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
            return;

        var list = LocalStorage.GetAttendance();

        list.RemoveAll(
            a => a.StaffId == id && a.WorkDate == date);

        LocalStorage.SaveAttendance(list);

        RefreshAll();
    }

    private void ClearAttendance()
    {
        workDate.Value = DateTime.Today;

        if (status.Items.Count > 0)
            status.SelectedIndex = 0;

        inTime.Clear();
        outTime.Clear();
        reason.Clear();
        supervisor.Clear();
        remarks.Clear();

        attendanceGrid.ClearSelection();
    }

    private void RefreshStaffPicker()
    {
        string? oldId =
            (staffPicker.SelectedItem as StaffOption)?.Id;

        var options = LocalStorage.GetStaff()
            .Where(s => s.Active)
            .OrderBy(
                s => s.Department,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                s => s.Name,
                StringComparer.OrdinalIgnoreCase)
            .Select(s => new StaffOption
            {
                Id = s.Id,
                Display =
                    $"{(string.IsNullOrWhiteSpace(s.Department) ? "Unassigned" : s.Department)} | {s.Code} - {s.Name}"
            })
            .ToList();

        staffPicker.DataSource = null;
        staffPicker.DisplayMember = "Display";
        staffPicker.ValueMember = "Id";
        staffPicker.DataSource = options;

        if (oldId != null)
        {
            staffPicker.SelectedItem =
                options.FirstOrDefault(s => s.Id == oldId);
        }
    }

    private void RefreshStaffGrid()
    {
        staffGrid.DataSource = null;

        staffGrid.DataSource = LocalStorage.GetStaff()
            .OrderBy(
                s => s.Department,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                s => s.Name,
                StringComparer.OrdinalIgnoreCase)
            .Select(s => new
            {
                s.Id,
                s.Code,
                s.Name,
                s.Department,
                s.Designation,
                s.Shift,
                s.Phone,
                Active = s.Active ? "Yes" : "No"
            })
            .ToList();

        if (staffGrid.Columns.Contains("Id"))
            staffGrid.Columns["Id"].Visible = false;
    }

    private void RefreshAttendance()
    {
        var people = LocalStorage.GetStaff()
            .ToDictionary(s => s.Id);

        attendanceGrid.DataSource = null;

        attendanceGrid.DataSource = LocalStorage.GetAttendance()
            .Select(a =>
            {
                people.TryGetValue(a.StaffId, out var staff);

                return new
                {
                    a.StaffId,
                    Department =
                        string.IsNullOrWhiteSpace(staff?.Department)
                            ? "Unassigned Department"
                            : staff!.Department,
                    StaffCode = staff?.Code ?? "",
                    StaffName = staff?.Name ?? "(Staff not found)",
                    a.WorkDate,
                    a.Status,
                    a.InTime,
                    a.OutTime,
                    a.Reason,
                    a.Supervisor,
                    a.Remarks
                };
            })
            .OrderBy(x =>
                x.Department.Equals(
                    "Incharge",
                    StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(
                x => x.Department.Equals(
                    "Incharge",
                    StringComparison.OrdinalIgnoreCase)
                    ? ""
                    : x.Department,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                x => x.StaffName,
                StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(x => x.WorkDate)
            .ToList();

        if (attendanceGrid.Columns.Contains("StaffId"))
            attendanceGrid.Columns["StaffId"].Visible = false;
    }

    private void BuildReports()
    {
        var page = new TabPage("Reports & Export");

        fromDate.Value = new DateTime(
            DateTime.Today.Year,
            DateTime.Today.Month,
            1);

        toDate.Value = DateTime.Today;

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(6),
            WrapContents = false,
            AutoScroll = true
        };

        bar.Controls.Add(new Label
        {
            Text = "From:",
            AutoSize = true,
            Margin = new Padding(4, 8, 2, 0)
        });

        bar.Controls.Add(fromDate);

        bar.Controls.Add(new Label
        {
            Text = "To:",
            AutoSize = true,
            Margin = new Padding(10, 8, 2, 0)
        });

        bar.Controls.Add(toDate);

        bar.Controls.Add(
            Btn("Load Report", (_, _) => LoadReport()));

        bar.Controls.Add(
            Btn("Export Excel", (_, _) => ExportExcel()));

        page.Controls.Add(reportGrid);
        page.Controls.Add(bar);

        tabs.TabPages.Add(page);
    }

    private List<AttendanceView> ReportRows()
    {
        if (fromDate.Value.Date > toDate.Value.Date)
        {
            throw new InvalidOperationException(
                "From date cannot be later than To date.");
        }

        var staff = LocalStorage.GetStaff()
            .ToDictionary(s => s.Id);

        return LocalStorage.GetAttendance()
            .Where(a =>
                DateTime.TryParseExact(
                    a.WorkDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date) &&
                date.Date >= fromDate.Value.Date &&
                date.Date <= toDate.Value.Date)
            .Select(a =>
            {
                staff.TryGetValue(a.StaffId, out var person);

                return new AttendanceView
                {
                    WorkDate = a.WorkDate,
                    StaffId = a.StaffId,
                    StaffCode = person?.Code ?? "",
                    StaffName = person?.Name ?? "(Staff not found)",
                    Department =
                        string.IsNullOrWhiteSpace(person?.Department)
                            ? "Unassigned Department"
                            : person!.Department,
                    Designation = person?.Designation ?? "",
                    Status = a.Status,
                    InTime = a.InTime,
                    OutTime = a.OutTime,
                    Reason = a.Reason,
                    Supervisor = a.Supervisor,
                    Remarks = a.Remarks
                };
            })
            .OrderBy(a =>
                a.Department.Equals(
                    "Incharge",
                    StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(
                a => a.Department.Equals(
                    "Incharge",
                    StringComparison.OrdinalIgnoreCase)
                    ? ""
                    : a.Department,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                a => a.StaffName,
                StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(a => a.WorkDate)
            .ToList();
    }

    private void LoadReport()
    {
        try
        {
            var rows = ReportRows();

            reportGrid.DataSource = rows
                .Select(a => new
                {
                    a.WorkDate,
                    a.Department,
                    a.StaffCode,
                    a.StaffName,
                    a.Designation,
                    a.Status,
                    a.InTime,
                    a.OutTime,
                    a.Reason,
                    a.Supervisor,
                    a.Remarks
                })
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Report",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void ExportExcel()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"Attendance-{DateTime.Today:yyyyMMdd}.xlsx"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var rows = ReportRows();

            using var workbook = new XLWorkbook();

            var sheet = workbook.Worksheets.Add("Attendance");

            string[] headers =
            {
                "Date",
                "Department",
                "Staff Code",
                "Staff Name",
                "Designation",
                "Status",
                "In Time",
                "Out Time",
                "Reason",
                "Supervisor",
                "Remarks"
            };

            for (int column = 0; column < headers.Length; column++)
                sheet.Cell(1, column + 1).Value = headers[column];

            for (int row = 0; row < rows.Count; row++)
            {
                var attendance = rows[row];

                string[] values =
                {
                    attendance.WorkDate,
                    attendance.Department,
                    attendance.StaffCode,
                    attendance.StaffName,
                    attendance.Designation,
                    attendance.Status,
                    attendance.InTime,
                    attendance.OutTime,
                    attendance.Reason,
                    attendance.Supervisor,
                    attendance.Remarks
                };

                for (int column = 0; column < values.Length; column++)
                {
                    sheet.Cell(row + 2, column + 1).Value =
                        values[column] ?? "";
                }
            }

            sheet.Row(1).Style.Font.Bold = true;

            sheet.Row(1).Style.Fill.BackgroundColor =
                XLColor.LightGray;

            sheet.SheetView.FreezeRows(1);
            sheet.Columns().AdjustToContents();

            workbook.SaveAs(dialog.FileName);

            MessageBox.Show(
                "Excel exported:\n" + dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Export failed:\n" + ex.Message);
        }
    }

    private void BuildBackup()
    {
        var page = new TabPage("Backup & Restore");

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(16),
            WrapContents = true
        };

        panel.Controls.Add(new Label
        {
            Text = "Data folder:\n" +
                   LocalStorage.DataDirectory +
                   "\n\nBackups are stored in Documents\\StaffAttendanceBackups.",
            AutoSize = true,
            Margin = new Padding(4, 4, 20, 20)
        });

        panel.Controls.Add(Btn("Create Backup", (_, _) =>
        {
            try
            {
                MessageBox.Show(
                    "Backup created:\n" +
                    LocalStorage.CreateBackup());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }));

        panel.Controls.Add(
            Btn("Restore Backup", (_, _) => Restore()));

        panel.Controls.Add(Btn("Open Data Folder", (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LocalStorage.DataDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }));

        page.Controls.Add(panel);
        tabs.TabPages.Add(page);
    }

    private void Restore()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description =
                "Select a backup folder containing staff.json and attendance.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var result = MessageBox.Show(
            "Current data will be backed up, then replaced. Continue?",
            "Confirm Restore",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        try
        {
            LocalStorage.RestoreBackup(dialog.SelectedPath);

            ClearStaff();
            ClearAttendance();
            RefreshAll();

            MessageBox.Show("Backup restored.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Restore failed:\n" + ex.Message);
        }
    }

    private void RefreshDashboard()
    {
        var staff = LocalStorage.GetStaff();

        string today = DateTime.Today.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        var attendance = LocalStorage.GetAttendance()
            .Where(a => a.WorkDate == today)
            .ToList();

        dashboard.Text =
            $"STAFF ATTENDANCE MANAGER\n\n" +
            $"Total staff: {staff.Count}     " +
            $"Active staff: {staff.Count(s => s.Active)}\n" +
            $"Today's attendance records: {attendance.Count}\n" +
            $"Present: {attendance.Count(a => a.Status == "Present")}     " +
            $"Absent: {attendance.Count(a => a.Status == "Absent")}     " +
            $"Leave: {attendance.Count(a => a.Status.Contains("Leave", StringComparison.OrdinalIgnoreCase))}\n\n" +
            $"Data folder: {LocalStorage.DataDirectory}";
    }

    private void RefreshAll()
    {
        RefreshDashboard();
        RefreshStaffGrid();
        RefreshStaffPicker();
        RefreshAttendance();
        LoadReport();
    }
}
