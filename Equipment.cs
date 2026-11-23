namespace EquipmentProgress;

// This class contains the information shared by all equipment.
// Abstract means we create one of its child classes, not Equipment directly.
public abstract class Equipment
{
    public string Name { get; }

    protected Equipment(string name)
    {
        Name = name;
    }

    public abstract string Category { get; }

    public abstract string HandlingInstructions();

    public override string ToString()
    {
        return Name + " (" + Category + ")";
    }
}

public class ElectronicEquipment : Equipment
{
    public ElectronicEquipment(string name) : base(name)
    {
    }

    public override string Category
    {
        get { return "Electronic"; }
    }

    public override string HandlingInstructions()
    {
        return "Switch off the power and check the cables.";
    }
}

public class NonElectronicEquipment : Equipment
{
    public NonElectronicEquipment(string name) : base(name)
    {
    }

    public override string Category
    {
        get { return "Non-Electronic"; }
    }

    public override string HandlingInstructions()
    {
        return "Check for damage and clean before returning.";
    }
}
