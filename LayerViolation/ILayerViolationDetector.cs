using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.LayerViolation
{
    public interface ILayerViolationDetector
    {
        List<string> DetectLayerViolations(string currentFolder, string relativePath, string content);
    }
}
