using System.Text;
using Microsoft.EntityFrameworkCore;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;
namespace TraceLock.Desktop.Forms;
public sealed class ReportsForm : Form
{
    private readonly AppUser user; private readonly Label summary = new(); private readonly DataGridView grid = Ui.Grid();
    public ReportsForm(AppUser user)
    {
        this.user = user; Ui.StyleDialog(this, "Reports | TraceLock", new Size(1120, 700), new Size(900, 560)); var bar = Ui.Toolbar(92); Controls.Add(grid); Controls.Add(bar);
        var summaryPanel = new Panel { Width = 500, Height = 72, Margin = new Padding(0, 0, 16, 0) }; summaryPanel.Controls.Add(summary); summary.Dock = DockStyle.Fill; summary.ForeColor = Ui.Text; summary.Font = new Font("Segoe UI Semibold", 10.5f); summary.TextAlign = ContentAlignment.MiddleLeft; bar.Controls.Add(summaryPanel);
        bar.Controls.Add(Ui.Button("Evidence CSV", (_, _) => ExportEvidence(), true)); bar.Controls.Add(Ui.Button("Custody CSV", (_, _) => ExportCustody())); bar.Controls.Add(Ui.Button("Audit CSV", (_, _) => ExportAudit())); RefreshData();
    }
    private void RefreshData() { using var db = Database.Create(); summary.Text = $"Cases: {db.Cases.Count()}     Evidence: {db.Evidence.Count()}\nTransfers: {db.CustodyRecords.Count()}     Analyses: {db.ForensicAnalyses.Count()}"; grid.DataSource = db.Evidence.Include(x => x.CaseFile).OrderBy(x => x.EvidenceNumber).Select(x => new { Evidence = x.EvidenceNumber, Name = x.Name, Type = x.Type, Case = x.CaseFile == null ? "" : x.CaseFile.CaseNumber, Status = x.Status, Custodian = x.CurrentCustodian, Integrity = x.IntegrityStatus, SHA256 = x.HashSha256, Collected = x.CollectedOn }).ToList(); }
    private static string Esc(string s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
    private void SaveCsv(string name, string text) { using var d = new SaveFileDialog { FileName = name, Filter = "CSV files (*.csv)|*.csv", Title = "Save report" }; if (d.ShowDialog(this) != DialogResult.OK) return; File.WriteAllText(d.FileName, text, new UTF8Encoding(true)); AuditService.Record("Report exported", $"{name} was exported to a local CSV file.", "Report", name); MessageBox.Show(this, "Report saved successfully.", "TraceLock", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    private void ExportEvidence() { using var db = Database.Create(); var sb = new StringBuilder("Evidence Number,Name,Type,Status,Case,Custodian,Storage,Integrity,SHA-256,Collected On\n"); foreach (var x in db.Evidence.Include(x => x.CaseFile).OrderBy(x => x.EvidenceNumber)) sb.AppendLine(string.Join(',', Esc(x.EvidenceNumber), Esc(x.Name), Esc(x.Type), Esc(x.Status), Esc(x.CaseFile?.CaseNumber ?? ""), Esc(x.CurrentCustodian), Esc(x.StorageLocation), Esc(x.IntegrityStatus), Esc(x.HashSha256), x.CollectedOn.ToString("yyyy-MM-dd"))); SaveCsv("tracelock-evidence-report.csv", sb.ToString()); }
    private void ExportCustody() { using var db = Database.Create(); var sb = new StringBuilder("Evidence,From,To,Purpose,Location,Condition,Authorization,Transferred At,Recorded At\n"); foreach (var x in db.CustodyRecords.Include(x => x.EvidenceItem).OrderBy(x => x.TransferredAt)) sb.AppendLine(string.Join(',', Esc(x.EvidenceItem?.EvidenceNumber ?? ""), Esc(x.FromPerson), Esc(x.ToPerson), Esc(x.Purpose), Esc(x.Location), Esc(x.Condition), Esc(x.Authorization), x.TransferredAt.ToString("yyyy-MM-dd HH:mm"), x.RecordedAt.ToString("yyyy-MM-dd HH:mm"))); SaveCsv("tracelock-chain-of-custody.csv", sb.ToString()); }
    private void ExportAudit() { using var db = Database.Create(); var sb = new StringBuilder("Timestamp,Actor,Action,Entity,Entity Id,Details,Source\n"); foreach (var x in db.AuditLogs.OrderByDescending(x => x.OccurredAt)) sb.AppendLine(string.Join(',', x.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"), Esc(x.Actor), Esc(x.Action), Esc(x.Entity), Esc(x.EntityId), Esc(x.Details), Esc(x.IpAddress))); SaveCsv("tracelock-audit-history.csv", sb.ToString()); }
}
