using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class TypeScriptIncludeAttribute : Attribute { }
}
