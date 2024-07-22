using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeScriptOutputAttribute : Attribute
    {
        public TypeScriptOutputAttribute(TypeScriptObjectAsset output = TypeScriptObjectAsset.Interface)
        {
            Output = output;
        }

        public TypeScriptObjectAsset Output { get; }
    }
}
