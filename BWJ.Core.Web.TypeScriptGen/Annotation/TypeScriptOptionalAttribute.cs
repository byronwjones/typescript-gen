using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TypeScriptOptionalAttribute : Attribute { }
}
