namespace ProjectAnalyzer.LayerViolation
{
    public interface ILayerViolationDetector
    {
        List<string> DetectLayerViolations(string currentFolder, string relativePath, string content);
    }
}
