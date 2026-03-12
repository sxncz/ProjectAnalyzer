using ProjectAnalzer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Reporter
{
    public interface IReporter
    {
        void Print(ScanResult result);
    }
}
