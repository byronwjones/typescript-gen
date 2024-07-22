using BWJ.Core.Web.TypeScriptGen.Annotation;
using System;
using System.Linq;
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
            TypeScriptObjectAsset defaultObjectAssetGeneration = TypeScriptObjectAsset.Inherit,
            TypeScriptNonValue regardNativeNullablesAs = TypeScriptNonValue.Inherit,
            Func<string, string>? namespaceTransformer = null,
            Func<Type, Type>? typeTransformer = null)
        {
            AreaConfigurations = new TypeScriptAreaConfig[] {
                new TypeScriptAreaConfig (
                    namespacePattern,
                    outputDirectoryPath,
                    inclusionMode,
                    clearOutputDirectoryBeforeRegeneration,
                    defaultObjectAssetGeneration,
                    regardNativeNullablesAs,
                    namespaceTransformer,
                    typeTransformer
                ),
            };
            Assembly = assembly;
            DefaultObjectAssetGeneration = defaultObjectAssetGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
        }

        public TypeScriptAssemblyConfig(Assembly assembly,
            TypeScriptNonValue regardNativeNullablesAs,
            TypeScriptObjectAsset defaultObjectAssetGeneration,
            TypeScriptAreaConfig areaConfig,
            params TypeScriptAreaConfig[] additionalAreaConfigs)
        {
            Assembly = assembly;
            AreaConfigurations = (new TypeScriptAreaConfig[] { areaConfig }).Concat(additionalAreaConfigs).ToArray();
            DefaultObjectAssetGeneration = defaultObjectAssetGeneration;
            RegardNativeNullablesAs = regardNativeNullablesAs;
        }

        internal Assembly Assembly { get; }
        internal TypeScriptObjectAsset DefaultObjectAssetGeneration { get; }
        internal TypeScriptNonValue RegardNativeNullablesAs { get; }
        internal TypeScriptAreaConfig[] AreaConfigurations { get; }
    }
}
