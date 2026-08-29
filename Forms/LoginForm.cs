using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Identity;
using TraceLock.Desktop.Data;
using TraceLock.Desktop.Models;
using TraceLock.Desktop.Services;

namespace TraceLock.Desktop.Forms;

public sealed class LoginForm : Form
{
    private readonly TextBox username = Ui.TextBox();
    private readonly TextBox password = Ui.TextBox();
    private readonly Label error = new();
    private Button signIn = null!;
    public AppUser? AuthenticatedUser { get; private set; }

    public LoginForm()
    {
        Ui.StyleForm(this);
        Text = "TraceLock | Secure Evidence Workspace";
        ClientSize = new Size(1120, 700);
        MinimumSize = new Size(900, 620);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Ui.Navy, Margin = Padding.Empty, Padding = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        Controls.Add(root);

        var hero = new GradientPanel { Dock = DockStyle.Fill, StartColor = Color.FromArgb(16, 43, 73), EndColor = Color.FromArgb(8, 17, 31), Padding = new Padding(48) };
        root.Controls.Add(hero, 0, 0);
        BuildHero(hero);

        var right = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Navy, Padding = new Padding(52, 46, 52, 42), AutoScroll = true };
        root.Controls.Add(right, 1, 0);
        var formCard = BuildLoginCard();
        formCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        formCard.Width = 520;
        right.Controls.Add(formCard);
        right.Resize += (_, _) =>
        {
            formCard.Width = Math.Min(520, Math.Max(360, right.ClientSize.Width - 104));
            formCard.Left = Math.Max(0, (right.ClientSize.Width - formCard.Width) / 2);
        };
        Shown += (_, _) => { username.Focus(); right.PerformLayout(); };
        AcceptButton = signIn;
    }

    private void BuildHero(Panel hero)
    {
        var brand = new Label { Text = "TRACELOCK", AutoSize = true, Font = new Font("Segoe UI Black", 30), ForeColor = Ui.Accent2, Location = new Point(48, 52) };
        hero.Controls.Add(brand);
        hero.Controls.Add(new Label { Text = "DIGITAL EVIDENCE MANAGEMENT", AutoSize = true, Font = new Font("Segoe UI Semibold", 9.5f), ForeColor = Ui.Text, Location = new Point(51, 104) });
        hero.Controls.Add(new Label { Text = "A focused workspace for cases, evidence,\nchain of custody and forensic records.", AutoSize = true, Font = new Font("Segoe UI", 12), ForeColor = Color.FromArgb(205, 218, 233), Location = new Point(51, 165) });
        AddFeature(hero, "01", "Evidence integrity", "Track SHA-256 verification and evidence status.", 292);
        AddFeature(hero, "02", "Chain of custody", "Record every transfer and current custodian.", 378);
        AddFeature(hero, "03", "Audit history", "Keep a clear record of important activity.", 464);
        var footer = new Label { Text = "TRACELOCK  •  SECURE EVIDENCE WORKSPACE", AutoSize = true, Font = new Font("Segoe UI Semibold", 7.5f), ForeColor = Ui.Muted };
        hero.Controls.Add(footer);
        hero.Resize += (_, _) => footer.Location = new Point(51, Math.Max(570, hero.ClientSize.Height - 48));
    }

    private Panel BuildLoginCard()
    {
        var card = Ui.CardPanel();
        card.Size = new Size(520, 540);
        card.Padding = new Padding(34, 30, 34, 30);
        card.Controls.Add(new Label { Text = "Welcome back", AutoSize = true, Font = new Font("Segoe UI Semibold", 27), ForeColor = Ui.Text, Location = new Point(34, 30) });
        card.Controls.Add(new Label { Text = "Sign in to continue to your investigation workspace.", AutoSize = false, Size = new Size(440, 42), Font = new Font("Segoe UI", 10), ForeColor = Ui.Muted, Location = new Point(37, 80) });
        AddField(card, "USERNAME", username, 135);
        AddField(card, "PASSWORD", password, 224);
        password.UseSystemPasswordChar = true;

        var showPassword = new CheckBox { Text = "Show password", AutoSize = true, ForeColor = Ui.Muted, Location = new Point(37, 312), FlatStyle = FlatStyle.Flat };
        showPassword.CheckedChanged += (_, _) => password.UseSystemPasswordChar = !showPassword.Checked;
        card.Controls.Add(showPassword);
        error.AutoSize = false; error.Size = new Size(440, 42); error.ForeColor = Ui.Red; error.Font = new Font("Segoe UI", 9f); error.Location = new Point(37, 340);
        card.Controls.Add(error);
        signIn = Ui.Button("Sign in", Login, true); signIn.Size = new Size(180, 46); signIn.Location = new Point(34, 386); card.Controls.Add(signIn);

        var hint = new Panel { BackColor = Color.FromArgb(12, 25, 43), Location = new Point(34, 452), Size = new Size(452, 64), Padding = new Padding(14) };
        Ui.Round(hint, 10); card.Controls.Add(hint);
        hint.Controls.Add(new Label { Text = "DEMO ACCESS", AutoSize = true, Font = new Font("Segoe UI Semibold", 7.8f), ForeColor = Ui.Accent, Location = new Point(14, 9) });
        hint.Controls.Add(new Label { Text = "Username: admin    •    Password: Admin@123!", AutoSize = false, Size = new Size(420, 24), Font = new Font("Segoe UI", 9.2f), ForeColor = Ui.Text, Location = new Point(14, 29) });
        return card;
    }

    private static void AddField(Control parent, string label, TextBox box, int y)
    {
        parent.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(37, y), ForeColor = Ui.Muted, Font = new Font("Segoe UI Semibold", 8.5f) });
        box.Location = new Point(34, y + 24); box.Size = new Size(452, 40); box.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; parent.Controls.Add(box);
    }

    private static void AddFeature(Control parent, string number, string title, string description, int y)
    {
        parent.Controls.Add(new Label { Text = number, AutoSize = true, Font = new Font("Segoe UI Black", 9), ForeColor = Ui.Accent, Location = new Point(51, y) });
        parent.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI Semibold", 10.5f), ForeColor = Ui.Text, Location = new Point(88, y - 2) });
        parent.Controls.Add(new Label { Text = description, AutoSize = false, Size = new Size(310, 38), Font = new Font("Segoe UI", 8.5f), ForeColor = Ui.Muted, Location = new Point(88, y + 22) });
    }

    private void Login(object? sender, EventArgs e)
    {
        error.Text = string.Empty;
        var enteredUsername = username.Text.Trim();
        var enteredPassword = password.Text;
        if (string.IsNullOrWhiteSpace(enteredUsername) || string.IsNullOrWhiteSpace(enteredPassword)) { error.Text = "Enter your username and password."; return; }
        signIn.Enabled = false; Cursor = Cursors.WaitCursor;
        try
        {
            using var db = Database.Create();
            var user = db.Users.AsEnumerable().FirstOrDefault(x => string.Equals(x.Username, enteredUsername, StringComparison.OrdinalIgnoreCase));
            if (user is null || !user.IsActive) { InvalidCredentials(); return; }
            var hasher = new PasswordHasher<AppUser>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, enteredPassword);
            if (result == PasswordVerificationResult.Failed) { InvalidCredentials(); return; }
            if (result == PasswordVerificationResult.SuccessRehashNeeded) user.PasswordHash = hasher.HashPassword(user, enteredPassword);
            user.LastLoginAt = DateTime.Now; db.SaveChanges();
            AuthenticatedUser = user; AppSession.User = user;
            AuditService.Record("User signed in", $"{user.Username} signed in to the TraceLock desktop workspace.", "User", user.Id.ToString());
            DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex)
        {
            error.Text = "Sign-in failed. Please try again.";
            MessageBox.Show(this, $"TraceLock could not complete the sign-in.\n\n{ex.Message}", "Sign-in error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { if (!IsDisposed) { signIn.Enabled = true; Cursor = Cursors.Default; } }
    }

    private void InvalidCredentials() { error.Text = "The username or password is incorrect."; password.SelectAll(); password.Focus(); }

    private sealed class GradientPanel : Panel
    {
        public Color StartColor { get; init; } public Color EndColor { get; init; }
        public GradientPanel() => SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, 35f); e.Graphics.FillRectangle(brush, ClientRectangle);
            using var glow = new SolidBrush(Color.FromArgb(24, 42, 184, 255)); e.Graphics.FillEllipse(glow, new Rectangle(-100, Height - 200, 330, 330));
            using var glow2 = new SolidBrush(Color.FromArgb(18, 82, 211, 255)); e.Graphics.FillEllipse(glow2, new Rectangle(Width - 180, -120, 300, 300));
        }
    }
}
