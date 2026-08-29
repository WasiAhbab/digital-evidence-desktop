using Microsoft.EntityFrameworkCore;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;
public sealed class CustodyForm : Form
{
    private readonly DataGridView grid = Ui.Grid(); private readonly AppUser user;
    public CustodyForm(AppUser user) { this.user = user; Ui.StyleDialog(this, "Chain of Custody | TraceLock", new Size(1200, 700), new Size(900, 560)); var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Button("Record transfer", (_, _) => NewTransfer(), true)); bar.Controls.Add(Ui.Button("Refresh", (_, _) => RefreshData())); RefreshData(); }
    private void RefreshData() { using var db = Database.Create(); grid.DataSource = db.CustodyRecords.Include(x => x.EvidenceItem).OrderByDescending(x => x.TransferredAt).Select(x => new { Id = x.Id, Evidence = x.EvidenceItem == null ? "" : x.EvidenceItem.EvidenceNumber, From = x.FromPerson, To = x.ToPerson, Purpose = x.Purpose, Location = x.Location, Condition = x.Condition, Transferred = x.TransferredAt, Recorded = x.RecordedAt }).ToList(); Ui.HideColumn(grid, "Id"); }
    private void NewTransfer() { using var f = new CustodyEditForm(user); if (f.ShowDialog(this) == DialogResult.OK) RefreshData(); }
}
sealed class CustodyEditForm : Form
{
    private readonly AppUser user; private readonly ComboBox evidence = Ui.Combo(), condition = Ui.Combo("Sealed / intact", "Good", "Damaged", "Opened for examination"); private readonly TextBox from = Ui.TextBox(), to = Ui.TextBox(), purpose = Ui.TextBox(), location = Ui.TextBox(), authorization = Ui.TextBox(), notes = Ui.TextBox(true); private readonly DateTimePicker transferred = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd MMM yyyy HH:mm" };
    public CustodyEditForm(AppUser user)
    {
        this.user = user; Ui.StyleDialog(this, "Record chain-of-custody transfer", new Size(760, 680), new Size(620, 560)); Ui.DialogBody(this, out var body, out var footer);
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2 }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); body.Controls.Add(t);
        Add(t, "Evidence", evidence); Add(t, "From", from); Add(t, "To", to); Add(t, "Purpose", purpose); Add(t, "Location", location); Add(t, "Condition", condition); Add(t, "Authorization", authorization); Add(t, "Transferred at", transferred); notes.Height = 120; Add(t, "Notes", notes, 140);
        var cancel = Ui.Button("Cancel", (_, _) => Close()); var save = Ui.Button("Record transfer", Save, true); footer.Controls.Add(cancel); footer.Controls.Add(save); AcceptButton = save;
        using var db = Database.Create(); foreach (var e in db.Evidence.OrderBy(x => x.EvidenceNumber)) evidence.Items.Add(new Choice(e.Id, $"{e.EvidenceNumber} — {e.Name}")); if (evidence.Items.Count > 0) evidence.SelectedIndex = 0; from.Text = user.FullName;
    }
    private static void Add(TableLayoutPanel t, string label, Control c, int height = 56) { var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, height)); t.Controls.Add(Ui.Label(label), 0, row); c.Dock = DockStyle.Fill; c.Margin = new Padding(6, 4, 0, 4); t.Controls.Add(c, 1, row); }
    private void Save(object? s, EventArgs e)
    {
        if (evidence.SelectedItem is not Choice c || string.IsNullOrWhiteSpace(to.Text) || string.IsNullOrWhiteSpace(purpose.Text)) { MessageBox.Show(this, "Evidence, receiving person and purpose are required."); return; }
        using var db = Database.Create(); var item = db.Evidence.Find(c.Id); if (item is null) { MessageBox.Show(this, "The selected evidence no longer exists."); return; }
        db.CustodyRecords.Add(new CustodyRecord { EvidenceItemId = c.Id, FromPerson = from.Text.Trim(), ToPerson = to.Text.Trim(), Purpose = purpose.Text.Trim(), Location = location.Text.Trim(), Condition = condition.Text, Authorization = authorization.Text.Trim(), Notes = notes.Text.Trim(), TransferredAt = transferred.Value, RecordedAt = DateTime.Now }); item.CurrentCustodian = to.Text.Trim(); item.UpdatedAt = DateTime.Now; db.SaveChanges(); AuditService.Record("Chain-of-custody transfer recorded", $"{item.EvidenceNumber} transferred from {from.Text.Trim()} to {to.Text.Trim()}.", "Custody", item.Id.ToString()); DialogResult = DialogResult.OK; Close();
    }
    private sealed record Choice(int Id, string Text) { public override string ToString() => Text; }
}
