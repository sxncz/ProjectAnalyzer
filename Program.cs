using ProjectAnalyzer.Core;
using ProjectAnalyzer.Reporting;
using ProjectAnalyzer.Services;

if (args.Length == 0)
{
    Console.WriteLine("Please provide a project path.");
    return;
}

var path = args[0];

if (!Directory.Exists(path))
{
    Console.WriteLine("Directory does not exist.");
    return;
}

// Check for .csproj
if (!Directory.GetFiles(path, "*.csproj").Any())
{
    Console.WriteLine("No .csproj file found in the provided directory.");
    return;
}

var scanner = new ProjectScanner();
var result = scanner.Scan(path);

var dbDependencies = DatabaseScanner.BuildDatabaseDependencies(path);
result.DatabaseDependencies = dbDependencies;

var reporter = new ConsoleReporter();
reporter.Print(result);

#pragma warning disable CS8604 // Possible null reference argument.
CoreMethods.GenerateDependencyGraph(result.ProjectName, result.FolderDependencies, result.FileDependencies, result.CircularDependencies, result.RiskScores, "dependencies.dot", 10.0);
#pragma warning restore CS8604 // Possible null reference argument.

//newly added
// Convert to XML-friendly structure
var resultXml = XMLService.PrepareScanResultForXml(result);

var outputFolder = AnalyzerPaths.GetProjectOutputFolder(result.ProjectName);

// Save XML
var outputXml = Path.Combine(outputFolder, $"{result.ProjectName}_ScanResult.xml");
XMLService.SaveScanResultToXml(resultXml, outputXml);
Console.WriteLine($"Scan result saved to {outputXml}");

CoreMethods.GenerateDatabaseDependencyGraph(result.DatabaseDependencies, result.ProjectName);