using Microsoft.AspNetCore.Identity;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;
public sealed class UsersForm : Form
{
    private readonly DataGridView grid = Ui.Grid();
    public UsersForm(AppUser user) { Ui.StyleDialog(this, "User Administration | TraceLock", new Size(1120, 680), new Size(900, 560)); var bar = Ui.Toolbar(); Controls.Add(grid); Controls.Add(bar); bar.Controls.Add(Ui.Button("New user", (_, _) => Edit(null), true)); bar.Controls.Add(Ui.Button("Edit selected", (_, _) => EditSelected())); bar.Controls.Add(Ui.Button("Activate / deactivate", (_, _) => Toggle())); RefreshData(); }
    private void RefreshData() { using var db = Database.Create(); grid.DataSource = db.Users.OrderBy(x => x.Username).Select(x => new { Id = x.Id, Username = x.Username, Name = x.FullName, Role = x.Role, Department = x.Department, Email = x.Email, Active = x.IsActive, LastLogin = x.LastLoginAt }).ToList(); Ui.HideColumn(grid, "Id"); }
    private int? Id() => grid.CurrentRow?.Cells["Id"].Value is int x ? x : null;
    private void EditSelected() { if (Id() is int x) Edit(x); }
    private void Edit(int? id) { using var f = new UserEditForm(id); if (f.ShowDialog(this) == DialogResult.OK) RefreshData(); }
    private void Toggle() { if (Id() is not int id) return; using var db = Database.Create(); var u = db.Users.Find(id); if (u is null) return; if (u.Username == AppSession.User?.Username) { MessageBox.Show(this, "You cannot deactivate the account currently in use."); return; } u.IsActive = !u.IsActive; db.SaveChanges(); AuditService.Record("User status changed", $"{u.Username} is now {(u.IsActive ? "active" : "inactive")}.", "User", u.Id.ToString()); RefreshData(); }
}
sealed class UserEditForm : Form
{
    private readonly int? id; private readonly TextBox username = Ui.TextBox(), name = Ui.TextBox(), email = Ui.TextBox(), department = Ui.TextBox(), password = Ui.TextBox(); private readonly ComboBox role = Ui.Combo(Roles.All); private readonly CheckBox active = new() { Text = "Account is active", Checked = true, ForeColor = Ui.Text, AutoSize = true };
    public UserEditForm(int? id)
    {
        this.id = id; Ui.StyleDialog(this, id.HasValue ? "Edit user" : "Create user", new Size(720, 560), new Size(600, 500)); Ui.DialogBody(this, out var body, out var footer);
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2 }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); body.Controls.Add(t);
        Add(t, "Username", username); Add(t, "Full name", name); Add(t, "Role", role); Add(t, "Email", email); Add(t, "Department", department); Add(t, id.HasValue ? "New password" : "Password", password); password.UseSystemPasswordChar = true; var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 56)); t.Controls.Add(Ui.Label("Status"), 0, row); t.Controls.Add(active, 1, row);
        var cancel = Ui.Button("Cancel", (_, _) => Close()); var save = Ui.Button("Save user", Save, true); footer.Controls.Add(cancel); footer.Controls.Add(save); AcceptButton = save; if (id.HasValue) LoadData();
    }
    private static void Add(TableLayoutPanel t, string label, Control c) { var row = t.RowCount; t.RowCount++; t.RowStyles.Add(new RowStyle(SizeType.Absolute, 56)); t.Controls.Add(Ui.Label(label), 0, row); c.Dock = DockStyle.Fill; c.Margin = new Padding(6, 4, 0, 4); t.Controls.Add(c, 1, row); }
    private void LoadData() { using var db = Database.Create(); var u = db.Users.Find(id!.Value); if (u is null) return; username.Text = u.Username; username.ReadOnly = true; name.Text = u.FullName; role.Text = u.Role; email.Text = u.Email; department.Text = u.Department; active.Checked = u.IsActive; }
    private void Save(object? s, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(name.Text) || (!id.HasValue && string.IsNullOrWhiteSpace(password.Text))) { MessageBox.Show(this, "Username, name and password are required for a new account."); return; }
        using var db = Database.Create(); var u = id.HasValue ? db.Users.Find(id.Value) : null; if (u is null) { u = new AppUser { Username = username.Text.Trim() }; db.Users.Add(u); }
        u.FullName = name.Text.Trim(); u.Role = role.Text; u.Email = email.Text.Trim(); u.Department = department.Text.Trim(); u.IsActive = active.Checked; if (!string.IsNullOrWhiteSpace(password.Text)) { var h = new PasswordHasher<AppUser>(); u.PasswordHash = h.HashPassword(u, password.Text); }
        try { db.SaveChanges(); AuditService.Record(id.HasValue ? "User updated" : "User created", $"Account {u.Username} was {(id.HasValue ? "updated" : "created")}.", "User", u.Id.ToString()); DialogResult = DialogResult.OK; Close(); } catch (Exception ex) { MessageBox.Show(this, "The user could not be saved.\n\n" + ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
