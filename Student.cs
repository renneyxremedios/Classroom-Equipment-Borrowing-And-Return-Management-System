namespace EquipmentProgress;

// Represents a student registered to borrow classroom equipment.
public sealed class Student
{
    public string Id { get; }
    public string Name { get; private set; } = "";
    public string Course { get; private set; } = "";
    public string Section { get; private set; } = "";

    // Check the ID before storing the remaining student details.
    public Student(string id, string name, string course, string section)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Student ID is required.");

        Id = id.Trim();
        UpdateDetails(name, course, section);
    }

    // Validate the name and normalize text entered through the form.
    public void UpdateDetails(string name, string course, string section)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student full name is required.");

        Name = name.Trim();
        Course = course.Trim();
        Section = section.Trim();
    }

    // Show a readable label in student selection controls.
    public override string ToString()
    {
        return Id + " - " + Name;
    }
}
