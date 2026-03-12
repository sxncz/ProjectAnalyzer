using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Graph
{
    public interface IGraphGenerator
    {
        void GenerateDependencyGraph(
            string outputFolder,
            string projectName,
            Dictionary<string, HashSet<string>> folderDependencies,
            Dictionary<string, HashSet<string>> fildeDependencies,
            List<string> circularDependencies,
            Dictionary<string, double> riskScores,
            double riskThreshold);
    }
}
