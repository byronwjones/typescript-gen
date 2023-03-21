using BWJ.Core.Web.TypeScriptGen.Annotation;
using System;

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
            TypeScriptNonValue? regardNativeNullablesAs = null,
            Func<string, string>? namespaceTransformer = null,
            Func<string, Type, string>? typeNameTransformer = null)
        {
            NamespacePattern = namespacePattern;
            OutputDirectoryPath = outputDirectoryPath;
            InclusionMode = inclusionMode;
            ClearOutputDirectoryBeforeRegeneration = clearOutputDirectoryBeforeRegeneration;
            DefaultObjectTypeGeneration = defaultObjectTypeGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
            NamespaceTransformer = namespaceTransformer;
            TypeNameTransformer = typeNameTransformer;
        }

        internal string NamespacePattern { get; }
        internal string OutputDirectoryPath { get; }
        internal TypeScriptInclusionMode InclusionMode { get; }
        internal bool ClearOutputDirectoryBeforeRegeneration { get; }
        internal TypeScriptOutput? DefaultObjectTypeGeneration { get; }
        internal TypeScriptNonValue? RegardNativeNullablesAs { get; }
        internal Func<string, string>? NamespaceTransformer { get; }
        internal Func<string, Type, string>? TypeNameTransformer { get; }
        internal TypeScriptAssemblyConfig? AssemblyConfig { get; set; }
    }
}
