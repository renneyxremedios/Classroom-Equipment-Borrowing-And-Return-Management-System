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

// a working equipment list without database storage.
// Login, students, borrowing, and returning will be added later.
public class MainWindow : Form
{
    private readonly TextBox equipmentName = new TextBox();
    private readonly ComboBox category = new ComboBox();
    private readonly ListBox equipmentList = new ListBox();

    public MainWindow()
    {
        Text = "Classroom Equipment";
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
        description.Text = "Equipment classes and handling instructions";
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
        equipmentList.DoubleClick += ShowHandlingInstructions;

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

        Equipment item;

        if (category.SelectedItem?.ToString() == "Electronic")
        {
            item = new ElectronicEquipment(name);
        }
        else
        {
            item = new NonElectronicEquipment(name);
        }

        equipmentList.Items.Add(item);
        equipmentName.Clear();
        equipmentName.Focus();
    }

    private void ShowHandlingInstructions(object? sender, EventArgs e)
    {
        if (equipmentList.SelectedItem is Equipment item)
        {
            MessageBox.Show(item.HandlingInstructions(), item.Name + " - Handling");
        }
    }
}
