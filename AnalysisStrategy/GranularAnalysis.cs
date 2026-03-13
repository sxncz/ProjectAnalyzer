using ProjectAnalyzer.AnalyzerPath;
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
        private readonly IAnalyzerPaths _analyzerPaths;
        public GranularAnalysis(IReporter reporter, IGraphGenerator graphGenerator, IDatabaseGraphGenerator dbGraphGenerator, IAnalyzerPaths analyzerPaths)
        {
            _reporter = reporter;
            _graphGenerator = graphGenerator;
            _dbGraphGenerator = dbGraphGenerator;
            _analyzerPaths = analyzerPaths;
        }
        public void Run(string path)
        {
            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);           

            var scanner = new ProjectScanner(new LayerViolationDetector(), new RiskCalculator.RiskCalculator(), new DependencyBuilder.DependencyBuilder());

            var rootProjectName = new DirectoryInfo(path).Name;
            var rootOutputFolder = _analyzerPaths.GetProjectOutputFolder(rootProjectName);

            foreach (var csproj in csprojFiles)
            {
                var projectFolder = Path.GetDirectoryName(csproj);

                if (projectFolder == null)
                    continue;

                var result = scanner.Scan(projectFolder);

                var dbDependencies = DatabaseScanner.BuildDatabaseDependencies(projectFolder);
                result.DatabaseDependencies = dbDependencies;

                _reporter.Print(result);

                // Output folder for this specific csproj
                var projectFolderName = new DirectoryInfo(projectFolder).Name;
                var outputFolder = Path.Combine(rootOutputFolder, projectFolderName);
                Directory.CreateDirectory(outputFolder);

                _graphGenerator.GenerateDependencyGraph(
                    outputFolder,
                    result.ProjectName,
                    result.FolderDependencies ?? new Dictionary<string, HashSet<string>>(), // Ensure FolderDependencies is not null before passing to GenerateDependencyGraph
                    result.FileDependencies,
                    result.CircularDependencies,
                    result.RiskScores,
                    10.0
                );

                _dbGraphGenerator.GenerateDatabaseGraph(
                    result.DatabaseDependencies,
                    result.ProjectName,
                    outputFolder
                );

                var resultXml = XMLService.PrepareScanResultForXml(result);

                var xmlPath = Path.Combine(outputFolder, $"{result.ProjectName}_ScanResult.xml");
                XMLService.SaveScanResultToXml(resultXml, xmlPath);
            }
        }
    }
}
