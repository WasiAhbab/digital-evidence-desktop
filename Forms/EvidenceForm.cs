using Microsoft.EntityFrameworkCore;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;

public sealed class EvidenceForm : Form
{
    private readonly AppUser user; private readonly DataGridView grid = Ui.Grid(); private readonly TextBox search = Ui.TextBox();
    public EvidenceForm(AppUser user)
    {
        this.user = user; Ui.StyleDialog(this, "Evidence Registry | TraceLock", new Size(1260, 720), new Size(950, 580));
        var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Label("Search")); search.Width = 260; bar.Controls.Add(search);
        bar.Controls.Add(Ui.Button("Add evidence", (_, _) => Edit(null), true)); bar.Controls.Add(Ui.Button("Edit selected", (_, _) => EditSelected())); bar.Controls.Add(Ui.Button("Verify integrity", (_, _) => Verify())); bar.Controls.Add(Ui.Button("Files", (_, _) => Files()));
        search.TextChanged += (_, _) => RefreshData(); grid.DoubleClick += (_, _) => EditSelected(); RefreshData();
    }
    private void RefreshData() { using var db = Database.Create(); var q = search.Text.Trim(); grid.DataSource = db.Evidence.Include(x => x.CaseFile).Where(x => string.IsNullOrEmpty(q) || x.EvidenceNumber.Contains(q) || x.Name.Contains(q) || x.Type.Contains(q)).OrderByDescending(x => x.UpdatedAt).Select(x => new { x.Id, Evidence = x.EvidenceNumber, Name = x.Name, Type = x.Type, Case = x.CaseFile == null ? "" : x.CaseFile.CaseNumber, Status = x.Status, Custodian = x.CurrentCustodian, Integrity = x.IntegrityStatus, SHA256 = x.HashSha256, Collected = x.CollectedOn }).ToList(); Ui.HideColumn(grid, "Id"); }
    private int? Id() => grid.CurrentRow?.Cells["Id"].Value is int x ? x : null;
    private void EditSelected() { if (Id() is int x) Edit(x); }
    private void Edit(int? id) { using var f = new EvidenceEditForm(id, user); if (f.ShowDialog(this) == DialogResult.OK) RefreshData(); }
    private void Files() { if (Id() is int x) { using var f = new FilesForm(user, x); f.ShowDialog(this); RefreshData(); } }
    private void Verify()
    {
        if (Id() is not int x) return; using var db = Database.Create(); var e = db.Evidence.SingleOrDefault(a => a.Id == x); if (e is null) return;
        var file = db.EvidenceFiles.Where(a => a.EvidenceItemId == x).OrderByDescending(a => a.UploadedAt).FirstOrDefault(); if (file is null) { MessageBox.Show(this, "No stored file is attached to this evidence item."); return; }
        var path = Path.Combine(AppPaths.BaseDirectory, file.FilePath); if (!File.Exists(path)) { MessageBox.Show(this, "The stored file could not be found.", "Integrity check", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var stream = File.OpenRead(path); var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)); e.IntegrityStatus = hash.Equals(file.Sha256, StringComparison.OrdinalIgnoreCase) ? "Verified" : "Mismatch"; e.HashSha256 = hash; e.HashGeneratedAt = DateTime.Now; e.UpdatedAt = DateTime.Now; db.SaveChanges(); AuditService.Record("Evidence integrity verified", $"{e.EvidenceNumber}: {e.IntegrityStatus}.", "Evidence", e.Id.ToString()); MessageBox.Show(this, $"SHA-256:\n{hash}\n\nStatus: {e.IntegrityStatus}", "Integrity result", MessageBoxButtons.OK, e.IntegrityStatus == "Verified" ? MessageBoxIcon.Information : MessageBoxIcon.Warning); RefreshData();
    }
}

