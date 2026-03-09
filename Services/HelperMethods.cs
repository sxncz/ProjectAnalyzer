using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ProjectAnalyzer.Services
{
    public static class HelperMethods
    {
        public static int CountKeyword(string content, string keyword)
        {
            int count = 0;
            int index = 0;

            while ((index = content.IndexOf(keyword, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += keyword.Length;
            }

            return count;
        }

        public static int CountMethods(string content)
        {
            var pattern = @"\b(public|private|protected|internal)\s+(async\s+)?(static\s+)?(virtual\s+|override\s+)?[\w<>\[\]]+\s+\w+\s*\(";

            return Regex.Matches(content, pattern).Count;
        }

        public static List<string> DetectCircularDependencies(Dictionary<string, HashSet<string>> folderDependencies)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();
            var cycles = new List<string>();

            foreach (var folder in folderDependencies.Keys)
            {
                if (HasCycle(folder, folderDependencies, visited, stack))
                {
                    cycles.Add($"Circular dependency detected involving: {folder}");
                }
            }

            return cycles;
        }

        private static bool HasCycle(string folder, Dictionary<string, HashSet<string>> folderDependencies, HashSet<string> visited, HashSet<string> stack)
        {
            if (stack.Contains(folder))
                return true;

            if (visited.Contains(folder))
                return false;

            visited.Add(folder);
            stack.Add(folder);

            if (folderDependencies.ContainsKey(folder))
            {
                foreach (var dependency in folderDependencies[folder])
                {
                    if (HasCycle(dependency, folderDependencies, visited, stack))
                        return true;
                }
            }

            stack.Remove(folder);
            return false;
        }

        public static (string dotPath, string pngPath) GetGraphPaths(string outputFolder, string fileName)
        {
            var dotPath = Path.Combine(outputFolder, $"{fileName}.dot");
            var pngPath = Path.Combine(outputFolder, $"{fileName}.png");

            return (dotPath, pngPath);
        }

        public static void ConvertDotToPng(string dotPath, string pngPath)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit();
            }
            else
            {
                throw new InvalidOperationException("Failed to start 'dot' process.");
            }
        }

    }
}
