using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    /// <summary>
    /// Determines how Nullable<> types are translated into TypeScript on the specific class decorated with this attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class NullableAsTypeScriptAttribute : Attribute
    {
        public NullableAsTypeScriptAttribute(TypeScriptNonValue nonValueType)
        {
            NonValueType = nonValueType;
        }

        public TypeScriptNonValue NonValueType { get; }
    }
}
