using System.Data;
using System.Drawing;

namespace EquipmentProgress;

public sealed partial class MainForm
{
    // Display equipment totals and a quick view of borrowing activity.
    private Control CreateDashboard()
    {
        var page = Ui.Page("Dashboard", "Current equipment availability and recent borrowing activity");
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1,
            BackColor = Color.White
        };
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5 };
        for (int i = 0; i < 5; i++)
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        var metricData = new (string Name, Label Value)[]
        {
            ("Total Items", totalCount), ("Available", availableCount),
            ("Borrowed", borrowedCount), ("Overdue", overdueCount),
            ("Students", studentCount)
        };
        for (int i = 0; i < metricData.Length; i++)
        {
            var card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2,
                BackColor = Ui.LightBlue, Margin = new Padding(6),
                Padding = new Padding(7)
            };
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            card.Controls.Add(new Label
            {
                Text = metricData[i].Name, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Ui.Gray
            }, 0, 0);
            card.Controls.Add(metricData[i].Value, 0, 1);
            metrics.Controls.Add(card, i, 0);
        }
        body.Controls.Add(metrics, 0, 0);

        var header = Ui.Bar(new Label
        {
            Text = "Borrowing activity", AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Padding = new Padding(0, 10, 20, 0)
        });
        var refresh = Ui.Button("Refresh", true);
        refresh.Click += (_, _) => Try(LoadDashboard);
        header.Controls.Add(refresh);
        body.Controls.Add(header, 0, 1);
        body.Controls.Add(dashboardGrid, 0, 2);
        page.Controls.Add(body, 0, 1);
        return page;
    }

    // Arrange the equipment table, search box and action buttons.
    private Control CreateEquipment()
    {
        var page = Ui.Page("Equipment Management", "Register, update, search, and classify classroom equipment");
        var host = new Panel { Dock = DockStyle.Fill };
        equipmentSearch.Width = 200;
        equipmentSearch.PlaceholderText = "Search ID or name";

        var add = Ui.Button("+ Add Equipment");
        var edit = Ui.Button("Edit", true);
        var delete = Ui.Button("Delete", true);
        edit.Enabled = false;
        delete.Enabled = false;
        var handling = Ui.Button("Handling Notes", true);
        var refresh = Ui.Button("Refresh", true);
        var bar = Ui.Bar(equipmentSearch, add, edit, delete, handling, refresh);
        equipmentSearch.TextChanged += (_, _) => Ui.Filter(
            equipmentGrid, equipmentSearch.Text, "ID", "Name");

        add.Click += (_, _) => EditEquipment(false);
        edit.Click += (_, _) => EditEquipment(true);
        delete.Click += (_, _) => DeleteEquipment();
        handling.Click += (_, _) => ShowHandling();
        refresh.Click += (_, _) => Try(LoadEquipment);

        host.Controls.Add(equipmentGrid);
        host.Controls.Add(bar);
        page.Controls.Add(host, 0, 1);
        return page;
    }

    private void EditEquipment(bool editing)
    {
        var row = equipmentGrid.CurrentRow;
        if (editing && row is null)
        {
            MessageBox.Show(this, "Select equipment first.");
            return;
        }

        // Start with empty fields when adding, or fill them from the selected row when editing.
        string idValue = "";
        string nameValue = "";
        string notesValue = "";

        if (editing && row != null)
        {
            idValue = Convert.ToString(row.Cells["ID"].Value) ?? "";
            nameValue = Convert.ToString(row.Cells["Name"].Value) ?? "";
            notesValue = Convert.ToString(row.Cells["Notes"].Value) ?? "";
        }

        var id = Ui.Text(idValue);
        var name = Ui.Text(nameValue);
        var notes = Ui.Text(notesValue);
        var category = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 245
        };
        category.Items.Add("Electronic");
        category.Items.Add("Non-Electronic");

        if (editing && row != null)
            category.SelectedItem = Convert.ToString(row.Cells["Category"].Value);
        else
            category.SelectedItem = "Electronic";
        id.ReadOnly = editing;
        if (!Ui.EditDialog(this, editing ? "Edit Equipment" : "Add Equipment",
                ("Equipment ID", id), ("Name", name), ("Category", category), ("Notes", notes))) return;

        Try(() =>
        {
            var item = Equipment.Create(id.Text, name.Text, category.SelectedItem?.ToString() ?? "", notes.Text);
            db.SaveEquipment(item, editing);
            RefreshAll();
            status.Text = "Equipment saved";
        });
    }

    private void DeleteEquipment()
    {
        var id = Ui.Selected(equipmentGrid, "ID");
        if (id is null) return;
        if (MessageBox.Show(this, $"Delete equipment {id}?\n"
            + "An item with borrowing history cannot be deleted.", "Confirm deletion",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Try(() => { db.DeleteEquipment(id); RefreshAll(); });
    }

    private void ShowHandling()
    {
        var row = equipmentGrid.CurrentRow;
        if (row is null) return;
        Try(() =>
        {
            Equipment item = Equipment.Create(
                Convert.ToString(row.Cells["ID"].Value) ?? "",
                Convert.ToString(row.Cells["Name"].Value) ?? "",
                Convert.ToString(row.Cells["Category"].Value) ?? "",
                Convert.ToString(row.Cells["Notes"].Value) ?? "");
            MessageBox.Show(this, item.HandlingInstructions(),
                $"{item.Name} - Handling", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    // Keep student registration and searching on one page.
    private Control CreateStudents()
    {
        var page = Ui.Page("Students", "Maintain student IDs, names, courses, and sections");
        var host = new Panel { Dock = DockStyle.Fill };
        studentSearch.Width = 210;
        studentSearch.PlaceholderText = "Search students";
        var add = Ui.Button("+ Add Student");
        var edit = Ui.Button("Edit", true);
        var delete = Ui.Button("Delete", true);
        edit.Enabled = false;
        delete.Enabled = false;
        var refresh = Ui.Button("Refresh", true);
        var bar = Ui.Bar(studentSearch, add, edit, delete, refresh);

        studentSearch.TextChanged += (_, _) => Ui.Filter(
            studentsGrid, studentSearch.Text, "ID", "Name", "Course", "Section");
        add.Click += (_, _) => EditStudent(false);
        edit.Click += (_, _) => EditStudent(true);
        delete.Click += (_, _) => DeleteStudent();
        refresh.Click += (_, _) => Try(LoadStudents);

        host.Controls.Add(studentsGrid);
        host.Controls.Add(bar);
        page.Controls.Add(host, 0, 1);
        return page;
    }

    private void EditStudent(bool editing)
    {
        var row = studentsGrid.CurrentRow;
        if (editing && row is null)
        {
            MessageBox.Show(this, "Select a student first.");
            return;
        }

        string idValue = "";
        string nameValue = "";
        string courseValue = "";
        string sectionValue = "";

        if (editing && row != null)
        {
            idValue = Convert.ToString(row.Cells["ID"].Value) ?? "";
            nameValue = Convert.ToString(row.Cells["Name"].Value) ?? "";
            courseValue = Convert.ToString(row.Cells["Course"].Value) ?? "";
            sectionValue = Convert.ToString(row.Cells["Section"].Value) ?? "";
        }

        var id = Ui.Text(idValue);
        var name = Ui.Text(nameValue);
        var courseField = Ui.Text(courseValue);
        var sectionField = Ui.Text(sectionValue);
        id.ReadOnly = editing;

        if (!Ui.EditDialog(this, editing ? "Edit Student" : "Add Student",
                ("Student ID", id), ("Full name", name),
                ("Course", courseField), ("Section", sectionField))) return;
        Try(() =>
        {
            db.SaveStudent(new Student(id.Text, name.Text, courseField.Text, sectionField.Text), editing);
            RefreshAll();
            status.Text = "Student saved";
        });
    }

    private void DeleteStudent()
    {
        string? id = Ui.Selected(studentsGrid, "ID");
        if (id is null) return;
        if (MessageBox.Show(this, $"Delete student {id}?\n"
            + "Students with borrowing history cannot be deleted.", "Confirm deletion",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Try(() => { db.DeleteStudent(id); RefreshAll(); });
    }

    // Show outstanding items in the return view.
    private Control CreateReturns()
    {
        var page = Ui.Page("Return Item", "Receive borrowed items and make them available again");
        var host = new Panel { Dock = DockStyle.Fill };
        var receive = Ui.Button("Mark Selected as Returned");
        receive.Enabled = false;
        var refresh = Ui.Button("Refresh", true);
        var bar = Ui.Bar(receive, refresh);
        receive.Click += (_, _) =>
        {
            string? loan = Ui.Selected(returnsGrid, "LoanID");
            if (loan is null || !long.TryParse(loan, out long loanId)) return;
            if (MessageBox.Show(this, "Mark this equipment as returned?",
                    "Confirm return", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            Try(() =>
            {
                db.Return(loanId, account.Id);
                RefreshAll();
                status.Text = "Return recorded";
            });
        };
        refresh.Click += (_, _) => Try(LoadReturns);
        host.Controls.Add(returnsGrid);
        host.Controls.Add(bar);
        page.Controls.Add(host, 0, 1);
        return page;
    }

    // Provide a searchable view of borrowing records.
    private Control CreateHistory()
    {
        var page = Ui.Page("Borrowing History", "Borrowed, overdue, and returned equipment transactions");
        var host = new Panel { Dock = DockStyle.Fill };
        historySearch.Width = 285;
        historySearch.PlaceholderText = "Search borrower, requisition or equipment";
        var refresh = Ui.Button("Refresh", true);
        var bar = Ui.Bar(historySearch, refresh);
        historySearch.TextChanged += (_, _) => Ui.Filter(
            historyGrid, historySearch.Text, "Requisition", "StudentID", "Borrower", "Equipment", "EquipmentID");
        refresh.Click += (_, _) => Try(LoadHistory);
        host.Controls.Add(historyGrid);
        host.Controls.Add(bar);
        page.Controls.Add(host, 0, 1);
        return page;
    }
}
