using System;

namespace BWJ.Core.Web.TypeScriptGen.Annotation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TypeScriptTypeInherentlyNullableAttribute : Attribute { }
}
