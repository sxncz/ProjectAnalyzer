namespace ProjectAnalyzer.RiskCalculator
{
    public class RiskCalculator : IRiskCalculator
    {
        public Dictionary<string, double> CalculateRiskScores(
            List<(string Path, long Size)> csFiles,
            Dictionary<string, int> linesPerFile,
            Dictionary<string, int> methodsPerFile,
            Dictionary<string, int> classesPerFile)
        {
            var riskScores = new Dictionary<string, double>();

            foreach (var (filePath, fileSize) in csFiles)
            {
                var sizeKB = fileSize / 1024.0;
                var lines = linesPerFile.GetValueOrDefault(filePath, 0);
                var methods = methodsPerFile.GetValueOrDefault(filePath, 0);
                var classes = classesPerFile.GetValueOrDefault(filePath, 0);

                var score = (lines / 100.0) + (methods * 1.5) + (classes * 2.0) + (sizeKB / 2.0);
                riskScores[filePath] = Math.Round(score, 2);
            }

            return riskScores;
        }
    }
}
