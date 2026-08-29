using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;
namespace TraceLock.Desktop.Forms;
public sealed class AuditForm : Form
{
    private readonly DataGridView grid = Ui.Grid(); private readonly TextBox search = Ui.TextBox();
    public AuditForm(AppUser user) { Ui.StyleDialog(this, "Audit Log | TraceLock", new Size(1250, 700), new Size(900, 560)); var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Label("Search")); search.Width = 300; bar.Controls.Add(search); bar.Controls.Add(Ui.Button("Refresh", (_, _) => RefreshData())); search.TextChanged += (_, _) => RefreshData(); RefreshData(); }
    private void RefreshData() { using var db = Database.Create(); var q = search.Text.Trim(); grid.DataSource = db.AuditLogs.Where(x => string.IsNullOrEmpty(q) || x.Actor.Contains(q) || x.Action.Contains(q) || x.Entity.Contains(q) || x.Details.Contains(q)).OrderByDescending(x => x.OccurredAt).Take(300).Select(x => new { Time = x.OccurredAt, Actor = x.Actor, Action = x.Action, Entity = x.Entity, Id = x.EntityId, Details = x.Details, Source = x.IpAddress }).ToList(); }
}
