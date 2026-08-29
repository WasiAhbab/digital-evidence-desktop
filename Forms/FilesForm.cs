using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;
public sealed class FilesForm : Form
{
    private readonly AppUser user; private readonly int? evidenceId; private readonly DataGridView grid = Ui.Grid(); private readonly Label evidenceLabel = new();
    public FilesForm(AppUser user, int? evidenceId = null)
    {
        this.user = user; this.evidenceId = evidenceId; Ui.StyleDialog(this, "Evidence Files & Integrity | TraceLock", new Size(1120, 680), new Size(850, 560));
        var bar = Ui.Toolbar(82); Controls.Add(grid); Controls.Add(bar); var info = new Panel { Width = 420, Height = 62, Margin = new Padding(0, 0, 14, 0) }; info.Controls.Add(new Label { Text = "EVIDENCE", AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI Semibold", 7.8f), Location = new Point(4, 6) }); evidenceLabel.AutoSize = false; evidenceLabel.AutoEllipsis = true; evidenceLabel.ForeColor = Ui.Text; evidenceLabel.Font = new Font("Segoe UI Semibold", 9.5f); evidenceLabel.Location = new Point(4, 24); evidenceLabel.Size = new Size(400, 28); info.Controls.Add(evidenceLabel); bar.Controls.Add(info);
        bar.Controls.Add(Ui.Button("Attach file", (_, _) => Attach(), true)); bar.Controls.Add(Ui.Button("Verify selected", (_, _) => Verify())); bar.Controls.Add(Ui.Button("Open selected", (_, _) => OpenSelected())); RefreshData();
    }
    private void RefreshData()
    {
        using var db = Database.Create();
        if (evidenceId.HasValue) { var e = db.Evidence.Find(evidenceId.Value); evidenceLabel.Text = e is null ? "Unknown evidence" : $"{e.EvidenceNumber} — {e.Name}"; grid.DataSource = db.EvidenceFiles.Where(x => x.EvidenceItemId == evidenceId.Value).OrderByDescending(x => x.UploadedAt).Select(x => new { Id = x.Id, File = x.FileName, Size = x.SizeBytes, Hash = x.Sha256, UploadedBy = x.UploadedBy, Uploaded = x.UploadedAt }).ToList(); }
        else { evidenceLabel.Text = "All stored evidence files"; grid.DataSource = db.EvidenceFiles.Include(x => x.EvidenceItem).OrderByDescending(x => x.UploadedAt).Select(x => new { Id = x.Id, Evidence = x.EvidenceItem == null ? "" : x.EvidenceItem.EvidenceNumber, File = x.FileName, Size = x.SizeBytes, Hash = x.Sha256, UploadedBy = x.UploadedBy, Uploaded = x.UploadedAt }).ToList(); }
        Ui.HideColumn(grid, "Id");
    }
    private void Attach()
    {
        if (!evidenceId.HasValue) { MessageBox.Show(this, "Open Files from an evidence record to attach a file."); return; }
        using var db = Database.Create(); var e = db.Evidence.Find(evidenceId.Value); if (e is null) return; using var dialog = new OpenFileDialog { Filter = "Evidence files|*.pdf;*.txt;*.csv;*.jpg;*.jpeg;*.png;*.mp4;*.wav;*.docx;*.zip|All files|*.*", Title = "Select evidence file" }; if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var info = new FileInfo(dialog.FileName); if (info.Length > 50 * 1024 * 1024) { MessageBox.Show(this, "Maximum file size is 50 MB."); return; }
        var folder = Path.Combine(AppPaths.EvidenceStorage, e.EvidenceNumber); Directory.CreateDirectory(folder); var stored = $"{Guid.NewGuid():N}{info.Extension.ToLowerInvariant()}"; var path = Path.Combine(folder, stored); File.Copy(dialog.FileName, path, true);
        using var stream = File.OpenRead(path); var hash = Convert.ToHexString(SHA256.HashData(stream)); db.EvidenceFiles.Add(new EvidenceFile { EvidenceItemId = e.Id, FileName = info.Name, ContentType = "application/octet-stream", FilePath = Path.GetRelativePath(AppPaths.BaseDirectory, path), SizeBytes = info.Length, Sha256 = hash, UploadedBy = user.FullName }); e.HashSha256 = hash; e.HashGeneratedAt = DateTime.Now; e.IntegrityStatus = "Verified"; e.UpdatedAt = DateTime.Now; db.SaveChanges(); AuditService.Record("Evidence file attached", $"{info.Name} attached to {e.EvidenceNumber}; SHA-256 recorded.", "EvidenceFile", e.Id.ToString()); RefreshData();
    }
    private int? Id() => grid.CurrentRow?.Cells["Id"].Value is int x ? x : null;
    private void Verify()
    {
        if (Id() is not int id) return; using var db = Database.Create(); var f = db.EvidenceFiles.Include(x => x.EvidenceItem).SingleOrDefault(x => x.Id == id); if (f is null) return; var path = Path.Combine(AppPaths.BaseDirectory, f.FilePath); if (!File.Exists(path)) { MessageBox.Show(this, "Stored file is missing."); return; }
        using var s = File.OpenRead(path); var actual = Convert.ToHexString(SHA256.HashData(s)); var ok = actual.Equals(f.Sha256, StringComparison.OrdinalIgnoreCase); if (f.EvidenceItem is not null) { f.EvidenceItem.IntegrityStatus = ok ? "Verified" : "Mismatch"; f.EvidenceItem.HashSha256 = actual; f.EvidenceItem.HashGeneratedAt = DateTime.Now; f.EvidenceItem.UpdatedAt = DateTime.Now; } db.SaveChanges(); AuditService.Record("Evidence file integrity checked", $"{f.FileName}: {(ok ? "verified" : "mismatch")}.", "EvidenceFile", f.Id.ToString()); MessageBox.Show(this, $"SHA-256:\n{actual}\n\nResult: {(ok ? "VERIFIED" : "MISMATCH")}", "Integrity check", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning); RefreshData();
    }
    private void OpenSelected() { if (Id() is not int id) return; using var db = Database.Create(); var f = db.EvidenceFiles.Find(id); if (f is null) return; var path = Path.Combine(AppPaths.BaseDirectory, f.FilePath); if (!File.Exists(path)) { MessageBox.Show(this, "Stored file is missing."); return; } System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); AuditService.Record("Evidence file accessed", $"{f.FileName} was opened.", "EvidenceFile", f.Id.ToString()); }
}
