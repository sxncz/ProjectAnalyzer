using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
