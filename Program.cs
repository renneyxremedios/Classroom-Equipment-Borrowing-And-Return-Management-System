using System.Drawing;
using System.Windows.Forms;

namespace EquipmentProgress;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }
}

// equipment names and categories are stored in a temporary list.
// Database, student records, login, borrowing, and returns will be added later.
public class MainWindow : Form
{
    private readonly TextBox equipmentName = new TextBox();
    private readonly ComboBox category = new ComboBox();
    private readonly ListBox equipmentList = new ListBox();

    public MainWindow()
    {
        Text = "Classroom Equipment - Progress Version 1";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(600, 430);
        MinimumSize = new Size(520, 380);
        Font = new Font("Segoe UI", 10);

        var heading = new Label();
        heading.Text = "Classroom Equipment";
        heading.Font = new Font("Segoe UI", 17, FontStyle.Bold);
        heading.Location = new Point(25, 20);
        heading.AutoSize = true;

        var description = new Label();
        description.Text = "Progress 1: add equipment to a temporary list";
        description.Location = new Point(25, 62);
        description.AutoSize = true;

        var nameLabel = new Label();
        nameLabel.Text = "Equipment name:";
        nameLabel.Location = new Point(25, 105);
        nameLabel.AutoSize = true;

        equipmentName.Location = new Point(175, 100);
        equipmentName.Width = 365;

        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        categoryLabel.Location = new Point(25, 145);
        categoryLabel.AutoSize = true;

        category.Location = new Point(175, 140);
        category.Width = 200;
        category.DropDownStyle = ComboBoxStyle.DropDownList;
        category.Items.Add("Electronic");
        category.Items.Add("Non-Electronic");
        category.SelectedIndex = 0;

        var addButton = new Button();
        addButton.Text = "Add Equipment";
        addButton.Location = new Point(395, 140);
        addButton.Width = 145;
        addButton.Click += AddEquipment;

        equipmentList.Location = new Point(25, 195);
        equipmentList.Size = new Size(515, 145);
        equipmentList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        Controls.Add(heading);
        Controls.Add(description);
        Controls.Add(nameLabel);
        Controls.Add(equipmentName);
        Controls.Add(categoryLabel);
        Controls.Add(category);
        Controls.Add(addButton);
        Controls.Add(equipmentList);
        AcceptButton = addButton;
    }

    private void AddEquipment(object? sender, EventArgs e)
    {
        string name = equipmentName.Text.Trim();

        if (name.Length == 0)
        {
            MessageBox.Show("Please enter an equipment name.");
            equipmentName.Focus();
            return;
        }

        string selectedCategory = category.SelectedItem?.ToString() ?? "Electronic";
        equipmentList.Items.Add(name + " (" + selectedCategory + ")");
        equipmentName.Clear();
        equipmentName.Focus();
    }
}
