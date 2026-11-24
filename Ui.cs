using System.Data;
using System.Drawing;

namespace EquipmentProgress;

// Keeps common control styling and layout helpers in one place.
internal static class Ui
{
    public static readonly Color Navy = Color.FromArgb(27, 48, 75);
    public static readonly Color Blue = Color.FromArgb(47, 110, 174);
    public static readonly Color LightBlue = Color.FromArgb(232, 241, 250);
    public static readonly Color Gray = Color.FromArgb(103, 120, 137);

    public static Button Button(string text, bool secondary = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(100, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = secondary ? LightBlue : Blue,
            ForeColor = secondary ? Navy : Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(5),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    // Configure a consistent table appearance across the application.
    public static DataGridView Grid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            RowTemplate = { Height = 30 },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(246, 249, 252) },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = LightBlue,
                ForeColor = Navy,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            },
            EnableHeadersVisualStyles = false
        };
    }

    public static TableLayoutPanel Page(string heading, string subtitle)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White,
            ColumnCount = 1, RowCount = 2
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(new Label
        {
            Text = heading, ForeColor = Navy,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 40
        });
        header.Controls.Add(new Label
        {
            Text = subtitle, ForeColor = Gray, Dock = DockStyle.Bottom, Height = 32
        });
        root.Controls.Add(header, 0, 0);
        return root;
    }

    public static FlowLayoutPanel Bar(params Control[] controls)
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, Padding = new Padding(0, 4, 0, 5)
        };
        bar.Controls.AddRange(controls);
        return bar;
    }

    public static TextBox Text(string value = "", bool readOnly = false)
    {
        return new TextBox
        {
            Text = value,
            ReadOnly = readOnly,
            Width = 245
        };
    }

    // Build an input dialog from the fields provided by each page.
    public static bool EditDialog(IWin32Window owner, string title, params (string Label, Control Field)[] fields)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ClientSize = new Size(505, 105 + fields.Length * 49),
            Font = new Font("Segoe UI", 10)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(16, 18, 16, 10),
            ColumnCount = 2, RowCount = fields.Length + 1
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));

        for (int i = 0; i < fields.Length; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
            var label = new Label
            {
                Text = fields[i].Label, TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            fields[i].Field.Dock = DockStyle.Fill;
            fields[i].Field.Margin = new Padding(0, 4, 0, 10);
            table.Controls.Add(label, 0, i);
            table.Controls.Add(fields[i].Field, 1, i);
        }

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft
        };
        var save = Button("Save");
        var cancel = Button("Cancel", true);
        save.DialogResult = DialogResult.OK;
        cancel.DialogResult = DialogResult.Cancel;
        actions.Controls.Add(save);
        actions.Controls.Add(cancel);
        table.Controls.Add(actions, 0, fields.Length);
        table.SetColumnSpan(actions, 2);
        dialog.AcceptButton = save;
        dialog.CancelButton = cancel;
        dialog.Controls.Add(table);
        return dialog.ShowDialog(owner) == DialogResult.OK;
    }

    public static string? Selected(DataGridView grid, string column)
    {
        if (grid.CurrentRow != null)
        {
            object? value = grid.CurrentRow.Cells[column].Value;
            if (value != null)
                return Convert.ToString(value);
        }
        MessageBox.Show("Select a record first.", "Selection required",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    public static void Filter(DataGridView grid, string search, params string[] columns)
    {
        if (grid.DataSource is not DataTable table)
            return;

        string filter = search.Trim();

        if (filter.Length == 0)
        {
            table.DefaultView.RowFilter = "";
            return;
        }

        // Escape characters that have special meaning in DataTable filters.
        filter = filter.Replace("'", "''");
        filter = filter.Replace("[", "[[]");
        filter = filter.Replace("%", "[%]");
        filter = filter.Replace("*", "[*]");

        var conditions = new List<string>();
        foreach (string column in columns)
        {
            conditions.Add($"CONVERT([{column}], 'System.String') LIKE '%{filter}%'");
        }

        table.DefaultView.RowFilter = string.Join(" OR ", conditions);
    }
}
