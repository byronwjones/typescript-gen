using BWJ.Core.Web.TypeScriptGen.Configuration;
using System.Collections.Generic;
using System.IO;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class DirectoryPreparer
    {
        private readonly string _rootPath;
        private readonly HashSet<string> _clearedDirectories = new HashSet<string>();

        public DirectoryPreparer(string rootPath)
        {
            _rootPath = rootPath;
        }

        public void PrepareDirectories(TypeScriptAssemblyConfig[] configs)
        {
            foreach(var config in configs)
            {
                PrepareAssemblyDirectories(config);
            }
        }

        private void PrepareAssemblyDirectories(TypeScriptAssemblyConfig config)
        {
            foreach(var area in config.AreaConfigurations)
            {
                PrepareAreaDirectory(area);
            }
        }

        private void PrepareAreaDirectory(TypeScriptAreaConfig config)
        {
            if(config.ClearOutputDirectoryBeforeRegeneration == false) { return; }

            var dir = Path.Combine(_rootPath, config.OutputDirectoryPath);
            if (Directory.Exists(dir) == false || _clearedDirectories.Contains(dir)) { return; }

            ClearDirectory(dir);
        }

        private void ClearDirectory(string directory)
        {
            var subDirs = Directory.GetDirectories(directory);
            foreach(var dir in subDirs)
            {
                ClearDirectory(dir);
            }

            var files = (new DirectoryInfo(directory)).GetFiles();
            foreach(var file in files)
            {
                file.Delete();
            }
        }
    }
}
