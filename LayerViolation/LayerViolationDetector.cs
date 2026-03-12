using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.LayerViolation
{
    public class LayerViolationDetector : ILayerViolationDetector
    {
        public List<string> DetectLayerViolations(string currentFolder, string relativePath, string content)
        {
            var violations = new List<string>();
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("using ")) continue;

                if (currentFolder == "Controllers" && trimmed.Contains(".BL"))
                    violations.Add($"{relativePath} references BL layer");

                if (currentFolder == "Service" && trimmed.Contains(".Controllers"))
                    violations.Add($"{relativePath} references Controllers layer");

                if (currentFolder == "Models" && trimmed.Contains(".Service"))
                    violations.Add($"{relativePath} references Service layer");
            }

            return violations;
        }
    }
}
