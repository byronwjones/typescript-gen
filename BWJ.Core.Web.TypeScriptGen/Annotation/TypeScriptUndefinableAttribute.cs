using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.GenericParameter, AllowMultiple = false)]
    public class TypeScriptUndefinableAttribute : Attribute { }
}
