using ProjectAnalyzer.AnalyzerPath;
using ProjectAnalyzer.Core;
using ProjectAnalyzer.Graph;
using ProjectAnalyzer.LayerViolation;
using ProjectAnalyzer.Reporter;
using ProjectAnalyzer.Services;

namespace ProjectAnalyzer.AnalysisStrategy
{
    public class HighLevelAnalysis : IAnalysisStrategy
    {
        private readonly IReporter _reporter;
        private readonly IGraphGenerator _graphGenerator;
        private readonly IDatabaseGraphGenerator _dbGraphGenerator;
        private readonly IAnalyzerPaths _analyzerPaths;

        public HighLevelAnalysis(IReporter reporter, IGraphGenerator graphGenerator, IDatabaseGraphGenerator dbGraphGenerator, IAnalyzerPaths analyzerPaths)
        {
            _reporter = reporter;
            _graphGenerator = graphGenerator;
            _dbGraphGenerator = dbGraphGenerator;
            _analyzerPaths = analyzerPaths;
        }

        public void Run(string path)
        {
            var scanner = new ProjectScanner(new LayerViolationDetector(), new RiskCalculator.RiskCalculator(), new DependencyBuilder.DependencyBuilder());

            var result = scanner.Scan(path);

            var dbDependencies =
                DatabaseScanner.BuildDatabaseDependencies(path);

            result.DatabaseDependencies = dbDependencies;

            _reporter.Print(result);

            // Ensure FolderDependencies is not null before passing to GenerateDependencyGraph
            var folderDependencies = result.FolderDependencies ?? new Dictionary<string, HashSet<string>>();

            var outputFolder = _analyzerPaths.GetProjectOutputFolder(result.ProjectName);

            _graphGenerator.GenerateDependencyGraph(
                outputFolder,
                result.ProjectName,
                folderDependencies,
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
