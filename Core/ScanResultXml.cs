using System.Xml.Serialization;

namespace ProjectAnalyzer.Core
{
    [XmlRoot("ScanResult")]
    public class ScanResultXml
    {
        public string ProjectName { get; set; } = string.Empty;
        public int TotalCsFiles { get; set; }
        public int TotalFolders { get; set; }
        public int DeepestLevel { get; set; }

        [XmlArray("LargestFiles")]
        [XmlArrayItem("File")]
        public List<FileInfoXml> LargestFiles { get; set; } = new();

        [XmlArray("FilesPerFolder")]
        [XmlArrayItem("Folder")]
        public List<KeyValueInt> FilesPerFolder { get; set; } = new();

        [XmlArray("ClassesPerFile")]
        [XmlArrayItem("File")]
        public List<KeyValueInt> ClassesPerFile { get; set; } = new();

        [XmlArray("MethodsPerFile")]
        [XmlArrayItem("File")]
        public List<KeyValueInt> MethodsPerFile { get; set; } = new();

        [XmlArray("LinesPerFile")]
        [XmlArrayItem("File")]
        public List<KeyValueInt> LinesPerFile { get; set; } = new();

        [XmlArray("LayerViolations")]
        [XmlArrayItem("Violation")]
        public List<string> LayerViolations { get; set; } = new();

        [XmlArray("FolderDependencies")]
        [XmlArrayItem("Folder")]
        public List<KeyValueStringList> FolderDependencies { get; set; } = new();

        [XmlArray("FileDependencies")]
        [XmlArrayItem("File")]
        public List<KeyValueStringList> FileDependencies { get; set; } = new();

        [XmlArray("CircularDependencies")]
        [XmlArrayItem("Dependency")]
        public List<string> CircularDependencies { get; set; } = new();

        [XmlArray("RiskScores")]
        [XmlArrayItem("Score")]
        public List<KeyValueDouble> RiskScores { get; set; } = new();
    }

    public class FileInfoXml
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
    }

    public class KeyValueInt
    {
        public string Key { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class KeyValueDouble
    {
        public string Key { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class KeyValueStringList
    {
        public string Key { get; set; } = string.Empty;
        [XmlArrayItem("Value")]
        public List<string> Value { get; set; } = new();
    }
}
