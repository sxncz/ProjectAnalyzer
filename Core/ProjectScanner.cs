using ProjectAnalyzer.DependencyBuilder;
using ProjectAnalyzer.LayerViolation;
using ProjectAnalyzer.RiskCalculator;
using ProjectAnalzer.Core;

namespace ProjectAnalyzer.Services
{
    public class ProjectScanner
    {
        private readonly HashSet<string> _ignoredFolders = new()
        {
            "bin",
            "obj",
            ".git",
            ".vs"
        };

        private readonly List<(string Path, long Size)> _csFiles = new();

        private readonly Dictionary<string, int> _filesPerFolder = new();

        private readonly Dictionary<string, int> _classesPerFile = new();

        private readonly Dictionary<string, int> _methodsPerFile = new();

        private readonly Dictionary<string, int> _linesPerFile = new();

        private readonly List<string> _layerViolations = new();

        private readonly Dictionary<string, HashSet<string>> _folderDependencies = new();

        private readonly Dictionary<string, HashSet<string>> _fileDependencies = new();

        private readonly Dictionary<string, string> _classToFileMap = new();

        public readonly Dictionary<string, HashSet<string>> _databaseDependencies = new();

        private string _rootPath = string.Empty;

        private int _folderCount = 0;

        private int _deepestLevel = 0;

        private readonly ILayerViolationDetector _layerViolationDetector;
        
        private readonly IRiskCalculator _riskCalculator;

        private readonly IDependencyBuilder _dependencyBuilder;

        public ProjectScanner(
            ILayerViolationDetector layerViolationDetector,
            IRiskCalculator riskCalculator,
            IDependencyBuilder dependencyBuilder)
        {
            _layerViolationDetector = layerViolationDetector;
            _riskCalculator = riskCalculator;
            _dependencyBuilder = dependencyBuilder;
        }

        public ScanResult Scan(string rootPath)
        {
            ResetState();

            _rootPath = rootPath;

            var projectName = new DirectoryInfo(rootPath).Name;

            ScanDirectory(rootPath, 0);

            return BuildResult(projectName);
        }

        private void ResetState()
        {
            _folderCount = 0;
            _deepestLevel = 0;

            _csFiles.Clear();
            _filesPerFolder.Clear();
            _classesPerFile.Clear();
            _methodsPerFile.Clear();
            _linesPerFile.Clear();
            _layerViolations.Clear();
            _folderDependencies.Clear();
            _fileDependencies.Clear();
            _classToFileMap.Clear();
            _databaseDependencies.Clear();
        }

        private ScanResult BuildResult(string projectName)
        {
            return new ScanResult
            {
                ProjectName = projectName,
                TotalCsFiles = _csFiles.Count,
                TotalFolders = _folderCount,
                DeepestLevel = _deepestLevel,
                LargestFiles = _csFiles.OrderByDescending(f => f.Size).Take(5).ToList(),
                FilesPerFolder = _filesPerFolder,
                ClassesPerFile = _classesPerFile,
                MethodsPerFile = _methodsPerFile,
                LinesPerFile = _linesPerFile,
                LayerViolations = _layerViolations,
                FolderDependencies = _folderDependencies,
                FileDependencies = _dependencyBuilder.BuildFileDependencies(_rootPath, _csFiles, _classToFileMap),
                CircularDependencies = HelperMethods.DetectCircularDependencies(_folderDependencies),
                RiskScores = _riskCalculator.CalculateRiskScores(_csFiles, _linesPerFile, _methodsPerFile, _classesPerFile),
                DatabaseDependencies = _databaseDependencies,
            };
        }

        private void ScanDirectory(string path, int currentLevel)
        {
            if (_ignoredFolders.Contains(new DirectoryInfo(path).Name))
                return;

            _folderCount++;

            if (currentLevel > _deepestLevel)
                _deepestLevel = currentLevel;

            foreach (var file in Directory.GetFiles(path, "*.cs"))
            {
                ProcessFile(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ScanDirectory(dir, currentLevel + 1);
            }
        }

        private void ProcessFile(string file)
        {
            var info = new FileInfo(file);
            var relativePath = Path.GetRelativePath(_rootPath, file);
            var folderName = new DirectoryInfo(Path.GetDirectoryName(file)!).Name;

            _csFiles.Add((relativePath, info.Length));

            if (!_filesPerFolder.ContainsKey(folderName))
                _filesPerFolder[folderName] = 0;

            _filesPerFolder[folderName]++;

            var content = File.ReadAllText(file);

            int classCount =
                HelperMethods.CountKeyword(content, "class ") +
                HelperMethods.CountKeyword(content, "interface ") +
                HelperMethods.CountKeyword(content, "record ");

            _classesPerFile[relativePath] = classCount;

            int methodCount = HelperMethods.CountMethods(content);
            _methodsPerFile[relativePath] = methodCount;

            var lines = content.Length;
            _linesPerFile[relativePath] = lines;

            var currentFolder = folderName;

            _layerViolations.AddRange(_layerViolationDetector.DetectLayerViolations(currentFolder, relativePath, content));

            _dependencyBuilder.PopulateFolderDependencies(currentFolder, content,  _filesPerFolder, _folderDependencies);

            var classNames = _dependencyBuilder.ExtractClassNames(content);

            foreach (var className in classNames)
            {
                if (!_classToFileMap.ContainsKey(className))
                    _classToFileMap[className] = relativePath;
            }
        }
    }
}