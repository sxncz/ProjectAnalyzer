using ProjectAnalyzer.Services;
using ProjectAnalzer.Core;
using System;

namespace ProjectAnalyzer.Reporting
{
    public class ConsoleReporter
    {
        public void Print(ScanResult result)
        {
            Console.WriteLine();
            Console.WriteLine($"Project: {result.ProjectName}");
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Total C# Files: {result.TotalCsFiles}");
            Console.WriteLine($"Total Folders: {result.TotalFolders}");
            Console.WriteLine($"Deepest Folder Level: {result.DeepestLevel}");
            Console.WriteLine();

            Console.WriteLine("Largest Files:");
            foreach (var file in result.LargestFiles)
            {
                double sizeKb = file.Size / 1024.0;
                Console.WriteLine($"- {file.Path} ({sizeKb:F2} KB)");
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Files Per Folder:");
            foreach (var folder in result.FilesPerFolder
                .OrderByDescending(f => f.Value))
            {
                Console.WriteLine($"- {folder.Key} ({folder.Value} files)");
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Class Distribution:");

            foreach (var entry in result.ClassesPerFile
                .Where(c => c.Value > 1)
                .OrderByDescending(c => c.Value))
            {
                Console.WriteLine($"- {entry.Key} ({entry.Value} types)");
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Method Distribution (Files with > 0 methods):");

            foreach (var entry in result.MethodsPerFile
                .Where(m => m.Value > 0)
                .OrderByDescending(m => m.Value))
            {
                Console.WriteLine($"- {entry.Key} ({entry.Value} methods)");
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Large Files (> 100 lines):");

            var largeFiles = result.LinesPerFile
                .Where(l => l.Value > 100)
                .OrderByDescending(l => l.Value)
                .ToList();

            if (!largeFiles.Any())
            {
                Console.WriteLine("No files exceed 100 lines. ✅");
            }
            else
            {
                foreach (var file in largeFiles)
                {
                    Console.WriteLine($"- {file.Key} ({file.Value} lines)");
                }
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Layer Violations:");

            if (!result.LayerViolations.Any())
            {
                Console.WriteLine("No layer violations detected. ✅");
            }
            else
            {
                foreach (var violation in result.LayerViolations)
                {
                    Console.WriteLine($"- {violation}");
                }
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Circular Dependencies:");

            if (!result.CircularDependencies.Any())
            {
                Console.WriteLine("No circular dependencies detected. ✅");
            }
            else
            {
                foreach (var cycle in result.CircularDependencies)
                {
                    Console.WriteLine($"- {cycle}");
                }
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Top Risky Files:");

            foreach (var entry in result.RiskScores.OrderByDescending(r => r.Value).Take(10))
            {
                Console.WriteLine($"- {entry.Key}: Risk Score = {entry.Value}");
            }

            Console.WriteLine("------------------------------");

            foreach (var file in result.FileDependencies)
            {
                Console.WriteLine($"File: {file.Key}");

                foreach (var dep in file.Value)
                {
                    Console.WriteLine($"   -> {dep}");
                }
            }

            Console.WriteLine("------------------------------");

            Console.WriteLine("Database dependencies found:");

            foreach (var kvp in result.DatabaseDependencies)
            {
                Console.WriteLine($"{kvp.Key} -> {string.Join(", ", kvp.Value)}");
            }

            Console.WriteLine();
        }
    }
}