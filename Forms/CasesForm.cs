using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;

public sealed class CasesForm : Form
{
    private readonly AppUser user; private readonly DataGridView grid = Ui.Grid(); private readonly TextBox search = Ui.TextBox();
    public CasesForm(AppUser user)
    {
        this.user = user; Ui.StyleDialog(this, "Cases | TraceLock", new Size(1180, 700), new Size(900, 560));
        var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar);
        bar.Controls.Add(Ui.Label("Search")); search.Width = 260; bar.Controls.Add(search);
        bar.Controls.Add(Ui.Button("New case", (_, _) => EditCase(null), true)); bar.Controls.Add(Ui.Button("Edit selected", (_, _) => EditSelected())); bar.Controls.Add(Ui.Button("Close case", (_, _) => CloseSelected()));
        search.TextChanged += (_, _) => RefreshData(); grid.DoubleClick += (_, _) => EditSelected(); RefreshData();
    }
    private void RefreshData()
    {
        using var db = Database.Create(); var q = search.Text.Trim();
        grid.DataSource = db.Cases.Where(x => string.IsNullOrEmpty(q) || x.CaseNumber.Contains(q) || x.Title.Contains(q) || x.Investigator.Contains(q)).OrderByDescending(x => x.UpdatedAt).Select(x => new { x.Id, Case = x.CaseNumber, Title = x.Title, Type = x.CaseType, Priority = x.Priority, Status = x.Status, Investigator = x.Investigator, Opened = x.OpenedOn }).ToList(); Ui.HideColumn(grid, "Id");
    }
    private int? SelectedId() => grid.CurrentRow?.Cells["Id"].Value is int id ? id : null;
    private void EditSelected() { if (SelectedId() is int id) EditCase(id); }
    private void EditCase(int? id) { using var f = new CaseEditForm(id, user); if (f.ShowDialog(this) == DialogResult.OK) RefreshData(); }
    private void CloseSelected()
    {
        if (SelectedId() is not int id) return; using var db = Database.Create(); var c = db.Cases.Find(id); if (c is null) return;
        if (MessageBox.Show(this, $"Mark {c.CaseNumber} as closed?", "Close case", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        c.Status = "Closed"; c.ClosedOn = DateTime.Now; c.UpdatedAt = DateTime.Now; db.SaveChanges(); AuditService.Record("Case closed", $"{c.CaseNumber} was closed.", "Case", c.Id.ToString()); RefreshData();
    }
}

sealed class CaseEditForm : Form
{
    private readonly int? id; private readonly AppUser user;
    private readonly TextBox number = Ui.TextBox(), title = Ui.TextBox(), investigator = Ui.TextBox(), description = Ui.TextBox(true);
    private readonly ComboBox type = Ui.Combo("Cybercrime", "Device Examination", "Incident Response", "Fraud", "Other"), status = Ui.Combo("Open", "In Progress", "Pending Review", "Closed"), priority = Ui.Combo("Low", "Medium", "High", "Critical");
    private readonly DateTimePicker opened = new() { Format = DateTimePickerFormat.Short };
    public CaseEditForm(int? id, AppUser user)
    {
        this.id = id; this.user = user; Ui.StyleDialog(this, id.HasValue ? "Edit case" : "New case", new Size(760, 650), new Size(620, 560));
        Ui.DialogBody(this, out var body, out var footer);
        var table = FormTable(145); body.Controls.Add(table);
        Add(table, "Case number", number); Add(table, "Title", title); Add(table, "Case type", type); Add(table, "Status", status); Add(table, "Priority", priority); Add(table, "Investigator", investigator); Add(table, "Opened", opened); description.Height = 120; Add(table, "Description", description, 140);
        var cancel = Ui.Button("Cancel", (_, _) => Close()); var save = Ui.Button("Save case", Save, true); footer.Controls.Add(cancel); footer.Controls.Add(save); AcceptButton = save;
        if (id.HasValue) LoadData(); else investigator.Text = user.FullName;
    }
    private static TableLayoutPanel FormTable(int labelWidth) { var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, Padding = Padding.Empty }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); return t; }
    private static void Add(TableLayoutPanel t, string label, Control control, int height = 56) { var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, height)); t.Controls.Add(Ui.Label(label), 0, row); control.Dock = DockStyle.Fill; control.Margin = new Padding(6, 4, 0, 4); t.Controls.Add(control, 1, row); }
    private void LoadData() { using var db = Database.Create(); var c = db.Cases.Find(id!.Value); if (c is null) return; number.Text = c.CaseNumber; title.Text = c.Title; type.Text = c.CaseType; status.Text = c.Status; priority.Text = c.Priority; investigator.Text = c.Investigator; opened.Value = c.OpenedOn; description.Text = c.Description; }
    private void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(number.Text) || string.IsNullOrWhiteSpace(title.Text)) { MessageBox.Show(this, "Case number and title are required."); return; }
        using var db = Database.Create(); var c = id.HasValue ? db.Cases.Find(id.Value) : null; if (c is null) { c = new CaseFile(); db.Cases.Add(c); }
        c.CaseNumber = number.Text.Trim(); c.Title = title.Text.Trim(); c.CaseType = type.Text; c.Status = status.Text; c.Priority = priority.Text; c.Investigator = investigator.Text.Trim(); c.OpenedOn = opened.Value.Date; c.Description = description.Text.Trim(); c.UpdatedAt = DateTime.Now; if (c.Id == 0) c.CreatedAt = DateTime.Now;
        try { db.SaveChanges(); AuditService.Record(id.HasValue ? "Case updated" : "Case created", $"{c.CaseNumber} — {c.Title}.", "Case", c.Id.ToString()); DialogResult = DialogResult.OK; Close(); } catch (Exception ex) { MessageBox.Show(this, "The case could not be saved.\n\n" + ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
