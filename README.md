# Classroom Equipment Borrowing and Return Management System

Managing classroom equipment, students, and borrowing records.

## Current features

- Main interface with Dashboard, Borrow Item, Return Item, Equipment, Students, and Borrowing History pages.
- Equipment registration with ID, name, category, and notes.
- Electronic and non-electronic equipment classes with category-specific handling instructions.
- Student registration with student ID, name, course, and section.
- Equipment and student search.
- Item selection and removal in the borrowing form.
- Dashboard statistics and borrowing-history tables populated with example records.

## In development

- Administrator account setup and password verification. The current entry screen opens the application without authentication.
- Equipment and student editing and deletion.
- Saving borrowing transactions, processing returns, and permanent history.
- MySQL integration. Current records are stored only in memory and reset when the application closes.

The dashboard and history include illustrative records, not actual transactions. Disabled controls indicate actions that have not yet been implemented.

## Requirements

- Windows
- .NET 8 SDK

## Build and run

```powershell
dotnet build EquipmentProj.csproj
dotnet run --project EquipmentProj.csproj
dotnet run --project EquipmentProj.csproj -- --smoke-test
```

## Project organization

- `Program.cs` — application entry point and smoke checks
- `LoginForm.cs` — application entry screen
- `MainForm.cs`, `MainForm.Pages.cs`, `MainForm.Borrow.cs` — primary interface
- `Equipment.cs` — equipment types and handling behavior
- `Student.cs` — student details
- `DemoStore.cs` — in-memory lists and example records
- `Ui.cs` — shared Windows Forms controls and helpers

## Team

Renneyx Remedios — integration and administration

Clark Vincent Chicote — equipment management and OOP

Philip Sotto — student management and borrowing interface
