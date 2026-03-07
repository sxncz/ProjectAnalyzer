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

if (!Directory.Exists(path))
{
    Console.WriteLine("Directory does not exist.");
    return;
}

// Check for .csproj or .sql files before proceeding
if (!Directory.GetFiles(path, "*.csproj").Any())
{
    Console.WriteLine("Warning: No .csproj file found in the provided directory.");
}
else if(!Directory.GetFiles(path, "*.sql").Any())
{
    Console.WriteLine("Warning: No .sql file found in the provided directory.");
}

var scanner = new ProjectScanner();
var result = scanner.Scan(path);

var dbDependencies = DatabaseScanner.BuildDatabaseDependencies(path);
result.DatabaseDependencies = dbDependencies;

var reporter = new ConsoleReporter();
reporter.Print(result);

// Ensure FolderDependencies is not null before passing to GenerateDependencyGraph
CoreMethods.GenerateDependencyGraph(
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

var outputFolder = AnalyzerPaths.GetProjectOutputFolder(result.ProjectName);

// Save XML
var outputXml = Path.Combine(outputFolder, $"{result.ProjectName}_ScanResult.xml");
XMLService.SaveScanResultToXml(resultXml, outputXml);
Console.WriteLine($"Scan result saved to {outputXml}");

CoreMethods.GenerateDatabaseDependencyGraph(result.DatabaseDependencies, result.ProjectName);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();