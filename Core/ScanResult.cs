using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProjectAnalzer.Core
{
    public class ScanResult
    {
        public string ProjectName { get; set; } = string.Empty;
        public int TotalCsFiles { get; set; }
        public int TotalFolders { get; set; }
        public int DeepestLevel { get; set; }
        public List<(string Path, long Size)> LargestFiles { get; set; } = new List<(string Path, long Size)>();
        public Dictionary<string, int> FilesPerFolder { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ClassesPerFile { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MethodsPerFile { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> LinesPerFile { get; set; } = new();
        public List<string> LayerViolations { get; set; } = new();
        public Dictionary<string, HashSet<string>>? FolderDependencies { get; internal set; }
        public Dictionary<string, HashSet<string>> FileDependencies { get; set; } = new Dictionary<string, HashSet<string>>();
        public List<string> CircularDependencies { get; set; } = new();
        public Dictionary<string, double> RiskScores { get; set; } = new();
        public Dictionary<string, HashSet<string>> DatabaseDependencies { get; set; } = new();
    }
}

