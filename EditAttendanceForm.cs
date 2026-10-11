using System.Globalization;
namespace StaffAttendanceManager;
public sealed class EditAttendanceForm : Form
{
    private readonly Attendance item;
    private readonly DateTimePicker date = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd-MMM-yyyy" };
    private readonly ComboBox status = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly CheckBox useIn = new() { Text = "Set in time", AutoSize = true };
    private readonly CheckBox useOut = new() { Text = "Set out time", AutoSize = true };
    private readonly DateTimePicker inTime = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "hh:mm tt", ShowUpDown = true, Width = 120 };
    private readonly DateTimePicker outTime = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "hh:mm tt", ShowUpDown = true, Width = 120 };
    private readonly TextBox reason = new(), supervisor = new(), remarks = new();
    public EditAttendanceForm(Attendance attendance)
    {
        item = attendance ?? throw new ArgumentNullException(nameof(attendance));
        Text = "Edit Attendance"; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; ClientSize = new Size(500, 490); Font = new Font("Segoe UI", 9.5F);
        BuildUI(); LoadItem();
    }
    private void BuildUI()
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 8 };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i=0;i<7;i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); t.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        status.Items.AddRange(new object[]{"Present","Absent","Half Day","Leave","Weekly Off","Holiday","On Duty"});
        Add(t,0,"Attendance Date",date); Add(t,1,"Status",status);
        var ip = new FlowLayoutPanel { Dock=DockStyle.Fill, WrapContents=false }; ip.Controls.Add(useIn); ip.Controls.Add(inTime); Add(t,2,"In Time",ip);
        var op = new FlowLayoutPanel { Dock=DockStyle.Fill, WrapContents=false }; op.Controls.Add(useOut); op.Controls.Add(outTime); Add(t,3,"Out Time",op);
        Add(t,4,"Reason",reason); Add(t,5,"Supervisor",supervisor); Add(t,6,"Remarks",remarks);
        var buttons = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.RightToLeft, WrapContents=false };
        var save = new Button { Text="Save Changes", Width=125, Height=34 }; var cancel = new Button { Text="Cancel", Width=90, Height=34, DialogResult=DialogResult.Cancel };
        save.Click += SaveChanges; cancel.Click += (_,_)=>Close(); buttons.Controls.Add(save); buttons.Controls.Add(cancel); t.Controls.Add(buttons,0,7); t.SetColumnSpan(buttons,2); Controls.Add(t); AcceptButton=save; CancelButton=cancel;
        useIn.CheckedChanged += (_,_)=>inTime.Enabled=useIn.Checked; useOut.CheckedChanged += (_,_)=>outTime.Enabled=useOut.Checked;
    }
    private static void Add(TableLayoutPanel t,int row,string label,Control c) { t.Controls.Add(new Label { Text=label, Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleLeft },0,row); c.Dock=DockStyle.Fill; t.Controls.Add(c,1,row); }
    private void LoadItem()
    {
        if(DateTime.TryParseExact(item.WorkDate,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var d)) date.Value=d;
        string s=string.IsNullOrWhiteSpace(item.Status)?"Present":item.Status; if(!status.Items.Contains(s)) status.Items.Add(s); status.Text=s;
        LoadTime(item.InTime,inTime,useIn); LoadTime(item.OutTime,outTime,useOut); reason.Text=item.Reason; supervisor.Text=item.Supervisor; remarks.Text=item.Remarks;
    }
    private static void LoadTime(string? value,DateTimePicker picker,CheckBox check)
    {
        if(DateTime.TryParse(value,CultureInfo.CurrentCulture,DateTimeStyles.None,out var parsed)){picker.Value=DateTime.Today.Add(parsed.TimeOfDay);check.Checked=true;} else {picker.Value=DateTime.Today;check.Checked=false;}
        picker.Enabled=check.Checked;
    }
    private void SaveChanges(object? sender,EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(status.Text)){MessageBox.Show("Please enter or select an attendance status.","Validation",MessageBoxButtons.OK,MessageBoxIcon.Warning);status.Focus();return;}
        item.WorkDate=date.Value.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture); item.Status=status.Text.Trim();
        item.InTime=useIn.Checked?inTime.Value.ToString("hh:mm tt",CultureInfo.InvariantCulture):""; item.OutTime=useOut.Checked?outTime.Value.ToString("hh:mm tt",CultureInfo.InvariantCulture):"";
        item.Reason=reason.Text.Trim();item.Supervisor=supervisor.Text.Trim();item.Remarks=remarks.Text.Trim();DialogResult=DialogResult.OK;Close();
    }
}