using ProjectAnalzer.Core;

namespace ProjectAnalyzer.Reporter
{
    public interface IReporter
    {
        void Print(ScanResult result);
    }
}
