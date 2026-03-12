using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.DependencyBuilder
{
    public class DependencyBuilder : IDependencyBuilder
    {
        public void PopulateFolderDependencies(
            string currentFolder,
            string content,
            Dictionary<string, int> filesPerFolder,
            Dictionary<string, HashSet<string>> folderDependencies)
        {
            if (!folderDependencies.ContainsKey(currentFolder))
                folderDependencies[currentFolder] = new HashSet<string>();

            foreach (var folder in filesPerFolder.Keys)
            {
                if (folder != currentFolder && content.Contains($".{folder}"))
                {
                    folderDependencies[currentFolder].Add(folder);
                }
            }
        }

        public List<string> ExtractClassNames(string content)
        {
            var result = new List<string>();
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("public class ") ||
                    trimmed.StartsWith("internal class ") ||
                    trimmed.StartsWith("class "))
                {
                    var parts = trimmed.Split(' ');
                    var classIndex = Array.IndexOf(parts, "class");
                    if (classIndex >= 0 && classIndex + 1 < parts.Length)
                        result.Add(parts[classIndex + 1].Trim('{', ' '));
                }
            }

            return result;
        }

        public Dictionary<string, HashSet<string>> BuildFileDependencies(
            string rootPath,
            List<(string Path, long Size)> csFiles,
            Dictionary<string, string> classToFileMap)
        {
            var fileDependencies = new Dictionary<string, HashSet<string>>();

            foreach (var file in csFiles)
            {
                var filePath = file.Path;
                var fullPath = Path.Combine(rootPath, filePath);
                if (!File.Exists(fullPath)) continue;

                if (!fileDependencies.ContainsKey(filePath))
                    fileDependencies[filePath] = new HashSet<string>();

                var content = File.ReadAllText(fullPath);

                foreach (var classEntry in classToFileMap)
                {
                    var className = classEntry.Key;
                    var targetFile = classEntry.Value;

                    if (filePath == targetFile) continue;

                    if (content.Contains(className))
                        fileDependencies[filePath].Add(targetFile);
                }
            }

            return fileDependencies;
        }
    }
}
