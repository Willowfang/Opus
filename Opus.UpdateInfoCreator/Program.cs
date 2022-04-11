// See https://aka.ms/new-console-template for more information

using Opus.UpdateInfoCreator;
using System.Text.Encodings.Web;
using System.Text.Json;

string defaultPath = Path.Combine(AppContext.BaseDirectory, "UpdateDefaults.json");
UpdateInfo? defaults = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(defaultPath));
string savePath;

if (args.Length < 2) return;

if (defaults == null) defaults = new UpdateInfo();

savePath = args[0];

if (savePath == null) return;

string? directoryName = Path.GetDirectoryName(savePath);
if (string.IsNullOrEmpty(directoryName)) return;

defaults.Version = args[1];

if (args.Length > 1)
{
    for (int i = 1; i < args.Length; i++)
    {
        if (args[i] == "-n")
        {
            if (!args[i + 1].StartsWith('-'))
            {
                defaults.Notes = args[i + 1].Split(';');
            }
        }

        if (args[i] == "-f")
        {
            if (!args[i + 1].StartsWith('-'))
            {
                defaults.SetupFileDirectory = args[i + 1];
            }
        }
    }
}

Console.WriteLine("Current values are:");
Console.WriteLine($"\tAssembly version: {defaults.Version}");
Console.WriteLine($"\tNotes: ");
foreach (string note in defaults.Notes)
{
    Console.WriteLine($"\t\t{note}");
}
Console.WriteLine($"\tSetup directory path: {defaults.SetupFileDirectory}");
Console.WriteLine();
Console.WriteLine("Please enter new values (or press enter to accept current values).");

Console.Write("Assembly version: ");
string? versionNew = Console.ReadLine();
if (!string.IsNullOrEmpty(versionNew))
    defaults.Version = versionNew;

Console.WriteLine();
List<string> notes = new List<string>();
Console.WriteLine("Add notes (enter empty to stop):");
while (true)
{
    Console.Write("\tNote: ");
    string? note = Console.ReadLine();
    if (string.IsNullOrEmpty(note))
    {
        Console.WriteLine();
        break;
    }
    else
    {
        notes.Add($"\"{note}\"");
        Console.WriteLine();
    }
}

if (notes.Count > 0)
{
    defaults.Notes = notes.ToArray();
}

Console.Write("Setup directory path: ");
string? setupPathNew = Console.ReadLine();
if (!string.IsNullOrEmpty(setupPathNew))
    defaults.SetupFileDirectory = Path.GetFullPath(setupPathNew);

Console.Write("\n");

Console.WriteLine($"Ready to write update info. File will be written to: {savePath}");
Console.Write("Press any key to continue...");
Console.ReadKey();

Directory.CreateDirectory(directoryName);

JsonSerializerOptions options = new JsonSerializerOptions()
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
string infoJson = JsonSerializer.Serialize(defaults, options);

File.WriteAllText(savePath, infoJson);

Console.Write("\n\n");

Console.WriteLine("Update info saved! Press any key to exit...");
Console.ReadKey();
