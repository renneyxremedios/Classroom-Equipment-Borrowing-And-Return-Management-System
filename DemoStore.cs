using System.Data;
using System.Linq;

namespace EquipmentProgress;

/// <summary>
/// Provides a local in-memory data source for the current application screens.
/// Data is initialized at startup and is not saved between sessions.
/// </summary>
public sealed class DemoStore
{
    private readonly List<Equipment> equipment = new();
    private readonly List<Student> students = new();
    private readonly List<SampleLoan> sampleLoans = new();

    public DemoStore()
    {
        equipment.Add(EquipmentProgress.Equipment.Create("EQ-001", "LCD Projector", "Electronic", "HDMI cable included"));
        equipment.Add(EquipmentProgress.Equipment.Create("EQ-002", "Laptop", "Electronic", "Power adapter included"));
        equipment.Add(EquipmentProgress.Equipment.Create("EQ-003", "Whiteboard", "Non-Electronic", "Portable"));
        equipment.Add(EquipmentProgress.Equipment.Create("EQ-004", "Folding Chair", "Non-Electronic", "Blue"));

        students.Add(new Student("ST-001", "Alex Santos", "BSCS", "1A"));
        students.Add(new Student("ST-002", "Jamie Cruz", "BSIT", "2B"));
        students.Add(new Student("ST-003", "Taylor Reyes", "BSCS", "3A"));

        // Initial borrowing records populate the dashboard and history views.
        equipment[1].MarkBorrowed();
        sampleLoans.Add(new SampleLoan(1, "SAMPLE-REQ-001", "ST-001",
            "EQ-002", DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1), null));
        sampleLoans.Add(new SampleLoan(2, "SAMPLE-REQ-002", "ST-002",
            "EQ-003", DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-4),
            DateTime.Now.AddDays(-4).AddHours(-1)));
    }

    // Calculate dashboard counts using the records in memory.
    public (int Total, int Available, int Borrowed, int Overdue, int Students) Summary()
    {
        int available = equipment.Count(x => x.IsAvailable);
        int overdue = sampleLoans.Count(x => !x.ReturnedAt.HasValue && x.DueAt < DateTime.Now);
        return (equipment.Count, available, equipment.Count - available, overdue, students.Count);
    }

    public DataTable Equipment()
    {
        var table = new DataTable();
        table.Columns.Add("ID");
        table.Columns.Add("Name");
        table.Columns.Add("Category");
        table.Columns.Add("Notes");
        table.Columns.Add("Status");
        foreach (var item in equipment)
            table.Rows.Add(item.Id, item.Name, item.Category, item.Notes,
                item.IsAvailable ? "Available" : "Borrowed (Sample)");
        return table;
    }

    // Only available items can be selected for a borrowing request.
    public DataTable AvailableEquipment()
    {
        var table = new DataTable();
        table.Columns.Add("ID");
        table.Columns.Add("Name");
        table.Columns.Add("Category");
        table.Columns.Add("Notes");
        foreach (var item in equipment.Where(x => x.IsAvailable))
            table.Rows.Add(item.Id, item.Name, item.Category, item.Notes);
        return table;
    }

    public DataTable Students()
    {
        var table = new DataTable();
        table.Columns.Add("ID");
        table.Columns.Add("Name");
        table.Columns.Add("Course");
        table.Columns.Add("Section");
        foreach (var student in students)
            table.Rows.Add(student.Id, student.Name, student.Course, student.Section);
        return table;
    }

    // Shape borrowing records for the history and return tables.
    public DataTable Loans(bool activeOnly = false)
    {
        var table = new DataTable();
        foreach (string column in new[]
        {
            "LoanID", "Requisition", "StudentID", "Borrower", "EquipmentID",
            "Equipment", "BorrowedAt", "DueAt", "ReturnedAt", "Status"
        })
            table.Columns.Add(column);

        foreach (var loan in sampleLoans)
        {
            if (activeOnly && loan.ReturnedAt.HasValue) continue;
            string borrower = students.FirstOrDefault(x => x.Id == loan.StudentId)?.Name ?? "";
            string item = equipment.FirstOrDefault(x => x.Id == loan.EquipmentId)?.Name ?? "";
            string status = loan.ReturnedAt.HasValue ? "RETURNED (SAMPLE)" :
                loan.DueAt < DateTime.Now ? "OVERDUE (SAMPLE)" : "BORROWED (SAMPLE)";
            table.Rows.Add(loan.Id.ToString(), loan.Requisition, loan.StudentId, borrower,
                loan.EquipmentId, item, loan.BorrowedAt.ToString("yyyy-MM-dd HH:mm"),
                loan.DueAt.ToString("yyyy-MM-dd HH:mm"),
                loan.ReturnedAt?.ToString("yyyy-MM-dd HH:mm") ?? "", status);
        }
        return table;
    }

    // Registration currently updates the active session only.
    public void SaveEquipment(Equipment item, bool editing)
    {
        if (editing)
            throw new NotSupportedException("Equipment editing is not available yet.");
        if (equipment.Any(x => x.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("An equipment item with this ID already exists.");
        equipment.Add(item);
    }

    public void SaveStudent(Student student, bool editing)
    {
        if (editing)
            throw new NotSupportedException("Student editing is not available yet.");
        if (students.Any(x => x.Id.Equals(student.Id, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("A student with this ID already exists.");
        students.Add(student);
    }

    public void DeleteEquipment(string id) =>
        throw new NotSupportedException("Deleting records is not available yet.");

    public void DeleteStudent(string id) =>
        throw new NotSupportedException("Deleting records is not available yet.");

    public void Borrow(string requisition, string studentId, DateTime borrowedAt,
        DateTime dueAt, List<string> equipmentIds, int administratorId) =>
        throw new NotSupportedException("Saving borrowing records is not available yet.");

    public void Return(long loanId, int administratorId) =>
        throw new NotSupportedException("Return processing is not available yet.");

    private sealed record SampleLoan(long Id, string Requisition, string StudentId,
        string EquipmentId, DateTime BorrowedAt, DateTime DueAt, DateTime? ReturnedAt);
}
