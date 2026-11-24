namespace EquipmentProgress;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--smoke-test"))
        {
            SmokeTest.Run();
            return;
        }

        // Set up Windows Forms before opening the application window.
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm(new DemoStore()));
    }
}

// Verify core data operations without launching the Windows Forms interface.
internal static class SmokeTest
{
    public static void Run()
    {
        // Check the core data operations without interacting with the forms.
        var store = new DemoStore();
        if (store.Equipment().Rows.Count < 3 || store.Students().Rows.Count < 2)
            throw new Exception("Initial records are missing.");

        var electronic = Equipment.Create("EQ-TEST", "Test projector", "Electronic", "");
        var furniture = Equipment.Create("EQ-TEST2", "Test chair", "Non-Electronic", "");
        if (electronic.HandlingInstructions() == furniture.HandlingInstructions())
            throw new Exception("Equipment category handling should differ.");

        store.SaveEquipment(electronic, editing: false);
        if (store.Equipment().Rows.Count < 4)
            throw new Exception("In-memory equipment addition failed.");

        store.SaveStudent(new Student("ST-TEST", "Test Student", "BSCS", "A"), editing: false);
        if (!store.Students().Rows.Cast<System.Data.DataRow>().Any(r => Convert.ToString(r["ID"]) == "ST-TEST"))
            throw new Exception("In-memory student registration failed.");

        if (store.Loans().Rows.Count == 0 || store.Loans(activeOnly: true).Rows.Count == 0)
            throw new Exception("Initial history records are missing.");

        Console.WriteLine("PASS: initial data, equipment types, registration, borrowing history.");
    }
}
