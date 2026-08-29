using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;

public sealed class MainForm : Form
{
    private readonly AppUser user;
    private readonly Panel content = new();
    private readonly Label pageTitle = new();
    private readonly Label pageSubtitle = new();
    private readonly List<Button> navButtons = new();

    public MainForm(AppUser user)
    {
        this.user = user ?? throw new ArgumentNullException(nameof(user));
        Ui.StyleForm(this);
        Text = "TraceLock | Digital Evidence Workspace";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        BuildShell();
        Shown += (_, _) => ShowDashboard();
    }

    private void BuildShell()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty, BackColor = Ui.Navy };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 255));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(shell);

        var sidebar = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Panel, Padding = new Padding(16, 18, 16, 16) };
        shell.Controls.Add(sidebar, 0, 0); shell.SetRowSpan(sidebar, 2);
        BuildSidebar(sidebar);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Navy, Padding = new Padding(30, 13, 30, 10) };
        shell.Controls.Add(header, 1, 0);
        BuildHeader(header);

        content.Dock = DockStyle.Fill; content.BackColor = Ui.Navy; content.Padding = new Padding(30, 8, 30, 24); content.AutoScroll = false;
        shell.Controls.Add(content, 1, 1);
    }

    private void BuildSidebar(Panel sidebar)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.Transparent, Margin = Padding.Empty, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        sidebar.Controls.Add(layout);

        var brand = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        brand.Controls.Add(new Label { Text = "TRACELOCK", AutoSize = true, Font = new Font("Segoe UI Black", 22), ForeColor = Ui.Accent2, Location = new Point(4, 0) });
        brand.Controls.Add(new Label { Text = "DIGITAL EVIDENCE MANAGEMENT", AutoSize = true, Font = new Font("Segoe UI Semibold", 7.4f), ForeColor = Ui.Muted, Location = new Point(7, 40) });
        layout.Controls.Add(brand, 0, 0);

        var profile = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Navy2, Padding = new Padding(12) }; Ui.Round(profile, 12); layout.Controls.Add(profile, 0, 1);
        var avatar = new Label { Text = Initials(user.FullName), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(4, 18, 30), BackColor = Ui.Accent, Location = new Point(10, 17), Size = new Size(44, 44) }; Ui.Round(avatar, 22); profile.Controls.Add(avatar);
        profile.Controls.Add(new Label { Text = user.FullName, AutoEllipsis = true, AutoSize = false, Location = new Point(65, 14), Size = new Size(145, 22), Font = new Font("Segoe UI Semibold", 9.2f), ForeColor = Ui.Text });
        profile.Controls.Add(new Label { Text = user.Role, AutoEllipsis = true, AutoSize = false, Location = new Point(65, 39), Size = new Size(145, 20), Font = new Font("Segoe UI", 8.2f), ForeColor = Ui.Muted });

        layout.Controls.Add(new Label { Text = "WORKSPACE", AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI Semibold", 7.5f), Padding = new Padding(6, 12, 0, 0), Margin = Padding.Empty }, 0, 2);
        var menu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 2), Margin = Padding.Empty };
        layout.Controls.Add(menu, 0, 3);
        AddMenu(menu, "Dashboard", ShowDashboard); AddMenu(menu, "Cases", () => Open(new CasesForm(user))); AddMenu(menu, "Evidence", () => Open(new EvidenceForm(user)));
        AddMenu(menu, "Chain of Custody", () => Open(new CustodyForm(user))); AddMenu(menu, "Forensic Analysis", () => Open(new AnalysisForm(user)));
        AddMenu(menu, "Files & Integrity", () => Open(new FilesForm(user))); AddMenu(menu, "Notifications", () => Open(new NotificationsForm(user)));
        AddMenu(menu, "Audit Log", () => Open(new AuditForm(user))); AddMenu(menu, "Reports", () => Open(new ReportsForm(user)));
        if (user.Role == Roles.Administrator) AddMenu(menu, "User Administration", () => Open(new UsersForm(user)));

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var signOut = Ui.NavButton("Sign out", (_, _) => { try { AuditService.Record("User signed out", $"{user.Username} closed the workspace.", "User", user.Id.ToString()); } catch { } Close(); });
        signOut.Dock = DockStyle.Fill; signOut.ForeColor = Ui.Red; footer.Controls.Add(signOut); layout.Controls.Add(footer, 0, 4); navButtons.Add(signOut);
    }

    private void BuildHeader(Panel header)
    {
        pageTitle.AutoSize = true; pageTitle.Font = new Font("Segoe UI Semibold", 21); pageTitle.ForeColor = Ui.Text; pageTitle.Location = new Point(30, 12); header.Controls.Add(pageTitle);
        pageSubtitle.AutoSize = false; pageSubtitle.Location = new Point(32, 51); pageSubtitle.Size = new Size(720, 24); pageSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; pageSubtitle.Font = new Font("Segoe UI", 9.2f); pageSubtitle.ForeColor = Ui.Muted; pageSubtitle.AutoEllipsis = true; header.Controls.Add(pageSubtitle);
        var status = new Panel { Size = new Size(182, 38), BackColor = Ui.Navy2, Anchor = AnchorStyles.Top | AnchorStyles.Right }; Ui.Round(status, 10); header.Controls.Add(status);
        status.Controls.Add(new Label { Text = "●", AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Ui.Green, Location = new Point(13, 10) });
        status.Controls.Add(new Label { Text = "Workspace ready", AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5f), ForeColor = Ui.Text, Location = new Point(32, 10) });
        void Position() { status.Left = Math.Max(20, header.ClientSize.Width - status.Width - 30); pageSubtitle.Width = Math.Max(280, status.Left - pageSubtitle.Left - 22); }
        header.Resize += (_, _) => Position(); header.HandleCreated += (_, _) => Position();
    }

    private void AddMenu(FlowLayoutPanel menu, string title, Action action)
    {
        Button? button = null;
        button = Ui.NavButton(title, (_, _) => { if (button is not null) SetActive(button); action(); });
        button.Width = 221; menu.Controls.Add(button); navButtons.Add(button);
    }
    private void SetActive(Button active) { foreach (var button in navButtons) Ui.SetNavActive(button, ReferenceEquals(button, active)); }

    private void ShowDashboard()
    {
        if (IsDisposed) return;
        var dashboardButton = navButtons.FirstOrDefault(b => b.Text == "Dashboard"); if (dashboardButton is not null) SetActive(dashboardButton);
        pageTitle.Text = "Dashboard"; pageSubtitle.Text = $"Good to see you, {user.FullName}. Here's your current evidence workspace.";
        content.SuspendLayout(); content.Controls.Clear();
        try
        {
            using var db = Database.Create();
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Margin = Padding.Empty, Padding = Padding.Empty };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); content.Controls.Add(root);
            var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent, Margin = Padding.Empty, Padding = Padding.Empty };
            for (var i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); root.Controls.Add(metrics, 0, 0);
            AddMetric(metrics, 0, "ACTIVE CASES", db.Cases.Count(x => x.Status != "Closed").ToString(), "Currently open investigations", Ui.Accent);
            AddMetric(metrics, 1, "EVIDENCE ITEMS", db.Evidence.Count().ToString(), "Registered evidence records", Ui.Green);
            AddMetric(metrics, 2, "PENDING ANALYSIS", db.ForensicAnalyses.Count(x => x.Status != "Completed").ToString(), "Awaiting examination", Ui.Amber);
            AddMetric(metrics, 3, "UNREAD ALERTS", db.Notifications.Count(x => (x.Recipient == user.FullName || x.Recipient == user.Username || x.Recipient == "*") && !x.IsRead).ToString(), "Notifications requiring attention", Ui.Red);
            var lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent, Margin = new Padding(0, 12, 0, 0), Padding = Padding.Empty };
            lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42)); lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58)); root.Controls.Add(lower, 0, 1);
            lower.Controls.Add(BuildActivityCard(db), 0, 0); lower.Controls.Add(BuildCasesCard(db), 1, 0);
        }
        catch (Exception ex)
        {
            content.Controls.Clear();
            var error = Ui.CardPanel(); error.Dock = DockStyle.Fill; error.Padding = new Padding(28);
            error.Controls.Add(new Label { Text = "Dashboard data could not be loaded", AutoSize = true, Font = new Font("Segoe UI Semibold", 16), ForeColor = Ui.Red, Location = new Point(28, 28) });
            error.Controls.Add(new Label { Text = ex.Message, AutoSize = false, Size = new Size(700, 90), Font = new Font("Segoe UI", 10), ForeColor = Ui.Muted, Location = new Point(30, 70) });
            var retry = Ui.Button("Retry", (_, _) => ShowDashboard(), true); retry.Location = new Point(30, 170); error.Controls.Add(retry); content.Controls.Add(error);
        }
        finally { content.ResumeLayout(true); }
    }

    private static void AddMetric(TableLayoutPanel host, int column, string title, string value, string caption, Color accent)
    {
        var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0, 0, column == 3 ? 0 : 10, 0), Margin = Padding.Empty }; host.Controls.Add(wrapper, column, 0);
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Ui.CardBackground, Padding = new Padding(18, 14, 18, 12), Margin = Padding.Empty }; Ui.Round(card, 11); wrapper.Controls.Add(card);
        var stripe = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent }; card.Controls.Add(stripe);
        card.Controls.Add(new Label { Text = title, AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI Semibold", 7.5f), Location = new Point(30, 13) });
        card.Controls.Add(new Label { Text = value, AutoSize = true, ForeColor = Ui.Text, Font = new Font("Segoe UI Black", 25), Location = new Point(30, 32) });
        card.Controls.Add(new Label { Text = caption, AutoEllipsis = true, AutoSize = false, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 8.2f), Location = new Point(30, 88), Size = new Size(240, 20) });
    }

    private static Panel BuildActivityCard(EvidenceDbContext db)
    {
        var card = MakeSectionCard(); var layout = MakeSectionLayout(); card.Controls.Add(layout); AddSectionHeading(layout, "Recent activity", "Latest actions recorded by the workspace", 0);
        var list = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(11, 21, 37), ForeColor = Ui.Text, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9f), IntegralHeight = false, ItemHeight = 40, DrawMode = DrawMode.OwnerDrawFixed, HorizontalScrollbar = false };
        list.DrawItem += (_, e) => { if (e.Index < 0) return; e.DrawBackground(); var text = list.Items[e.Index]?.ToString() ?? string.Empty; var bounds = new Rectangle(e.Bounds.X + 14, e.Bounds.Y + 2, Math.Max(1, e.Bounds.Width - 28), e.Bounds.Height - 4); TextRenderer.DrawText(e.Graphics, text, list.Font, bounds, Ui.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix); using var pen = new Pen(Ui.Line); e.Graphics.DrawLine(pen, e.Bounds.Left + 12, e.Bounds.Bottom - 1, e.Bounds.Right - 12, e.Bounds.Bottom - 1); };
        foreach (var audit in db.AuditLogs.OrderByDescending(x => x.OccurredAt).Take(8)) list.Items.Add($"{audit.OccurredAt:dd MMM HH:mm}   •   {audit.Actor}   •   {audit.Action}");
        if (list.Items.Count == 0) list.Items.Add("No recent activity has been recorded."); layout.Controls.Add(list, 0, 1); return card;
    }

    private static Panel BuildCasesCard(EvidenceDbContext db)
    {
        var card = MakeSectionCard(); var layout = MakeSectionLayout(); card.Controls.Add(layout); AddSectionHeading(layout, "Priority cases", "Open investigations that need attention", 0);
        var grid = Ui.Grid(); grid.DataSource = db.Cases.Where(x => x.Status != "Closed").OrderByDescending(x => x.Priority == "Critical").ThenByDescending(x => x.Priority == "High").ThenByDescending(x => x.UpdatedAt).Take(8).Select(x => new { Case = x.CaseNumber, Title = x.Title, Priority = x.Priority, Status = x.Status, Investigator = x.Investigator }).ToList();
        Ui.SetFillWeight(grid, "Case", 18); Ui.SetFillWeight(grid, "Title", 34); Ui.SetFillWeight(grid, "Priority", 17); Ui.SetFillWeight(grid, "Status", 18); Ui.SetFillWeight(grid, "Investigator", 24);
        layout.Controls.Add(grid, 0, 1); return card;
    }

    private static Panel MakeSectionCard() { var card = new Panel { Dock = DockStyle.Fill, BackColor = Ui.CardBackground, Padding = new Padding(18), Margin = Padding.Empty }; Ui.Round(card, 12); return card; }
    private static TableLayoutPanel MakeSectionLayout() { var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent, Margin = Padding.Empty, Padding = Padding.Empty }; layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); return layout; }
    private static void AddSectionHeading(TableLayoutPanel layout, string title, string subtitle, int row)
    {
        var heading = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = Padding.Empty };
        heading.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI Semibold", 13), ForeColor = Ui.Text, Location = new Point(0, 0) });
        heading.Controls.Add(new Label { Text = subtitle, AutoSize = false, Size = new Size(600, 24), Font = new Font("Segoe UI", 8.4f), ForeColor = Ui.Muted, Location = new Point(1, 28) }); layout.Controls.Add(heading, 0, row);
    }
    private void Open(Form form) { form.StartPosition = FormStartPosition.CenterParent; form.ShowDialog(this); if (!IsDisposed) ShowDashboard(); }
    private static string Initials(string? name) { var parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries); if (parts.Length == 0) return "U"; if (parts.Length == 1) return parts[0][0].ToString().ToUpperInvariant(); return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant(); }
}
