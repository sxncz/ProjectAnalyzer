using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectAnalyzer.DependencyBuilder
{
    public interface IDependencyBuilder
    {
        void PopulateFolderDependencies(
            string currentFolder,
            string content,
            Dictionary<string, int> filesPerFolder,
            Dictionary<string, HashSet<string>> folderDependencies);

        List<string> ExtractClassNames(string content);

        Dictionary<string, HashSet<string>> BuildFileDependencies(
            string rootPath,
            List<(string Path, long Size)> csFiles,
            Dictionary<string, string> classToFileMap);
    }
}
