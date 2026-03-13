namespace ProjectAnalyzer.Graph
{
    public interface IDatabaseGraphGenerator
    {
        void GenerateDatabaseGraph(

            Dictionary<string, HashSet<string>> dbDependencies,
            string projectName,
            string outputFolder);
    }
}
