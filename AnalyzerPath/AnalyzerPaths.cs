namespace ProjectAnalyzer.AnalyzerPath
{
    public class AnalyzerPaths : IAnalyzerPaths
    {
        public const string OutputFolder = @"C:\ProjectAnalyzer\AnalysisOutput";

        public string GetProjectOutputFolder(string projectName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                projectName = projectName.Replace(c, '_');

            var path = Path.Combine(OutputFolder, projectName);

            Directory.CreateDirectory(path);

            return path;
        }
    }
}
