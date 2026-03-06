using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Core
{
    public static class AnalyzerPaths
    {
        public const string OutputFolder = @"C:\ProjectAnalyzer\AnalysisOutput";

        public static string GetProjectOutputFolder(string projectName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                projectName = projectName.Replace(c, '_');

            var path = Path.Combine(OutputFolder, projectName);

            Directory.CreateDirectory(path);

            return path;
        }
    }
}
