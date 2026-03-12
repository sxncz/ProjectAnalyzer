using ProjectAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                writer.WriteLine($"\"{fromFolder}\";");

                foreach (var toFolder in kvp.Value)
                {
                    writer.WriteLine($"\"{fromFolder}\" -> \"{toFolder}\";");
                }
            }

            if (fileDependencies != null)
            {
                foreach (var kvp in fileDependencies)
                {
                    foreach (var toFile in kvp.Value)
                    {
                        writer.WriteLine($"\"{kvp.Key}\" -> \"{toFile}\";");
                    }
                }
            }

            writer.WriteLine("}");
            writer.Flush(); // ensure written
            writer.Close(); // force close

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
                writer.Flush(); // ensure written
                writer.Close(); // force close
            }

            HelperMethods.ConvertDotToPng(dotPath, pngPath);
        }
    }
}