using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.RiskCalculator
{
    public interface IRiskCalculator
    {
        Dictionary<string, double> CalculateRiskScores(
            List<(string Path, long Size)> csFiles,
            Dictionary<string, int> linesPerFile,
            Dictionary<string, int> methodsPerFile,
            Dictionary<string, int> classesPerFile);
    }
}
