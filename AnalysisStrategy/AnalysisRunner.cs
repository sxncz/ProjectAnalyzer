using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.AnalysisStrategy
{
    public class AnalysisRunner
    {
        private readonly IAnalysisStrategy _strategy;
        public AnalysisRunner(IAnalysisStrategy strategy)
        {
            _strategy = strategy;
        }
        public void Execute(string path)
        {
            _strategy.Run(path);
        }
    }
}