using BWJ.Core.Web.TypeScriptGen.Annotation;
using System;
using System.Reflection;

namespace BWJ.Core.Web.TypeScriptGen.Configuration
{
    public sealed class TypeScriptAssemblyConfig
    {
        public TypeScriptAssemblyConfig (
            Assembly assembly,
            string outputDirectoryPath,
            TypeScriptInclusionMode inclusionMode,
            bool clearOutputDirectoryBeforeRegeneration,
            string namespacePattern = ".+",
            TypeScriptOutput? defaultObjectTypeGeneration = null,
            TypeScriptNonValue? regardNativeNullablesAs = null,
            Func<string, string>? namespaceTransformer = null,
            Func<string, Type, string>? typeNameTransformer = null)
        {
            AreaConfigurations = new TypeScriptAreaConfig[] {
                new TypeScriptAreaConfig (
                    namespacePattern,
                    outputDirectoryPath,
                    inclusionMode,
                    clearOutputDirectoryBeforeRegeneration,
                    defaultObjectTypeGeneration,
                    regardNativeNullablesAs,
                    namespaceTransformer,
                    typeNameTransformer
                ),
            };
            Assembly = assembly;
            DefaultObjectTypeGeneration = defaultObjectTypeGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
        }

        public TypeScriptAssemblyConfig(Assembly assembly,
            TypeScriptNonValue? regardNativeNullablesAs = null,
            TypeScriptOutput? defaultObjectTypeGeneration = null,
            params TypeScriptAreaConfig[] areaConfigs)
        {
            Assembly = assembly;
            AreaConfigurations = areaConfigs;
            DefaultObjectTypeGeneration = defaultObjectTypeGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
        }

        internal Assembly Assembly { get; }
        internal TypeScriptOutput? DefaultObjectTypeGeneration { get; }
        internal TypeScriptNonValue? RegardNativeNullablesAs { get; }
        internal TypeScriptAreaConfig[] AreaConfigurations { get; }
    }
}
