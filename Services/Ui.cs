using System.Drawing.Drawing2D;

namespace TraceLock.Desktop.Services;

public static class Ui
{
    public static readonly Color Navy = Color.FromArgb(8, 15, 28);
    public static readonly Color Navy2 = Color.FromArgb(12, 22, 39);
    public static readonly Color Panel = Color.FromArgb(17, 29, 49);
    public static readonly Color Panel2 = Color.FromArgb(24, 40, 65);
    public static readonly Color CardBackground = Color.FromArgb(19, 34, 56);
    public static readonly Color CardHover = Color.FromArgb(30, 51, 79);
    public static readonly Color Accent = Color.FromArgb(48, 194, 255);
    public static readonly Color Accent2 = Color.FromArgb(117, 223, 255);
    public static readonly Color Text = Color.FromArgb(244, 248, 252);
    public static readonly Color Muted = Color.FromArgb(156, 174, 197);
    public static readonly Color Line = Color.FromArgb(45, 65, 92);
    public static readonly Color Green = Color.FromArgb(53, 213, 157);
    public static readonly Color Amber = Color.FromArgb(250, 193, 65);
    public static readonly Color Red = Color.FromArgb(248, 107, 117);

    public static void StyleForm(Form form)
    {
        form.BackColor = Navy;
        form.ForeColor = Text;
        form.Font = new Font("Segoe UI", 9.5f);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.AutoScroll = false;
        SetDoubleBuffered(form, true);
    }

    public static void StyleDialog(Form form, string title, Size size, Size minimum)
    {
        StyleForm(form);
        form.Text = title;
        form.ClientSize = size;
        form.MinimumSize = minimum;
        form.MaximizeBox = true;
        form.FormBorderStyle = FormBorderStyle.Sizable;
        form.StartPosition = FormStartPosition.CenterParent;
    }

    public static Panel DialogBody(Form form, out Panel body, out FlowLayoutPanel footer)
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = Padding.Empty, Margin = Padding.Empty, BackColor = Navy };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        form.Controls.Add(root);

        body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(26, 24, 26, 18), BackColor = Navy };
        footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(18, 13, 18, 13), BackColor = Panel, AutoScroll = false };
        root.Controls.Add(body, 0, 0);
        root.Controls.Add(footer, 0, 1);
        return body;
    }

    public static FlowLayoutPanel Toolbar(int height = 58)
        => new() { Dock = DockStyle.Top, Height = height, Padding = new Padding(0, 8, 0, 8), WrapContents = false, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Navy };

    public static Button Button(string text, EventHandler? click = null, bool primary = false)
    {
        var b = new ModernButton
        {
            Text = text,
            Primary = primary,
            AutoSize = false,
            Height = 40,
            Width = 130,
            FlatStyle = FlatStyle.Flat,
            ForeColor = primary ? Color.FromArgb(4, 18, 30) : Text,
            Padding = new Padding(14, 0, 14, 0),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0),
            Font = new Font("Segoe UI Semibold", 9.5f)
        };
        if (click != null) b.Click += click;
        return b;
    }

    public static Button NavButton(string text, EventHandler click)
    {
        var b = new ModernButton
        {
            Text = text, AutoSize = false, Width = 221, Height = 42,
            FlatStyle = FlatStyle.Flat, ForeColor = Muted, TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(17, 0, 10, 0), Cursor = Cursors.Hand,
            Margin = new Padding(0, 3, 0, 3), Font = new Font("Segoe UI Semibold", 9.2f)
        };
        b.Click += click;
        return b;
    }

    public static void SetNavActive(Button button, bool active)
    {
        if (button is ModernButton modern) modern.Active = active;
        button.ForeColor = active ? Color.FromArgb(4, 18, 30) : Muted;
        button.Invalidate();
    }

    public static TextBox TextBox(bool multiline = false)
    {
        return new TextBox
        {
            Multiline = multiline, BackColor = Color.FromArgb(10, 20, 35), ForeColor = Text,
            BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10, 7, 10, 7),
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            Font = new Font("Segoe UI", 10f), MinimumSize = new Size(0, multiline ? 0 : 38)
        };
    }

    public static ComboBox Combo(params string[] items)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(10, 20, 35), ForeColor = Text, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f), MinimumSize = new Size(0, 36) };
        c.Items.AddRange(items);
        if (items.Length > 0) c.SelectedIndex = 0;
        return c;
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(10, 20, 35), BorderStyle = BorderStyle.None,
            GridColor = Line, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, EnableHeadersVisualStyles = false,
            Font = new Font("Segoe UI", 9.1f), ColumnHeadersHeight = 42, ScrollBars = ScrollBars.Both
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Panel2; g.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.8f); g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        g.DefaultCellStyle.BackColor = Color.FromArgb(12, 23, 39); g.DefaultCellStyle.ForeColor = Text;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(35, 86, 122); g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0); g.RowTemplate.Height = 40;
        g.DataError += (_, _) => { };
        return g;
    }

    public static Label Label(string text) => new() { Text = text, AutoSize = true, ForeColor = Muted, Margin = new Padding(3, 7, 8, 3), Font = new Font("Segoe UI", 9f) };

    public static void HideColumn(DataGridView grid, string name)
    {
        var column = grid.Columns[name];
        if (column is not null) column.Visible = false;
    }

    public static void SetFillWeight(DataGridView grid, string name, float weight)
    {
        var column = grid.Columns[name];
        if (column is not null) column.FillWeight = weight;
    }

    public static Panel CardPanel()
    {
        var p = new Panel { BackColor = CardBackground, Padding = new Padding(18), Margin = Padding.Empty, BorderStyle = BorderStyle.None };
        Round(p, 12); return p;
    }

    public static void Round(Control c, int radius = 12)
    {
        void Apply()
        {
            if (c.IsDisposed || c.Width < 2 || c.Height < 2) return;
            using var path = RoundedPath(new Rectangle(0, 0, c.Width - 1, c.Height - 1), radius);
            c.Region?.Dispose();
            c.Region = new Region(path);
        }
        c.Resize += (_, _) => Apply(); c.HandleCreated += (_, _) => Apply(); if (c.IsHandleCreated) Apply();
    }

    public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (d <= 0) { path.AddRectangle(bounds); return path; }
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90); path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90); path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path;
    }

    private sealed class ModernButton : Button
    {
        public bool Primary { get; set; }
        public bool Active { get; set; }
        private bool hover;
        public ModernButton() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true); FlatAppearance.BorderSize = 0; }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; var rect = ClientRectangle; rect.Inflate(-1, -1);
            if (rect.Width <= 0 || rect.Height <= 0) return;
            var fill = Primary ? (hover ? Accent2 : Accent) : Active ? Accent : (hover ? CardHover : Panel2);
            using var brush = new SolidBrush(fill); using var path = RoundedPath(rect, 9); e.Graphics.FillPath(brush, path);
            if (!Primary && !Active) { using var pen = new Pen(Line); e.Graphics.DrawPath(pen, path); }
            if (Active) { using var accentBrush = new SolidBrush(Color.FromArgb(4, 18, 30)); e.Graphics.FillRectangle(accentBrush, 0, 8, 3, Math.Max(0, Height - 16)); }
            TextRenderer.DrawText(e.Graphics, Text, Font, rect, (Primary || Active) ? Color.FromArgb(4, 18, 30) : Ui.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }

    private static void SetDoubleBuffered(Control control, bool enabled)
    {
        typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(control, enabled, null);
    }
}
