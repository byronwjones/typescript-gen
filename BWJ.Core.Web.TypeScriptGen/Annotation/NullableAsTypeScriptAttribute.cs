using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
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
