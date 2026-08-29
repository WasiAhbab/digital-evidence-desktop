using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;
namespace TraceLock.Desktop.Forms;
public sealed class NotificationsForm : Form
{
    private readonly AppUser user; private readonly DataGridView grid = Ui.Grid();
    public NotificationsForm(AppUser user) { this.user = user; Ui.StyleDialog(this, "Notifications | TraceLock", new Size(1050, 640), new Size(800, 540)); var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Button("Mark selected as read", (_, _) => Read())); bar.Controls.Add(Ui.Button("Refresh", (_, _) => RefreshData())); RefreshData(); }
    private void RefreshData() { using var db = Database.Create(); grid.DataSource = db.Notifications.Where(x => x.Recipient == user.FullName || x.Recipient == user.Username || x.Recipient == "*").OrderByDescending(x => x.CreatedAt).Take(150).Select(x => new { Id = x.Id, Severity = x.Severity, Title = x.Title, Message = x.Message, Read = x.IsRead, Created = x.CreatedAt }).ToList(); Ui.HideColumn(grid, "Id"); }
    private void Read() { if (grid.CurrentRow?.Cells["Id"].Value is not int id) return; using var db = Database.Create(); var n = db.Notifications.Find(id); if (n is null) return; n.IsRead = true; db.SaveChanges(); RefreshData(); }
}
