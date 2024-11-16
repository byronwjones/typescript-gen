using System;
using System.Collections.Generic;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class TypeScriptType
    {
        public string TypeName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public GenerationTarget? GenerationTarget { get; set; }
        public string AssemblyQualifiedName { get; set; } = string.Empty;
        public string? DefaultEnumValue { get; set; }
        public bool IsArray { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsNullable { get; set; }
        public bool IsUndefinable { get; set; }
        public bool IsOptional { get; set; }
        public bool IsPropertyTypeAGenericParameter { get; set; }
        public bool IsClass { get; set; }
        public List<TypeScriptType> GenericArguments { get; set; } = new List<TypeScriptType>();
        public Type? OriginalType { get; set; }
    }
}