sealed class EvidenceEditForm : Form
{
    private readonly int? id; private readonly AppUser user;
    private readonly TextBox no = Ui.TextBox(), name = Ui.TextBox(), desc = Ui.TextBox(true), device = Ui.TextBox(), person = Ui.TextBox(), storage = Ui.TextBox(), location = Ui.TextBox(), collector = Ui.TextBox(), custodian = Ui.TextBox(), notes = Ui.TextBox(true);
    private readonly ComboBox type = Ui.Combo("Document", "Hard drive", "Mobile phone", "Network capture", "Image", "Video", "Other"), status = Ui.Combo("Received", "In Examination", "Reviewed", "Released", "Retired"), cases = Ui.Combo();
    private readonly DateTimePicker collected = new() { Format = DateTimePickerFormat.Short };
    public EvidenceEditForm(int? id, AppUser user)
    {
        this.id = id; this.user = user; Ui.StyleDialog(this, id.HasValue ? "Edit evidence" : "Register evidence", new Size(860, 760), new Size(700, 580));
        Ui.DialogBody(this, out var body, out var footer); var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2 }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); body.Controls.Add(t);
        Add(t, "Evidence number", no); Add(t, "Name", name); Add(t, "Type", type); Add(t, "Status", status); Add(t, "Case", cases); Add(t, "Source device", device); Add(t, "Associated person", person); Add(t, "Storage location", storage); Add(t, "Collection location", location); Add(t, "Collector", collector); Add(t, "Current custodian", custodian); Add(t, "Collected on", collected); desc.Height = 110; notes.Height = 110; Add(t, "Description", desc, 130); Add(t, "Notes", notes, 130);
        var cancel = Ui.Button("Cancel", (_, _) => Close()); var save = Ui.Button("Save evidence", Save, true); footer.Controls.Add(cancel); footer.Controls.Add(save); AcceptButton = save;
        LoadCases(); if (id.HasValue) LoadData(); else custodian.Text = user.FullName;
    }
    private static void Add(TableLayoutPanel t, string label, Control control, int height = 56) { var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, height)); t.Controls.Add(Ui.Label(label), 0, row); control.Dock = DockStyle.Fill; control.Margin = new Padding(6, 4, 0, 4); t.Controls.Add(control, 1, row); }
    private void LoadCases() { using var db = Database.Create(); cases.Items.Clear(); cases.Items.Add("No case"); foreach (var c in db.Cases.OrderBy(x => x.CaseNumber)) cases.Items.Add(new CaseChoice(c.Id, c.CaseNumber + " — " + c.Title)); cases.SelectedIndex = 0; }
    private void LoadData() { using var db = Database.Create(); var e = db.Evidence.Find(id!.Value); if (e is null) return; no.Text = e.EvidenceNumber; name.Text = e.Name; type.Text = e.Type; status.Text = e.Status; device.Text = e.SourceDevice; person.Text = e.AssociatedPerson; storage.Text = e.StorageLocation; location.Text = e.CollectionLocation; collector.Text = e.Collector; custodian.Text = e.CurrentCustodian; collected.Value = e.CollectedOn; desc.Text = e.Description; notes.Text = e.Notes; if (e.CaseFileId is int cid) for (var i = 0; i < cases.Items.Count; i++) if (cases.Items[i] is CaseChoice cc && cc.Id == cid) cases.SelectedIndex = i; }
    private void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(no.Text) || string.IsNullOrWhiteSpace(name.Text)) { MessageBox.Show(this, "Evidence number and name are required."); return; }
        using var db = Database.Create(); var item = id.HasValue ? db.Evidence.Find(id.Value) : null; if (item is null) { item = new EvidenceItem(); db.Evidence.Add(item); }
        item.EvidenceNumber = no.Text.Trim(); item.Name = name.Text.Trim(); item.Type = type.Text; item.Status = status.Text; item.SourceDevice = device.Text.Trim(); item.AssociatedPerson = person.Text.Trim(); item.StorageLocation = storage.Text.Trim(); item.CollectionLocation = location.Text.Trim(); item.Collector = collector.Text.Trim(); item.CurrentCustodian = custodian.Text.Trim(); item.CollectedOn = collected.Value.Date; item.Description = desc.Text.Trim(); item.Notes = notes.Text.Trim(); item.CaseFileId = cases.SelectedItem is CaseChoice cc ? cc.Id : null; item.UpdatedAt = DateTime.Now;
        try { db.SaveChanges(); AuditService.Record(id.HasValue ? "Evidence updated" : "Evidence registered", $"{item.EvidenceNumber} — {item.Name}.", "Evidence", item.Id.ToString()); DialogResult = DialogResult.OK; Close(); } catch (Exception ex) { MessageBox.Show(this, "The evidence could not be saved.\n\n" + ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private sealed record CaseChoice(int Id, string Text) { public override string ToString() => Text; }
}
