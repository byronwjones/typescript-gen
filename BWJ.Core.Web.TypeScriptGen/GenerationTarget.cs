﻿using BWJ.Core.Web.TypeScriptGen.Annotation;
using BWJ.Core.Web.TypeScriptGen.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class GenerationTarget
    {
        public GenerationTarget(Type type, TypeScriptAreaConfig areaConfig)
        {
            Type = type;
            AreaConfig = areaConfig;
        }

        public Type Type { get; }
        public bool GenerateClass { get; set; }
        public bool GenerateInterface { get; set; }
        public TypeScriptAreaConfig AreaConfig { get; }
        public List<string> SourcePath { get; } = new List<string>();
        public string DefaultEnumName
        {
            get => Type.IsEnum ? Type.GetEnumNames()[0] : string.Empty;
        }
        public IEnumerable<string> GenericArguments
        {
            get => Type.IsGenericTypeDefinition ?
                Type.GetGenericArguments().Select(a => a.Name) : new List<string>();
        }
        public IEnumerable<PropertyInfo> MemberProperties
        {
            get => Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<TypeScriptIgnoreAttribute>() is null);
        }

        public IEnumerable<TypeScriptType> TypeScriptProperties { get; set; } = new List<TypeScriptType>();

        public List<string> TypeScriptImports { get; set; } = new List<string>();

        public bool InterfaceGenerationRequested(TypeScriptObjectAsset rootOutputDemand)
            => GenerateInterface || IsOutputGenerationRequested(rootOutputDemand, TypeScriptObjectAsset.Interface);

        public bool ClassGenerationRequested(TypeScriptObjectAsset rootOutputDemand)
            => GenerateClass || IsOutputGenerationRequested(rootOutputDemand, TypeScriptObjectAsset.Class);

        private bool IsOutputGenerationRequested(TypeScriptObjectAsset rootOutputDemand, TypeScriptObjectAsset outputType)
        {
            if (Type.IsEnum) { return false; }

            var outputAttr = Type.GetCustomAttribute<TypeScriptOutputAttribute>();
            if (outputAttr is not null && (outputAttr.Output & outputType) == outputType) { return true; }

            var areaGenSpec = AreaConfig.DefaultObjectAssetGeneration;
            if ((areaGenSpec & outputType) == outputType) { return true; }

            var assmGenSpec = AreaConfig.AssemblyConfig?.DefaultObjectAssetGeneration;
            if (assmGenSpec is not null && (assmGenSpec & outputType) == outputType) { return true; }

            return (rootOutputDemand & outputType) == outputType;
        }
    }
}
