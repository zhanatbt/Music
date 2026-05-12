using System.Drawing;
using System.Windows.Forms;

namespace MusicApp.UI;

public static class AppleMusicTheme
{
    public static readonly Color Background = Color.FromArgb(18, 18, 18);
    public static readonly Color Surface = Color.FromArgb(30, 30, 30);
    public static readonly Color SurfaceAlt = Color.FromArgb(42, 42, 42);
    public static readonly Color Accent = Color.FromArgb(252, 60, 68);
    public static readonly Color AccentHover = Color.FromArgb(220, 40, 50);
    public static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
    public static readonly Color TextSecondary = Color.FromArgb(153, 153, 153);
    public static readonly Color Border = Color.FromArgb(58, 58, 60);

    public static void Apply(Form form)
    {
        form.BackColor = Background;
        form.ForeColor = TextPrimary;
        ApplyToControls(form.Controls);
    }

    private static void ApplyToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            ApplyControl(control);
            if (control.HasChildren)
                ApplyToControls(control.Controls);
        }
    }

    private static void ApplyControl(Control c)
    {
        switch (c)
        {
            case DataGridView g:
                ApplyGrid(g);
                return;
            case AxWMPLib.AxWindowsMediaPlayer:
                return;
            case Button b:
                b.BackColor = Accent;
                b.ForeColor = TextPrimary;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = AccentHover;
                b.FlatAppearance.MouseOverBackColor = AccentHover;
                b.Cursor = Cursors.Hand;
                return;
            case TabPage tp:
                tp.BackColor = Background;
                tp.ForeColor = TextPrimary;
                return;

            case TabControl tc:
                tc.BackColor = Background;
                tc.Appearance = TabAppearance.Normal;
                return;

            case SplitContainer sc:
                sc.BackColor = Background;
                sc.Panel1.BackColor = Background;
                sc.Panel2.BackColor = Background;
                return;

            case TableLayoutPanel tlp:
                tlp.BackColor = Background;
                return;

            case FlowLayoutPanel flp:
                flp.BackColor = Background;
                return;

            case Panel p:
                p.BackColor = Background;
                return;
            case TextBox t:
                t.BackColor = Surface;
                t.ForeColor = TextPrimary;
                t.BorderStyle = BorderStyle.FixedSingle;
                return;
            case NumericUpDown n:
                n.BackColor = Surface;
                n.ForeColor = TextPrimary;
                return;
            case ComboBox cb:
                cb.BackColor = Surface;
                cb.ForeColor = TextPrimary;
                cb.FlatStyle = FlatStyle.Flat;
                return;
            case CheckedListBox cl:
                cl.BackColor = Surface;
                cl.ForeColor = TextPrimary;
                return;
            case Label l:
                l.ForeColor = TextPrimary;
                if (l.BackColor != Color.Transparent)
                    l.BackColor = Color.Transparent;
                return;
            case GroupBox gb:
                gb.BackColor = Background;
                gb.ForeColor = TextSecondary;
                return;
        }
    }

    private static void ApplyGrid(DataGridView g)
    {
        g.BackgroundColor = Surface;
        g.GridColor = Border;
        g.BorderStyle = BorderStyle.None;
        g.EnableHeadersVisualStyles = false;

        g.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        g.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

        g.DefaultCellStyle.BackColor = Surface;
        g.DefaultCellStyle.ForeColor = TextPrimary;
        g.DefaultCellStyle.SelectionBackColor = Accent;
        g.DefaultCellStyle.SelectionForeColor = TextPrimary;

        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(24, 24, 24);
        g.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
        g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Accent;

        g.RowHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        g.RowHeadersDefaultCellStyle.ForeColor = TextPrimary;
        g.RowHeadersDefaultCellStyle.SelectionBackColor = Accent;
    }
}