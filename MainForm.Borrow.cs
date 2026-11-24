using System.Data;
using System.Drawing;

namespace EquipmentProgress;

public sealed partial class MainForm
{
    // Group borrower details, loan dates and item selection controls.
    private Control CreateBorrow()
    {
        var page = Ui.Page("Borrow Item", "Record a requisition for one or more individually tracked items");
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 198));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4,
            BackColor = Ui.LightBlue, Padding = new Padding(13)
        };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++)
            details.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        void Field(int row, int labelColumn, string label, Control control)
        {
            details.Controls.Add(new Label
            {
                Text = label, TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill, ForeColor = Ui.Navy
            }, labelColumn, row);
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(6, 5, 6, 7);
            details.Controls.Add(control, labelColumn + 1, row);
        }

        requisition.PlaceholderText = "e.g. REQ-2026-001";
        course.ReadOnly = true;
        section.ReadOnly = true;
        studentSelect.SelectedIndexChanged += (_, _) =>
        {
            if (studentSelect.SelectedItem is Student s)
            {
                course.Text = s.Course;
                section.Text = s.Section;
            }
            else { course.Clear(); section.Clear(); }
        };

        borrowAt.Value = DateTime.Now;
        dueAt.Value = DateTime.Now.AddHours(2);
        borrowAt.ValueChanged += (_, _) => RedrawCart();
        dueAt.ValueChanged += (_, _) => RedrawCart();

        Field(0, 0, "Requisition No:", requisition);
        Field(0, 2, "Course:", course);
        Field(1, 0, "Student Name:", studentSelect);
        Field(1, 2, "Section:", section);
        Field(2, 0, "Borrower Type:", Ui.Text("Student", readOnly: true));
        Field(2, 2, "Date / Time From:", borrowAt);
        Field(3, 0, "Return Deadline:", dueAt);

        var selection = Ui.Bar(
            new Label
            {
                Text = "Item Name:", Width = 112,
                TextAlign = ContentAlignment.MiddleRight, Height = 35
            },
            itemSelect
        );
        var add = Ui.Button("Add Item");
        var remove = Ui.Button("Remove Selected", true);
        var refresh = Ui.Button("Refresh Available", true);
        selection.Controls.AddRange([add, remove, refresh]);

        var actions = Ui.Bar();
        actions.FlowDirection = FlowDirection.RightToLeft;
        var save = Ui.Button("Save Borrowing");
        save.Enabled = false;
        var clear = Ui.Button("Clear", true);
        actions.Controls.AddRange([save, clear]);

        body.Controls.Add(details, 0, 0);
        body.Controls.Add(selection, 0, 1);
        body.Controls.Add(borrowGrid, 0, 2);
        body.Controls.Add(actions, 0, 3);
        page.Controls.Add(body, 0, 1);

        add.Click += (_, _) => AddBorrowItem();
        remove.Click += (_, _) => RemoveBorrowItem();
        refresh.Click += (_, _) => Try(LoadBorrowSelectors);
        clear.Click += (_, _) => ClearBorrowForm();
        save.Click += (_, _) => SaveBorrowing();
        ClearBorrowForm();
        return page;
    }

    // Refresh the borrower list and filter out equipment already selected.
    private void LoadBorrowSelectors()
    {
        string? id = (studentSelect.SelectedItem as Student)?.Id;
        studentSelect.Items.Clear();
        foreach (DataRow row in db.Students().Rows)
        {
            var student = new Student(
                row["ID"].ToString() ?? "",
                row["Name"].ToString() ?? "",
                row["Course"].ToString() ?? "",
                row["Section"].ToString() ?? "");
            studentSelect.Items.Add(student);
            if (student.Id == id) studentSelect.SelectedItem = student;
        }
        if (studentSelect.SelectedIndex < 0 && studentSelect.Items.Count > 0)
            studentSelect.SelectedIndex = 0;

        itemSelect.Items.Clear();
        foreach (DataRow row in db.AvailableEquipment().Rows)
        {
            string availableId = row["ID"].ToString() ?? "";
            bool alreadySelected = false;
            foreach (Equipment selected in cart)
            {
                if (selected.Id == availableId)
                    alreadySelected = true;
            }

            if (alreadySelected)
                continue;
            itemSelect.Items.Add(Equipment.Create(
                availableId,
                row["Name"].ToString() ?? "",
                row["Category"].ToString() ?? "",
                row["Notes"].ToString() ?? ""));
        }
        if (itemSelect.Items.Count > 0) itemSelect.SelectedIndex = 0;
        RedrawCart();
    }

    // Prevent the same equipment item from appearing twice in the selection.
    private void AddBorrowItem()
    {
        if (itemSelect.SelectedItem is not Equipment equipment)
        {
            MessageBox.Show(this, "Add equipment under Equipment first, or refresh availability.");
            return;
        }
        foreach (Equipment selected in cart)
        {
            if (selected.Id == equipment.Id)
            {
                MessageBox.Show(this, "You have already selected this item.");
                return;
            }
        }
        cart.Add(equipment);
        Try(LoadBorrowSelectors);
    }

    private void RemoveBorrowItem()
    {
        string? id = Ui.Selected(borrowGrid, "ItemID");
        if (id is null) return;
        for (int i = cart.Count - 1; i >= 0; i--)
        {
            if (cart[i].Id == id)
                cart.RemoveAt(i);
        }

        Try(LoadBorrowSelectors);
    }

    // Rebuild the table when selected items or loan dates change.
    private void RedrawCart()
    {
        var table = new DataTable();
        table.Columns.Add("DateBorrow");
        table.Columns.Add("From");
        table.Columns.Add("To");
        table.Columns.Add("ItemID");
        table.Columns.Add("ItemName");
        table.Columns.Add("Quantity");
        foreach (Equipment equipment in cart)
            table.Rows.Add(borrowAt.Value.ToString("MM/dd/yyyy"),
                borrowAt.Value.ToString("hh:mm tt"),
                dueAt.Value.ToString("hh:mm tt"),
                equipment.Id, equipment.Name, 1);
        borrowGrid.DataSource = table;
    }

    // Reset the selected items and prepare a new requisition number.
    private void ClearBorrowForm()
    {
        cart.Clear();
        requisition.Text = "REQ-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff");
        borrowAt.Value = DateTime.Now;
        dueAt.Value = DateTime.Now.AddHours(2);
        RedrawCart();
        if (IsHandleCreated) Try(LoadBorrowSelectors);
    }

    private void SaveBorrowing()
    {
        Try(() =>
        {
            if (studentSelect.SelectedItem is not Student student)
                throw new ArgumentException("Please register and select a student.");
            if (cart.Count == 0)
                throw new ArgumentException("Select at least one available item.");
            var selectedIds = new List<string>();
            foreach (Equipment item in cart)
                selectedIds.Add(item.Id);

            db.Borrow(requisition.Text, student.Id, borrowAt.Value, dueAt.Value,
                selectedIds, account.Id);
            MessageBox.Show(this, $"Saved borrowing for {cart.Count} item(s).",
                "Borrow item", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearBorrowForm();
            RefreshAll();
            status.Text = "Borrowing saved";
        });
    }
}
