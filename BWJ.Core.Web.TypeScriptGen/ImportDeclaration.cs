using System.Collections.Generic;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class ImportDeclaration
    {
        public string Namespace { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public HashSet<string> ImportedTypes { get; } = new HashSet<string>();
    }
}