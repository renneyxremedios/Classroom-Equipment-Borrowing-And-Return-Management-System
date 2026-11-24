using System.Drawing;

namespace EquipmentProgress;

public sealed class LoginForm : Form
{
    private readonly DemoStore db;
    private readonly bool initialSetup;
    // Account fields share the same input styles as the other forms.
    private readonly TextBox username = Ui.Text();
    private readonly TextBox displayName = Ui.Text();
    private readonly TextBox password = Ui.Text();
    private readonly TextBox confirmation = Ui.Text();

    public LoginForm(DemoStore database)
    {
        db = database;
        initialSetup = false; // Account provisioning will be added with authentication.

        Text = "Classroom Equipment | Login";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 520);
        Size = new Size(900, 550);
        Font = new Font("Segoe UI", 10);

        // Keep the system title separate from the entry form.
        var brand = new Panel { Dock = DockStyle.Left, Width = 350, BackColor = Ui.Navy };
        brand.Controls.Add(new Label
        {
            Text = "CLASSROOM\nEQUIPMENT\n\nBorrowing and Return\nManagement System",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White, Dock = DockStyle.Fill, Padding = new Padding(28, 90, 15, 0)
        });

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(35, 42, 35, 24),
            ColumnCount = 1,
            RowCount = 9,
            AutoScroll = true,
            BackColor = Color.White
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Add(Control control, int height)
        {
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            control.Dock = control is Label ? DockStyle.Fill : DockStyle.Top;
            form.Controls.Add(control);
        }

        Add(new Label
        {
            Text = initialSetup ? "Create administrator" : "Welcome",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Ui.Navy,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        }, 62);
        Add(new Label
        {
            Text = initialSetup ? "First launch: create your login." : "Classroom equipment management",
            ForeColor = Ui.Gray
        }, 37);

        username.PlaceholderText = "Username";
        displayName.PlaceholderText = "Full name";
        password.PlaceholderText = "Password";
        confirmation.PlaceholderText = "Confirm password";
        password.UseSystemPasswordChar = true;
        confirmation.UseSystemPasswordChar = true;

        Add(username, 46);
        if (initialSetup) Add(displayName, 46);
        Add(password, 46);
        if (initialSetup) Add(confirmation, 46);

        var submit = Ui.Button(initialSetup ? "Create admin account" : "Open Workspace");
        submit.Height = 40;
        submit.Click += (_, _) => Submit();
        Add(submit, 55);

        Add(new Label
        {
            Text = initialSetup
                ? "Use at least 8 characters. There is no default password."
                : "",
            ForeColor = Ui.Gray
        }, 58);

        Controls.Add(form);
        Controls.Add(brand);
        AcceptButton = submit;
    }

    private void Submit()
    {
        // Open the main workspace while account verification is under development.
        using var main = new MainForm(db, new Administrator(0, "local", "Local User"));
        Hide();
        main.ShowDialog(this);
        Show();
    }
}
