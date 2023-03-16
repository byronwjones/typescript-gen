using BWJ.Core.Web.TypeScriptGen.Annotation;

namespace BWJ.Core.Web.TypeScriptGen.Configuration
{
    public sealed class TypeScriptAreaConfig
    {
        public TypeScriptAreaConfig(
            string namespacePattern,
            string outputDirectoryPath,
            TypeScriptInclusionMode inclusionMode,
            bool clearOutputDirectoryBeforeRegeneration,
            TypeScriptOutput? defaultObjectTypeGeneration = null,
            TypeScriptNonValue? regardNativeNullablesAs = null)
        {
            NamespacePattern = namespacePattern;
            OutputDirectoryPath = outputDirectoryPath;
            InclusionMode = inclusionMode;
            ClearOutputDirectoryBeforeRegeneration = clearOutputDirectoryBeforeRegeneration;
            DefaultObjectTypeGeneration = defaultObjectTypeGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
        }

        internal string NamespacePattern { get; }
        internal string OutputDirectoryPath { get; }
        internal TypeScriptInclusionMode InclusionMode { get; }
        internal bool ClearOutputDirectoryBeforeRegeneration { get; }
        internal TypeScriptOutput? DefaultObjectTypeGeneration { get; }
        internal TypeScriptNonValue? RegardNativeNullablesAs { get; }
        internal TypeScriptAssemblyConfig? AssemblyConfig { get; set; }
    }
}
