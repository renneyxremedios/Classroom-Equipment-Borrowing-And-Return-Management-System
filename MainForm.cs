using System.Data;
using System.Drawing;

namespace EquipmentProgress;

public sealed partial class MainForm : Form
{
    private readonly DemoStore db;
    private readonly Administrator account;
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly Label status = new() { Text = "Local session • changes are not saved after closing", Dock = DockStyle.Fill, ForeColor = Ui.Gray };

    // Each page uses its own grid to display the relevant records.
    private readonly DataGridView equipmentGrid = Ui.Grid();
    private readonly DataGridView studentsGrid = Ui.Grid();
    private readonly DataGridView returnsGrid = Ui.Grid();
    private readonly DataGridView historyGrid = Ui.Grid();
    private readonly DataGridView dashboardGrid = Ui.Grid();
    private readonly DataGridView borrowGrid = Ui.Grid();

    private readonly TextBox equipmentSearch = Ui.Text();
    private readonly TextBox studentSearch = Ui.Text();
    private readonly TextBox historySearch = Ui.Text();

    private readonly Label totalCount = Metric();
    private readonly Label availableCount = Metric();
    private readonly Label borrowedCount = Metric();
    private readonly Label overdueCount = Metric();
    private readonly Label studentCount = Metric();

    // Borrowing controls are shared with the borrowing page.
    private readonly TextBox requisition = Ui.Text();
    private readonly TextBox course = Ui.Text(readOnly: true);
    private readonly TextBox section = Ui.Text(readOnly: true);
    private readonly ComboBox studentSelect = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 285 };
    private readonly ComboBox itemSelect = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 315 };
    private readonly DateTimePicker borrowAt = DateTimeInput();
    private readonly DateTimePicker dueAt = DateTimeInput();
    private readonly List<Equipment> cart = new List<Equipment>();

    public bool LoggedOut { get; private set; }

    public MainForm(DemoStore database, Administrator signedIn)
    {
        db = database;
        account = signedIn;
        Text = "Classroom Equipment | Borrowing and Return";
        Font = new Font("Segoe UI", 10);
        MinimumSize = new Size(1030, 680);
        Size = new Size(1280, 790);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        var top = new TableLayoutPanel
        {
            Height = 67, Dock = DockStyle.Top, BackColor = Ui.Navy,
            ColumnCount = 2, RowCount = 1, Margin = Padding.Empty
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        top.Controls.Add(new Label
        {
            Text = "CLASSROOM EQUIPMENT / BORROWING & RETURN",
            ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(22, 0, 0, 0),
            AutoEllipsis = true
        }, 0, 0);

        var accountArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
            Margin = Padding.Empty, BackColor = Ui.Navy
        };
        accountArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        accountArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));

        accountArea.Controls.Add(new Label
        {
            Text = $"Signed in: {account.DisplayName}",
            ForeColor = Color.White, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        }, 0, 0);

        var logout = Ui.Button("Log out", secondary: true);
        logout.AutoSize = false;
        logout.Dock = DockStyle.Fill;
        logout.Margin = new Padding(10, 15, 14, 15);
        logout.Click += (_, _) =>
        {
            LoggedOut = true;
            Close();
        };
        accountArea.Controls.Add(logout, 1, 0);
        top.Controls.Add(accountArea, 1, 0);

        var footer = new Panel { Height = 30, Dock = DockStyle.Bottom, Padding = new Padding(15, 5, 15, 4) };
        footer.Controls.Add(status);

        tabs.TabPages.Add(MakeTab("Dashboard", CreateDashboard()));
        tabs.TabPages.Add(MakeTab("Borrow Item", CreateBorrow()));
        tabs.TabPages.Add(MakeTab("Return Item", CreateReturns()));
        tabs.TabPages.Add(MakeTab("Equipment", CreateEquipment()));
        tabs.TabPages.Add(MakeTab("Students", CreateStudents()));
        tabs.TabPages.Add(MakeTab("Borrowing History", CreateHistory()));
        tabs.SelectedIndexChanged += (_, _) => ReloadSelected();

        Controls.Add(tabs);
        Controls.Add(footer);
        Controls.Add(top);

        Shown += (_, _) => RefreshAll();
    }

    private static TabPage MakeTab(string title, Control page)
    {
        var tab = new TabPage(title) { BackColor = Color.White, Padding = new Padding(4) };
        tab.Controls.Add(page);
        return tab;
    }

    // Refresh only the page currently selected by the user.
    private void ReloadSelected()
    {
        Try(() =>
        {
            string page = tabs.SelectedTab?.Text ?? "";
            switch (page)
            {
                case "Dashboard":
                    LoadDashboard();
                    break;
                case "Borrow Item":
                    LoadBorrowSelectors();
                    break;
                case "Return Item":
                    LoadReturns();
                    break;
                case "Equipment":
                    LoadEquipment();
                    break;
                case "Students":
                    LoadStudents();
                    break;
                case "Borrowing History":
                    LoadHistory();
                    break;
            }
        });
    }

    // Reload related views after an equipment or student record changes.
    private void RefreshAll()
    {
        Try(() =>
        {
            LoadDashboard();
            LoadEquipment();
            LoadStudents();
            LoadBorrowSelectors();
            LoadReturns();
            LoadHistory();
            status.Text = "Records refreshed • session data only";
        });
    }

    private void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception error)
        {
            status.Text = "Operation failed";
            MessageBox.Show(this, error.Message, "Operation failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static Label Metric() => new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 19, FontStyle.Bold),
        ForeColor = Ui.Navy
    };

    private static DateTimePicker DateTimeInput()
    {
        return new DateTimePicker
        {
            CustomFormat = "yyyy-MM-dd HH:mm",
            Format = DateTimePickerFormat.Custom,
            ShowUpDown = true,
            Width = 220
        };
    }

    private void LoadDashboard()
    {
        var (total, available, borrowed, overdue, students) = db.Summary();
        totalCount.Text = total.ToString();
        availableCount.Text = available.ToString();
        borrowedCount.Text = borrowed.ToString();
        overdueCount.Text = overdue.ToString();
        studentCount.Text = students.ToString();
        dashboardGrid.DataSource = db.Loans();
    }

    private void LoadEquipment()
    {
        equipmentGrid.DataSource = db.Equipment();
        Ui.Filter(equipmentGrid, equipmentSearch.Text, "ID", "Name");
    }

    private void LoadStudents()
    {
        studentsGrid.DataSource = db.Students();
        Ui.Filter(studentsGrid, studentSearch.Text, "ID", "Name", "Course", "Section");
    }

    private void LoadReturns()
    {
        returnsGrid.DataSource = db.Loans(activeOnly: true);
    }

    private void LoadHistory()
    {
        historyGrid.DataSource = db.Loans();
        Ui.Filter(historyGrid, historySearch.Text, "Requisition", "StudentID", "Borrower", "Equipment", "EquipmentID");
    }
}
