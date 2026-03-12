using ProjectAnalyzer.Core;
using ProjectAnalyzer.Graph;
using ProjectAnalyzer.LayerViolation;
using ProjectAnalyzer.Reporter;
using ProjectAnalyzer.Services;

namespace ProjectAnalyzer.AnalysisStrategy
{
    public class GranularAnalysis : IAnalysisStrategy
    {
        private readonly IReporter _reporter;
        private readonly IGraphGenerator _graphGenerator;
        private readonly IDatabaseGraphGenerator _dbGraphGenerator;
        public GranularAnalysis(IReporter reporter, IGraphGenerator graphGenerator, IDatabaseGraphGenerator dbGraphGenerator)
        {
            _reporter = reporter;
            _graphGenerator = graphGenerator;
            _dbGraphGenerator = dbGraphGenerator;
        }
        public void Run(string path)
        {
            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

            var dbDependencies = DatabaseScanner.BuildDatabaseDependencies(path);

            foreach (var csproj in csprojFiles)
            {
                var projectFolder = Path.GetDirectoryName(csproj);

                if (projectFolder == null)
                    continue;

                var scanner = new ProjectScanner(new LayerViolationDetector(), new RiskCalculator.RiskCalculator(), new DependencyBuilder.DependencyBuilder());
                var result = scanner.Scan(projectFolder);

                result.DatabaseDependencies = dbDependencies;

                _reporter.Print(result);

                // Ensure FolderDependencies is not null before passing to GenerateDependencyGraph
                var folderDependencies = result.FolderDependencies ?? new Dictionary<string, HashSet<string>>();

                _graphGenerator.GenerateDependencyGraph(
                    outputFolder: AnalyzerPaths.GetProjectOutputFolder(result.ProjectName),
                    result.ProjectName,
                    folderDependencies,
                    result.FileDependencies,
                    result.CircularDependencies,
                    result.RiskScores,
                    10.0
                );

                _dbGraphGenerator.GenerateDatabaseGraph(
                    result.DatabaseDependencies,
                    "Database",
                    AnalyzerPaths.GetProjectOutputFolder("Database")
                );

                var resultXml = XMLService.PrepareScanResultForXml(result);

                var outputFolder = AnalyzerPaths.GetProjectOutputFolder(result.ProjectName);

                var xmlPath = Path.Combine(outputFolder, $"{result.ProjectName}_ScanResult.xml");
                XMLService.SaveScanResultToXml(resultXml, xmlPath);
            }
        }
    }
}
