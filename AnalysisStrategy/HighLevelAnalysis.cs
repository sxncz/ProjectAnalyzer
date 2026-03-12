using ProjectAnalyzer.Core;
using ProjectAnalyzer.Graph;
using ProjectAnalyzer.LayerViolation;
using ProjectAnalyzer.Reporter;
using ProjectAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.AnalysisStrategy
{
    public class HighLevelAnalysis : IAnalysisStrategy
    {
        private readonly IReporter _reporter;
        private readonly IGraphGenerator _graphGenerator;
        private readonly IDatabaseGraphGenerator _dbGraphGenerator;

        public HighLevelAnalysis(IReporter reporter, IGraphGenerator graphGenerator, IDatabaseGraphGenerator dbGraphGenerator)
        {
            _reporter = reporter;
            _graphGenerator = graphGenerator;
            _dbGraphGenerator = dbGraphGenerator;
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

            _graphGenerator.GenerateDependencyGraph(
                outputFolder: AnalyzerPaths.GetProjectOutputFolder(result.ProjectName),
                result.ProjectName,
                folderDependencies,
                result.FileDependencies,
                result.CircularDependencies,
                result.RiskScores,
                10.0
            );

            var outputFolder =
                AnalyzerPaths.GetProjectOutputFolder(result.ProjectName);

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
