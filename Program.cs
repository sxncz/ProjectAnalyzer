using ProjectAnalyzer.AnalysisStrategy;
using ProjectAnalyzer.AnalyzerPath;
using ProjectAnalyzer.Graph;
using ProjectAnalyzer.Reporter;
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
else if (!Directory.GetFiles(path, "*.sql", SearchOption.AllDirectories).Any())
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

string choice;

while (true)
{
    Console.WriteLine("Choose analysis type:");
    Console.WriteLine("1 - High Level View");
    Console.WriteLine("2 - Granular View");
    Console.Write("Enter choice: ");

    choice = Console.ReadLine() ?? string.Empty;

    if (choice == "1" || choice == "2")
        break;

    Console.WriteLine("Invalid choice. Please enter 1 or 2 or press CTRL + C to exit.\n");
}

IAnalyzerPaths analyzerPaths = new AnalyzerPaths();

IReporter reporter = new ConsoleReporter();
IGraphGenerator graphGenerator = new GraphvizGenerator();
IDatabaseGraphGenerator dbGraphGenerator = (IDatabaseGraphGenerator)graphGenerator;

IAnalysisStrategy strategy = choice switch
{
    "1" => new HighLevelAnalysis(reporter, graphGenerator, dbGraphGenerator, analyzerPaths),
    "2" => new GranularAnalysis(reporter, graphGenerator, dbGraphGenerator, analyzerPaths),
    _ => throw new Exception()
};

var runner = new AnalysisRunner(strategy);

runner.Execute(path);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();