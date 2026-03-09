using ProjectAnalyzer.Core;
using ProjectAnalyzer.Reporting;
using ProjectAnalyzer.Services;

string path;

if (args.Length == 0)
{
    Console.WriteLine("Enter project path:");
    path = Console.ReadLine()!;
}
else
{
    path = args[0];
}

while (!Directory.Exists(path))
{
    Console.WriteLine("Directory does not exist. Please provide a correct path or press CTRL + C to exit.");
    Console.WriteLine("Enter project path:");
    path = Console.ReadLine()!;
}

// Check for .csproj or .sql files before proceeding
if (!Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories).Any())
{
    Console.WriteLine("Warning: No .csproj file found in the provided directory.");
}
else if(!Directory.GetFiles(path, "*.sql", SearchOption.AllDirectories).Any())
{
    Console.WriteLine("Warning: No .sql file found in the provided directory.");
}

if (!EnvironmentValidator.IsGraphvizInstalled(out var dotPath))
{
    Console.WriteLine("Graphviz is not installed or 'dot' is not in your PATH.");
    Console.WriteLine("Please install Graphviz from https://graphviz.org/download/ and make sure 'dot.exe' is in your system PATH.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

// Find all projects
var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

if (!csprojFiles.Any())
{
    Console.WriteLine("No .csproj files found in this directory.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}

Console.WriteLine($"Found {csprojFiles.Length} projects.");

var rootProjectName = Path.GetFileName(path);

foreach (var csproj in csprojFiles)
{
    var projectFolder = Path.GetDirectoryName(csproj);
    if (projectFolder == null)
        continue;

    Console.WriteLine($"\nScanning project: {csproj}");


    var scanner = new ProjectScanner();
    var result = scanner.Scan(projectFolder);

    var dbDependencies = DatabaseScanner.BuildDatabaseDependencies(projectFolder);
    result.DatabaseDependencies = dbDependencies;

    var reporter = new ConsoleReporter();
    reporter.Print(result);

    var projectFolderName = new DirectoryInfo(projectFolder).Name;

    var outputFolder = Path.Combine(
        "C:\\ProjectAnalyzer\\AnalysisOutput",
        rootProjectName,
        projectFolderName
    );

    Directory.CreateDirectory(outputFolder);

    var dotPathOutput = Path.Combine(outputFolder, "dependencies.dot");

    // Ensure FolderDependencies is not null before passing to GenerateDependencyGraph  
    CoreMethods.GenerateDependencyGraph(
        outputFolder,
        result.ProjectName,
        result.FolderDependencies ?? new Dictionary<string, HashSet<string>>(),
        result.FileDependencies,
        result.CircularDependencies,
        result.RiskScores,
        "dependencies.dot",
        10.0
    );

    // Convert to XML-friendly structure
    var resultXml = XMLService.PrepareScanResultForXml(result);

    // Save XML
    var outputXml = Path.Combine(outputFolder, $"{result.ProjectName}_ScanResult.xml");
    XMLService.SaveScanResultToXml(resultXml, outputXml);
    Console.WriteLine($"Scan result saved to {outputXml}");

    CoreMethods.GenerateDatabaseDependencyGraph(result.DatabaseDependencies, result.ProjectName, outputFolder);
}

Console.WriteLine("\nAll projects scanned successfully.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();