using System.Diagnostics;

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
            int count = 0;

            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (
                    (trimmed.StartsWith("public ") ||
                     trimmed.StartsWith("private ") ||
                     trimmed.StartsWith("protected ") ||
                     trimmed.StartsWith("internal "))
                    &&
                    trimmed.Contains("(")
                    &&
                    trimmed.Contains(")")
                    &&
                    !trimmed.Contains("class ") &&
                    !trimmed.Contains("interface ") &&
                    !trimmed.Contains("record ")
                   )
                {
                    count++;
                }
            }

            return count;
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
            Process.Start(new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

    }
}
