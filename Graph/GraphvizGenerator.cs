using ProjectAnalyzer.Services;

namespace ProjectAnalyzer.Graph
{
    public class GraphvizGenerator : IGraphGenerator, IDatabaseGraphGenerator
    {
        public void GenerateDependencyGraph(
            string outputFolder,
            string projectName,
            Dictionary<string, HashSet<string>> folderDependencies,
            Dictionary<string, HashSet<string>> fileDependencies,
            List<string> circularDependencies,
            Dictionary<string, double> riskScores,
            double riskThreshold)
        {
            Directory.CreateDirectory(outputFolder);

            foreach (var c in Path.GetInvalidFileNameChars())
                projectName = projectName.Replace(c, '_');

            var (dotPath, pngPath) =
                HelperMethods.GetGraphPaths(outputFolder, $"{projectName}_dependencies");

            using var writer = new StreamWriter(dotPath);

            writer.WriteLine("digraph ProjectDependencies {");
            writer.WriteLine("rankdir=LR;");
            writer.WriteLine("node [shape=box, style=filled, color=lightgray];");            

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
            writer.Flush(); // ensure written
            writer.Close(); // force close

            bool hasContent = (folderDependencies != null && folderDependencies.Count > 0) || (fileDependencies != null && fileDependencies.Count > 0);

            if (!hasContent)
            {
                File.Delete(dotPath);
                Console.WriteLine($"No dependencies found for {projectName}. Graph generation skipped.");
                return;
            }

            HelperMethods.ConvertDotToPng(dotPath, pngPath);

            Console.WriteLine($"Dependency diagram saved to: {pngPath}");
        }

        public void GenerateDatabaseGraph(
            Dictionary<string, HashSet<string>> dbDependencies,
            string projectName,
            string outputFolder
            )
        {
            Directory.CreateDirectory(outputFolder);

            var (dotPath, pngPath) = HelperMethods.GetGraphPaths(outputFolder, $"{projectName}_database_dependencies");

            bool hasContent = false;

            using (var writer = new StreamWriter(dotPath))
            {
                writer.WriteLine("digraph DatabaseDependencies {");

                foreach (var kvp in dbDependencies)
                {
                    foreach (var table in kvp.Value)
                    {
                        writer.WriteLine($"\"{kvp.Key}\" -> \"{table}\";");
                        hasContent = true;
                    }
                }

                writer.WriteLine("}");
                writer.Flush(); // ensure written
                writer.Close(); // force close
            }

            if (!hasContent)
            {
                File.Delete(dotPath);
                Console.WriteLine($"No dependencies found for {projectName}. Graph generation skipped.");
                return;
            }

            HelperMethods.ConvertDotToPng(dotPath, pngPath);
        }
    }
}