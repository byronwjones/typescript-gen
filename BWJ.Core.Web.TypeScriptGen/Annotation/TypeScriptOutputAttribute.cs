using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeScriptOutputAttribute : Attribute
    {
        public TypeScriptOutputAttribute(TypeScriptOutput output = TypeScriptOutput.Interface)
        {
            Output = output;
        }

        public TypeScriptOutput Output { get; }
    }
}
