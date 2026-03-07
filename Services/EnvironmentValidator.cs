using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Services
{
    public static class EnvironmentValidator
    {
        public static bool IsGraphvizInstalled(out string dotPath)
        {
            dotPath = string.Empty;

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dot",
                        Arguments = "-V",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                // If process runs successfully, Graphviz is installed
                dotPath = "dot";
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
