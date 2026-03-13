using ProjectAnalyzer.Core;
using ProjectAnalzer.Core;
using System.Xml.Serialization;

namespace ProjectAnalyzer.Services
{
    public static class XMLService
    {
        public static ScanResultXml PrepareScanResultForXml(ScanResult result)
        {
            return new ScanResultXml
            {
                ProjectName = result.ProjectName,
                TotalCsFiles = result.TotalCsFiles,
                TotalFolders = result.TotalFolders,
                DeepestLevel = result.DeepestLevel,
                LargestFiles = result.LargestFiles
                    .Select(f => new FileInfoXml { Path = f.Path, Size = f.Size })
                    .ToList(),
                FilesPerFolder = result.FilesPerFolder
                    .Select(kvp => new KeyValueInt { Key = kvp.Key, Value = kvp.Value })
                    .ToList(),
                ClassesPerFile = result.ClassesPerFile
                    .Select(kvp => new KeyValueInt { Key = kvp.Key, Value = kvp.Value })
                    .ToList(),
                MethodsPerFile = result.MethodsPerFile
                    .Select(kvp => new KeyValueInt { Key = kvp.Key, Value = kvp.Value })
                    .ToList(),
                LinesPerFile = result.LinesPerFile
                    .Select(kvp => new KeyValueInt { Key = kvp.Key, Value = kvp.Value })
                    .ToList(),
                LayerViolations = result.LayerViolations,
                FolderDependencies = (result.FolderDependencies ?? new Dictionary<string, HashSet<string>>())
                    .Select(kvp => new KeyValueStringList { Key = kvp.Key, Value = kvp.Value.ToList() })
                    .ToList(),
                FileDependencies = result.FileDependencies
                    .Select(kvp => new KeyValueStringList { Key = kvp.Key, Value = kvp.Value.ToList() })
                    .ToList(),
                CircularDependencies = result.CircularDependencies,
                RiskScores = result.RiskScores
                    .Select(kvp => new KeyValueDouble { Key = kvp.Key, Value = kvp.Value })
                    .ToList()
            };
        }

        public static void SaveScanResultToXml(ScanResultXml resultXml, string outputPath)
        {
            var serializer = new XmlSerializer(typeof(ScanResultXml));
            using var writer = new StreamWriter(outputPath);
            serializer.Serialize(writer, resultXml);
        }
    }
}
