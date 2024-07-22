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
            TypeScriptObjectAsset defaultObjectAssetGeneration = TypeScriptObjectAsset.Inherit,
            TypeScriptNonValue regardNativeNullablesAs = TypeScriptNonValue.Inherit,
            Func<string, string>? namespaceTransformer = null,
            Func<Type, Type>? typeTransformer = null)
        {
            NamespacePattern = namespacePattern;
            OutputDirectoryPath = outputDirectoryPath;
            InclusionMode = inclusionMode;
            ClearOutputDirectoryBeforeRegeneration = clearOutputDirectoryBeforeRegeneration;
            DefaultObjectAssetGeneration = defaultObjectAssetGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
            NamespaceTransformer = namespaceTransformer;
            TypeTransformer = typeTransformer;
        }

        internal string NamespacePattern { get; }
        internal string OutputDirectoryPath { get; }
        internal TypeScriptInclusionMode InclusionMode { get; }
        internal bool ClearOutputDirectoryBeforeRegeneration { get; }
        internal TypeScriptObjectAsset DefaultObjectAssetGeneration { get; }
        internal TypeScriptNonValue RegardNativeNullablesAs { get; }
        internal Func<string, string>? NamespaceTransformer { get; }
        internal Func<Type, Type>? TypeTransformer { get; }
        internal TypeScriptAssemblyConfig? AssemblyConfig { get; set; }
    }
}
