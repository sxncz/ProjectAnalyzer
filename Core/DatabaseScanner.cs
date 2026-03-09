using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectAnalyzer.Core
{
    public static class DatabaseScanner
    {
        public static Dictionary<string, HashSet<string>> BuildDatabaseDependencies(string rootPath)
        {
            var dbDependencies = new Dictionary<string, HashSet<string>>();

            var csFiles = Directory.GetFiles(rootPath, "*.sql", SearchOption.AllDirectories).Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\")).ToList();

            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileName(file);

                if (!dbDependencies.ContainsKey(fileName))
                    dbDependencies[fileName] = new HashSet<string>();

                var tableRegex = new Regex(@"(FROM|JOIN|INTO|UPDATE)\s+([A-Za-z0-9_]+)", RegexOptions.IgnoreCase);

                foreach (Match match in tableRegex.Matches(content))
                {
                    var table = match.Groups[2].Value;
                    dbDependencies[fileName].Add(table);
                }

                var execRegex = new Regex(@"EXEC\s+([A-Za-z0-9_]+)", RegexOptions.IgnoreCase);

                foreach (Match match in execRegex.Matches(content))
                {
                    var procedure = match.Groups[1].Value;
                    dbDependencies[fileName].Add($"SP:{procedure}");
                }

                var efRegex = new Regex(@"_context\.([A-Za-z0-9_]+)", RegexOptions.IgnoreCase);

                foreach (Match match in efRegex.Matches(content))
                {
                    var table = match.Groups[1].Value;
                    dbDependencies[fileName].Add(table);
                }
            }

            return dbDependencies;
        }
    }
}
