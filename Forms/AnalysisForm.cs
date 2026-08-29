using Microsoft.EntityFrameworkCore;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;
public sealed class AnalysisForm : Form
{
    private readonly DataGridView grid = Ui.Grid(); private readonly AppUser user;
    public AnalysisForm(AppUser user) { this.user = user; Ui.StyleDialog(this, "Forensic Analysis | TraceLock", new Size(1200, 700), new Size(900, 560)); var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Button("New analysis", (_, _) => Edit(null), true)); bar.Controls.Add(Ui.Button("Edit selected", (_, _) => Selected())); grid.DoubleClick += (_, _) => Selected(); RefreshData(); }
    private void RefreshData() { using var db = Database.Create(); grid.DataSource = db.ForensicAnalyses.Include(x => x.EvidenceItem).OrderByDescending(x => x.UpdatedAt).Select(x => new { Id = x.Id, Evidence = x.EvidenceItem == null ? "" : x.EvidenceItem.EvidenceNumber, Analyst = x.Analyst, Status = x.Status, Tools = x.ToolsUsed, Started = x.StartedAt, Completed = x.CompletedAt, Findings = x.Findings }).ToList(); Ui.HideColumn(grid, "Id"); }
    private void Selected() { if (grid.CurrentRow?.Cells["Id"].Value is int id) Edit(id); }
    private void Edit(int? id) { using var f = new AnalysisEditForm(id, user); if (f.ShowDialog(this) == DialogResult.OK) RefreshData(); }
}
sealed class AnalysisEditForm : Form
{
    private readonly int? id; private readonly AppUser user; private readonly ComboBox evidence = Ui.Combo(), status = Ui.Combo("Pending", "In Progress", "Completed", "Needs Review"); private readonly TextBox analyst = Ui.TextBox(), tools = Ui.TextBox(), findings = Ui.TextBox(true), notes = Ui.TextBox(true), report = Ui.TextBox(); private readonly DateTimePicker started = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy HH:mm" };
    public AnalysisEditForm(int? id, AppUser user)
    {
        this.id = id; this.user = user; Ui.StyleDialog(this, id.HasValue ? "Edit forensic analysis" : "New forensic analysis", new Size(820, 740), new Size(650, 580)); Ui.DialogBody(this, out var body, out var footer);
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2 }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); body.Controls.Add(t);
        Add(t, "Evidence", evidence); Add(t, "Analyst", analyst); Add(t, "Status", status); Add(t, "Tools used", tools); Add(t, "Started", started); Add(t, "Report reference", report); findings.Height = 140; notes.Height = 120; Add(t, "Findings", findings, 160); Add(t, "Notes", notes, 140);
        var cancel = Ui.Button("Cancel", (_, _) => Close()); var save = Ui.Button("Save analysis", Save, true); footer.Controls.Add(cancel); footer.Controls.Add(save); AcceptButton = save;
        using var db = Database.Create(); foreach (var e in db.Evidence.OrderBy(x => x.EvidenceNumber)) evidence.Items.Add(new Choice(e.Id, $"{e.EvidenceNumber} — {e.Name}")); if (evidence.Items.Count > 0) evidence.SelectedIndex = 0; analyst.Text = user.FullName; if (id.HasValue) LoadData();
    }
    private static void Add(TableLayoutPanel t, string label, Control c, int height = 56) { var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, height)); t.Controls.Add(Ui.Label(label), 0, row); c.Dock = DockStyle.Fill; c.Margin = new Padding(6, 4, 0, 4); t.Controls.Add(c, 1, row); }
    private void LoadData() { using var db = Database.Create(); var a = db.ForensicAnalyses.Find(id!.Value); if (a is null) return; analyst.Text = a.Analyst; status.Text = a.Status; tools.Text = a.ToolsUsed; findings.Text = a.Findings; notes.Text = a.Notes; report.Text = a.ReportReference; started.Value = a.StartedAt; for (var i = 0; i < evidence.Items.Count; i++) if (evidence.Items[i] is Choice c && c.Id == a.EvidenceItemId) evidence.SelectedIndex = i; }
    private void Save(object? s, EventArgs e)
    {
        if (evidence.SelectedItem is not Choice c || string.IsNullOrWhiteSpace(analyst.Text)) { MessageBox.Show(this, "Evidence and analyst are required."); return; }
        using var db = Database.Create(); var a = id.HasValue ? db.ForensicAnalyses.Find(id.Value) : null; if (a is null) { a = new ForensicAnalysis(); db.ForensicAnalyses.Add(a); }
        a.EvidenceItemId = c.Id; a.Analyst = analyst.Text.Trim(); a.Status = status.Text; a.ToolsUsed = tools.Text.Trim(); a.Findings = findings.Text.Trim(); a.Notes = notes.Text.Trim(); a.ReportReference = report.Text.Trim(); a.StartedAt = started.Value; a.CompletedAt = a.Status == "Completed" ? DateTime.Now : null; a.UpdatedAt = DateTime.Now; db.SaveChanges(); AuditService.Record(id.HasValue ? "Forensic analysis updated" : "Forensic analysis created", $"Analysis for {c.Text} is {a.Status}.", "ForensicAnalysis", a.Id.ToString()); DialogResult = DialogResult.OK; Close();
    }
    private sealed record Choice(int Id, string Text) { public override string ToString() => Text; }
}
