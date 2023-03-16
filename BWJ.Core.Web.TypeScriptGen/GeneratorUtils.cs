using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal static class GeneratorUtils
    {
        public static string ResolveImportPath(GenerationTarget usingImport, GenerationTarget beingImported)
        {
            var biPaths = GetTargetPaths(beingImported);
            var uiPaths = GetTargetPaths(usingImport).Reverse().ToArray();

            int? backupSteps = null;
            string commonRoot = string.Empty;
            for (var i = 0; i < uiPaths.Length; i++)
            {
                if (biPaths.Contains(uiPaths[i]))
                {
                    backupSteps = i;
                    commonRoot = uiPaths[i];
                    break;
                }
            }
            if (backupSteps is null)
            {
                backupSteps = uiPaths.Length;
            }

            var path = string.Empty;
            if (backupSteps == 0)
            {
                path = "./";
            }
            else
            {
                for (var i = 0; i < backupSteps; i++)
                {
                    path += "../";
                }
            }

            var skipCount = commonRoot != string.Empty ? Array.IndexOf(biPaths, commonRoot) + 1 : 0;
            path += string.Join('/', biPaths.Skip(skipCount));

            path = $"{path}/{beingImported.Type.Name.ToKebabCase()}";
            return path;
        }

        public static string[] GetTargetPaths(GenerationTarget gt)
        {
            var output = new string[gt.SourcePath.Count];
            for (var i = 0; i < gt.SourcePath.Count; i++)
            {
                output[i] = string.Join('/', gt.SourcePath.Take(i + 1));
            }

            return output;
        }

        public static string GetTypeScriptPropertyType(TypeScriptType type)
        {
            string value = string.Empty;
            if (type.IsClass)
            {
                value = $"I{type.TypeName}";

                if (type.GenericArguments?.Any() == true)
                {
                    value = $"{value}<{string.Join(", ", type.GenericArguments.Select(a => GetTypeScriptPropertyType(a)))}>";
                }
            }
            else if (type.IsArray)
            {
                value = type.GenericArguments[0].IsNullable || type.GenericArguments[0].IsUndefinable || type.GenericArguments[0].IsArray ?
                    $"({GetTypeScriptPropertyType(type.GenericArguments[0])})[]" : $"{GetTypeScriptPropertyType(type.GenericArguments[0])}[]";
            }
            else
            {
                value = type.TypeName;
            }

            if (type.IsUndefinable && type.IsOptional == false)
            {
                value += " | undefined";
            }
            if (type.IsNullable)
            {
                value += " | null";
            }

            return value;
        }

        public static string GetPropertyValueInitializationSnippet(TypeScriptType type)
        {
            string value;
            if (type.IsUndefinable || type.IsOptional)
            {
                value = "undefined";
            }
            else if (type.IsNullable)
            {
                value = "null";
            }
            else if (type.IsPropertyTypeAGenericParameter)
            {
                value = $"new type{type.TypeName}()";
            }
            else if (type.IsClass)
            {
                value = type.GenericArguments.Any() ?
                    $"new {type.TypeName}({string.Join(", ", type.GenericArguments.Select(p => p.TypeName))})" :
                    $"new {type.TypeName}()";
            }
            else if (type.IsArray)
            {
                value = "[]";
            }
            else if (false == string.IsNullOrEmpty(type.DefaultEnumValue))
            {
                value = $"{type.TypeName}.{type.DefaultEnumValue}";
            }
            else if (type.TypeName == "string")
            {
                value = "''";
            }
            else if (type.TypeName == "number")
            {
                value = "0";
            }
            else if (type.TypeName == "boolean")
            {
                value = "false";
            }
            else
            {
                value = "UNSUPPORTED_VALUE_UNABLE_TO_INITIALIZE";
            }

            return value;
        }

        public static string GetTypeName(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                var typeName = GetSanitizedTypeName(type);
                typeName = $"{typeName}<{string.Join(", ", type.GetGenericArguments().Select(a => GetTypeName(a)))}>";
                return typeName;
            }

            return type.Name;
        }

        public static string GetSanitizedTypeName(Type type)
            => type.Name.Contains('`') ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name;

        public static Type? GetIEnumerableType(Type type)
        {
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
            .GetGenericArguments()[0];
        }
    }
}
