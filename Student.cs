namespace EquipmentProgress;

// Stores the basic information of a student who may borrow equipment.
// The student registration form and database will be added in a later milestone.
public class Student
{
    public string StudentId { get; }
    public string FullName { get; private set; } = "";
    public string Course { get; private set; } = "";
    public string Section { get; private set; } = "";

    public Student(string studentId, string fullName, string course, string section)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            throw new ArgumentException("Student ID is required.");
        }

        StudentId = studentId.Trim();

        // Use this method to check and save the other student details.
        UpdateDetails(fullName, course, section);
    }

    public void UpdateDetails(string fullName, string course, string section)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Student name is required.");
        }

        FullName = fullName.Trim();
        Course = course.Trim();
        Section = section.Trim();
    }

    public override string ToString()
    {
        return StudentId + " - " + FullName;
    }
}
