using ProjectAnalyzer.Services;
using ProjectAnalzer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectAnalyzer.Services
{
    public class ProjectScanner
    {
        private readonly HashSet<string> _ignoredFolders = new()
        {
            "bin",
            "obj",
            ".git"
        };

        private readonly List<(string Path, long Size)> _csFiles = new();

        private Dictionary<string, int> _filesPerFolder = new();

        private Dictionary<string, int> _classesPerFile = new();

        private Dictionary<string, int> _methodsPerFile = new();

        private Dictionary<string, int> _linesPerFile = new();

        private List<string> _layerViolations = new();

        private Dictionary<string, HashSet<string>> _folderDependencies = new();

        private Dictionary<string, HashSet<string>> _fileDependencies = new();

        private Dictionary<string, string> _classToFileMap = new();

        public Dictionary<string, HashSet<string>> _databaseDependencies = new();

        private string _rootPath = string.Empty;

        private int _folderCount = 0;

        private int _deepestLevel = 0;

        public ScanResult Scan(string rootPath)
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

            _rootPath = rootPath;

            var projectName = new DirectoryInfo(rootPath).Name;

            ScanDirectory(rootPath, 0);

            var result = new ScanResult
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
                FileDependencies = CoreMethods.BuildFileDependencies(_rootPath, _csFiles, _classToFileMap), 
                CircularDependencies = HelperMethods.DetectCircularDependencies(_folderDependencies),
                RiskScores = CoreMethods.CalculateRiskScores(_csFiles, _linesPerFile, _methodsPerFile, _classesPerFile),
                DatabaseDependencies = _databaseDependencies,
            };

            return result;
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
                var info = new FileInfo(file);
                var relativePath = Path.GetRelativePath(_rootPath, file);
                var folderName = new DirectoryInfo(path).Name;

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

                var currentFolder = new DirectoryInfo(path).Name;

                _layerViolations.AddRange(CoreMethods.DetectLayerViolations(currentFolder, relativePath, content));

                CoreMethods.PopulateFolderDependencies(currentFolder, content, _filesPerFolder, _folderDependencies);

                var classNames = CoreMethods.ExtractClassNames(content);

                foreach (var className in classNames)
                {
                    if (!_classToFileMap.ContainsKey(className))
                        _classToFileMap[className] = relativePath;
                }
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ScanDirectory(dir, currentLevel + 1);
            }
        }
    }
}