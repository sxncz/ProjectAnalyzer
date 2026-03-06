using ProjectAnalyzer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Services
{
    public class CoreMethods
    {

        //private const string outputFolder = AnalyzerPaths.GetProjectOutputFolder(projectName);

        public static List<string> DetectLayerViolations(
           string currentFolder,
           string relativePath,
           string content)
        {
            var violations = new List<string>();
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("using "))
                    continue;

                if (currentFolder == "Controllers" && trimmed.Contains(".BL"))
                    violations.Add($"{relativePath} references BL layer");

                if (currentFolder == "Service" && trimmed.Contains(".Controllers"))
                    violations.Add($"{relativePath} references Controllers layer");

                if (currentFolder == "Models" && trimmed.Contains(".Service"))
                    violations.Add($"{relativePath} references Service layer");
            }

            return violations;
        }

        public static void PopulateFolderDependencies(
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

        public static Dictionary<string, double> CalculateRiskScores(
            List<(string Path, long Size)> csFiles,
            Dictionary<string, int> linesPerFile,
            Dictionary<string, int> methodsPerFile,
            Dictionary<string, int> classesPerFile)
        {
            var riskScores = new Dictionary<string, double>();

            foreach (var (filePath, fileSize) in csFiles)
            {
                var sizeKB = fileSize / 1024.0;
                var lines = linesPerFile.ContainsKey(filePath) ? linesPerFile[filePath] : 0;
                var methods = methodsPerFile.ContainsKey(filePath) ? methodsPerFile[filePath] : 0;
                var classes = classesPerFile.ContainsKey(filePath) ? classesPerFile[filePath] : 0;

                var score = (lines / 100.0) + (methods * 1.5) + (classes * 2.0) + (sizeKB / 2.0);
                riskScores[filePath] = Math.Round(score, 2);
            }

            return riskScores;
        }

        public static void GenerateDependencyGraph(
            string projectName,
            Dictionary<string, HashSet<string>> folderDependencies,
            Dictionary<string, HashSet<string>> fileDependencies,
            List<string> circularDependencies,
            Dictionary<string, double> riskScores,
            string outputDotPath,
            double riskThreshold = 10.0)
        {
            var outputFolder = AnalyzerPaths.GetProjectOutputFolder(projectName);

            Directory.CreateDirectory(outputFolder);

            foreach (var c in Path.GetInvalidFileNameChars())
                projectName = projectName.Replace(c, '_');

            var (dotPath, pngPath) = HelperMethods.GetGraphPaths(outputFolder, $"{projectName}_dependencies");

            //--- Folder Level ---
            //Generate DOT file
            using (var writer = new StreamWriter(dotPath))
            {
                writer.WriteLine("digraph ProjectDependencies {");
                writer.WriteLine("    rankdir=LR;");
                writer.WriteLine("    node [shape=box, style=filled, color=lightgray];");

                foreach (var kvp in folderDependencies)
                {
                    var fromFolder = kvp.Key;

                    var fromColor = (riskScores != null &&
                                     riskScores.ContainsKey(fromFolder) &&
                                     riskScores[fromFolder] >= riskThreshold)
                        ? "orange"
                        : "lightgray";

                    writer.WriteLine($"    \"{fromFolder}\" [fillcolor={fromColor}];");

                    foreach (var toFolder in kvp.Value)
                    {
                        var edgeColor = (circularDependencies != null &&
                                         circularDependencies.Any(c =>
                                             c.Contains(fromFolder) && c.Contains(toFolder)))
                            ? "red"
                            : "black";

                        writer.WriteLine($"    \"{fromFolder}\" -> \"{toFolder}\" [color={edgeColor}];");
                    }
                }

                // --- File level ---
                if (fileDependencies != null)
                {
                    foreach (var kvp in fileDependencies)
                    {
                        var fromFile = kvp.Key;
                        writer.WriteLine($"    \"{fromFile}\" [fillcolor=lightblue];");

                        foreach (var toFile in kvp.Value)
                        {
                            var edgeColor = "black";
                            writer.WriteLine($"    \"{fromFile}\" -> \"{toFile}\" [color={edgeColor}];");
                        }
                    }
                }

                writer.WriteLine("}");
            }

            HelperMethods.ConvertDotToPng(dotPath, pngPath);    

            Console.WriteLine($"Dependency diagram saved to: {pngPath}");
            Console.WriteLine();
        }

        public static List<string> ExtractClassNames(string content)
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
                    {
                        result.Add(parts[classIndex + 1].Trim('{', ' '));
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, HashSet<string>> BuildFileDependencies(string rootPath, List<(string Path, long Size)> csFiles, Dictionary<string, string> classToFileMap)
        {
            var fileDependencies = new Dictionary<string, HashSet<string>>();

            foreach (var file in csFiles)
            {
                var filePath = file.Path;
                var fullPath = Path.Combine(rootPath, filePath);

                if (!File.Exists(fullPath))
                    continue; // skip missing files

                if (!fileDependencies.ContainsKey(filePath))
                    fileDependencies[filePath] = new HashSet<string>();

                var content = File.ReadAllText(fullPath);

                foreach (var classEntry in classToFileMap)
                {
                    var className = classEntry.Key;
                    var targetFile = classEntry.Value;

                    if (filePath == targetFile)
                        continue;

                    if (content.Contains(className))
                    {
                        fileDependencies[filePath].Add(targetFile);
                    }
                }
            }

            return fileDependencies;
        }

        public static void GenerateDatabaseDependencyGraph(Dictionary<string, HashSet<string>> dbDependencies,string projectName)
        {
            var outputFolder = AnalyzerPaths.GetProjectOutputFolder(projectName);

            Directory.CreateDirectory(outputFolder);

            var (dotPath, pngPath) = HelperMethods.GetGraphPaths(outputFolder, $"{projectName}_database_dependencies");

            using (var writer = new StreamWriter(dotPath))
            {
                writer.WriteLine("digraph DatabaseDependencies {");

                foreach (var kvp in dbDependencies)
                {
                    foreach (var table in kvp.Value)
                    {
                        writer.WriteLine($"\"{kvp.Key}\" -> \"{table}\";");
                    }
                }

                writer.WriteLine("}");
            }

            HelperMethods.ConvertDotToPng(dotPath, pngPath);
        }
    }
}
