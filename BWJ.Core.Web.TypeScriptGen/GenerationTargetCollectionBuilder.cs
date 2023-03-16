using BWJ.Core.Web.TypeScriptGen.Annotation;
using BWJ.Core.Web.TypeScriptGen.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class GenerationTargetCollectionBuilder
    {
        private readonly List<GenerationTarget> _generationTargets = new List<GenerationTarget>();

        public List<GenerationTarget> GetGenerationTargets(TypeScriptAssemblyConfig[] configs)
        {
            _generationTargets.Clear();
            foreach (var config in configs)
            {
                ConfigureGenerationTargetsFromAssembly(config);
            }

            return _generationTargets;
        }

        private void ConfigureGenerationTargetsFromAssembly(TypeScriptAssemblyConfig config)
        {
            var renderableTypes = config.Assembly.GetExportedTypes()
                .Where(t => IsRenderableType(t) && !TypeRequestedExclusion(t));

            foreach (var area in config.AreaConfigurations)
            {
                area.AssemblyConfig = config;
                ConfigureGenerationTargetsFromAssemblyArea(area, renderableTypes);
            }
        }

        private void ConfigureGenerationTargetsFromAssemblyArea(TypeScriptAreaConfig config, IEnumerable<Type> renderableTypes)
        {
            var applicableTypes = renderableTypes
                .Where(t => TypeIncludable(t, config.InclusionMode)
                            && TypeNamespaceMatchesPattern(t, config.NamespacePattern)
                            && IsNovelType(t));

            var pathParts = config.OutputDirectoryPath.Split('\\', '/');
            var sigIndex = GetSignificantNamespaceIndex(applicableTypes);

            var genTypes = applicableTypes.Select(x => {
                var g = new GenerationTarget(x, config);
                g.SourcePath.AddRange(pathParts);
                g.SourcePath.AddRange(GetSignificantNamespace(x, sigIndex));
                return g;
            });
            _generationTargets.AddRange(genTypes);
        }

        private static IEnumerable<string> GetSignificantNamespace(Type type, int significantNamespaceIndex)
        {
            var namespaceParts = type.Namespace?.Split('.') ?? new string[0];
            if (significantNamespaceIndex >= namespaceParts.Length)
            {
                return new string[0];
            }

            return namespaceParts.Skip(significantNamespaceIndex);
        }
        private static int GetSignificantNamespaceIndex(IEnumerable<Type> types)
        {
            if (types.Count() < 2) { return 0; }
            var sampleNamespace = types.First().Namespace?.Split('.') ?? new string[0];

            var index = 0;
            for (index = 0; index < sampleNamespace.Length; index++)
            {
                var partialNamespace = string.Join('.', sampleNamespace.Take(index + 1));
                if (types.Any(t => (t.Namespace?.StartsWith(partialNamespace) ?? false) == false)) { return index; }
            }

            return index;
        }

        private static bool IsRenderableType(Type type)
            => (type.IsEnum || type.IsClass) && type.IsAbstract == false;
        private static bool TypeRequestedExclusion(Type type)
            => type.GetCustomAttribute<TypeScriptIgnoreAttribute>() is not null;
        private static bool TypeRequestedInclusion(Type type)
            => type.GetCustomAttribute<TypeScriptIncludeAttribute>() is not null;
        private static bool TypeIncludable(Type type, TypeScriptInclusionMode inclusionMode)
            => inclusionMode == TypeScriptInclusionMode.Implicit || TypeRequestedInclusion(type);
        private static bool TypeNamespaceMatchesPattern(Type type, string pattern)
            => !string.IsNullOrWhiteSpace(type.Namespace) && Regex.IsMatch(type.Namespace, pattern);
        private bool IsNovelType(Type type)
            => _generationTargets.Any(t => t.Type.AssemblyQualifiedName == type.AssemblyQualifiedName) == false;
    }
}
