namespace EquipmentProgress;

// Common equipment information and availability tracking shared by each category.
public abstract class Equipment
{
    public string Id { get; }
    public string Name { get; }
    public string Notes { get; }
    public bool IsAvailable { get; private set; } = true;

    protected Equipment(string id, string name, string notes)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Equipment ID is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.");

        Id = id.Trim();
        Name = name.Trim();
        Notes = notes.Trim();
    }

    public void MarkBorrowed()
    {
        IsAvailable = false;
    }

    public void MarkAvailable()
    {
        IsAvailable = true;
    }

    public abstract string Category { get; }

    // Each equipment type explains how it should be handled.
    public abstract string HandlingInstructions();

    public override string ToString()
    {
        return Id + " - " + Name;
    }

    // Create the matching equipment type from the selected category.
    public static Equipment Create(string id, string name, string category, string notes)
    {
        if (category == "Electronic")
            return new ElectronicEquipment(id, name, notes);

        if (category == "Non-Electronic")
            return new NonElectronicEquipment(id, name, notes);

        throw new ArgumentException("Choose an equipment category.");
    }
}

public sealed class ElectronicEquipment : Equipment
{
    public ElectronicEquipment(string id, string name, string notes)
        : base(id, name, notes)
    {
    }

    public override string Category
    {
        get { return "Electronic"; }
    }

    public override string HandlingInstructions()
    {
        return "Switch off power and check cables.";
    }
}

public sealed class NonElectronicEquipment : Equipment
{
    public NonElectronicEquipment(string id, string name, string notes)
        : base(id, name, notes)
    {
    }

    public override string Category
    {
        get { return "Non-Electronic"; }
    }

    public override string HandlingInstructions()
    {
        return "Inspect for damage and clean before returning.";
    }
}

// One transaction records one student borrowing one item.
// Several transactions can share the same requisition number.
public sealed class BorrowingTransaction
{
    public string RequisitionNo { get; }
    public string StudentId { get; }
    public string EquipmentId { get; }
    public DateTime BorrowedAt { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }

    public BorrowingTransaction(string requisitionNo, string studentId, string equipmentId,
                                DateTime borrowedAt, DateTime dueAt)
    {
        if (string.IsNullOrWhiteSpace(requisitionNo) ||
            string.IsNullOrWhiteSpace(studentId) ||
            string.IsNullOrWhiteSpace(equipmentId))
            throw new ArgumentException("Requisition, student, and equipment are required.");

        RequisitionNo = requisitionNo.Trim();
        StudentId = studentId.Trim();
        EquipmentId = equipmentId.Trim();
        RecordBorrowing(borrowedAt, dueAt);
    }

    public void RecordBorrowing(DateTime borrowedAt, DateTime dueAt)
    {
        if (dueAt <= borrowedAt)
            throw new ArgumentException("The return deadline must be after borrowing time.");

        BorrowedAt = borrowedAt;
        DueAt = dueAt;
        ReturnedAt = null;
    }

    public void RecordReturn(DateTime returnedAt)
    {
        if (ReturnedAt != null)
            throw new InvalidOperationException("This equipment was already returned.");

        ReturnedAt = returnedAt;
    }

    // Returned items are excluded from the overdue check.
    public bool IsOverdue()
    {
        return ReturnedAt == null && DueAt < DateTime.Now;
    }
}

public sealed class Administrator
{
    public int Id { get; }
    public string Username { get; }
    public string DisplayName { get; }

    public Administrator(int id, string username, string displayName)
    {
        Id = id;
        Username = username;
        DisplayName = displayName;
    }
}
