using BWJ.Core.Web.TypeScriptGen.Annotation;
using BWJ.Core.Web.TypeScriptGen.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BWJ.Core.Web.TypeScriptGen
{
    public class TypeScriptGenerator
    {
        private readonly string _rootPath;
        private readonly TypeScriptNonValue _regardNativeNullablesAs;
        private readonly TypeScriptOutput _defaultObjectTypeGeneration;
        private readonly Dictionary<Type, Type> _propertyTypeMappings;
        private readonly TypeScriptAssemblyConfig[] _configs;
        private List<GenerationTarget> _typesToGenerate = new List<GenerationTarget>();

        public TypeScriptGenerator(
            string rootPath,
            Dictionary<Type, Type> propertyTypeMappings,
            TypeScriptNonValue regardNativeNullablesAs,
            TypeScriptOutput defaultObjectTypeGeneration,
            params TypeScriptAssemblyConfig[] configs)
        {
            _rootPath = rootPath;
            _regardNativeNullablesAs = regardNativeNullablesAs;
            _defaultObjectTypeGeneration = defaultObjectTypeGeneration;
            _propertyTypeMappings = propertyTypeMappings;
            _configs = configs;
        }

        public TypeScriptGenerator(
            string rootPath,
            TypeScriptNonValue regardNativeNullablesAs,
            TypeScriptOutput defaultObjectTypeGeneration,
            params TypeScriptAssemblyConfig[] configs)
        {
            _rootPath = rootPath;
            _regardNativeNullablesAs = regardNativeNullablesAs;
            _defaultObjectTypeGeneration = defaultObjectTypeGeneration;
            _propertyTypeMappings = new Dictionary<Type, Type>();
            _configs = configs;
        }

        public async Task GenerateTypeScript()
        {
            (new DirectoryPreparer(_rootPath)).PrepareDirectories(_configs);
            _typesToGenerate = (new GenerationTargetCollectionBuilder()).GetGenerationTargets(_configs);
            DetermineInterfaceRendering();
            DetermineClassRendering();

            foreach (var type in _typesToGenerate)
            {
                if (type.Type.IsEnum)
                {
                    await GenerateEnum(type);
                }
                else
                {
                    await GenerateObjectTypes(type);
                }
            }
        }

        private void DetermineClassRendering()
        {
            var needsClassRender = _typesToGenerate
                .Where(t => t.ClassGenerationRequested(_defaultObjectTypeGeneration))
                .ToList();

            needsClassRender.ForEach(t => t.GenerateClass = true);
            DetermineClassRenderRecursive(needsClassRender);
        }

        private void DetermineInterfaceRendering()
        {
            var needsIfaceRender = _typesToGenerate
                .Where(t => t.InterfaceGenerationRequested(_defaultObjectTypeGeneration))
                .ToList();

            needsIfaceRender.ForEach(t => t.GenerateInterface = true);
            DetermineClassRenderRecursive(needsIfaceRender);
        }

        private void DetermineClassRenderRecursive(List<GenerationTarget> needsClass)
        {
            if (needsClass.Any() == false) { return; }

            var usedTypes = new HashSet<string>();
            foreach (var target in needsClass)
            {
                var classType = target.Type;
                var classPropertyTypes = classType.GetProperties()
                    .Select(p => p.PropertyType);
                foreach (var cpt in classPropertyTypes)
                {
                    usedTypes.Add(cpt.AssemblyQualifiedName!);
                }
            }

            var discoveredNeedsClass = _typesToGenerate
                .Where(t => usedTypes.Contains(t.Type.AssemblyQualifiedName!) && t.GenerateClass == false)
                .ToList();
            discoveredNeedsClass.ForEach(t => t.GenerateClass = true);
            DetermineClassRenderRecursive(discoveredNeedsClass);
        }

        private void ConfigureImportStatements(GenerationTarget generationTarget)
        {
            var generatedPropertyTypes = generationTarget.MemberProperties
                .Where(prop => _typesToGenerate.Any(t => t.Type.AssemblyQualifiedName == prop.PropertyType.AssemblyQualifiedName))
                .Select(prop => _typesToGenerate.First(t => t.Type.AssemblyQualifiedName == prop.PropertyType.AssemblyQualifiedName));

            foreach (var genProp in generatedPropertyTypes)
            {
                var importTypes = new List<string>();
                if (genProp.GenerateInterface)
                {
                    importTypes.Add($"I{genProp.Type.Name}");
                }
                else if (genProp.Type.IsEnum || genProp.GenerateClass)
                {
                    importTypes.Add(genProp.Type.Name);
                }

                generationTarget.TypeScriptImports
                    .Add($"import {{ {string.Join(", ", importTypes)} }} from '{GeneratorUtils.ResolveImportPath(generationTarget, genProp)}';");
            }
        }

        private async Task GenerateObjectTypes(GenerationTarget generationTarget)
        {
            generationTarget.TypeScriptProperties = generationTarget.MemberProperties
                .Select(prop => ToTypeScriptType(generationTarget, prop));

            ConfigureImportStatements(generationTarget);

            var filePath = Path.Combine(generationTarget.SourcePath.ToArray());
            filePath = Path.Combine(_rootPath, filePath);
            Directory.CreateDirectory(filePath);
            var fileName = $"{GeneratorUtils.GetSanitizedTypeName(generationTarget.Type).ToKebabCase()}.ts";
            using var writer = new StreamWriter(Path.Combine(filePath, fileName));

            foreach (var import in generationTarget.TypeScriptImports)
            {
                await writer.WriteLineAsync(import);
            }
            await writer.WriteLineAsync();

            if (generationTarget.GenerateInterface)
            {
                await writer.WriteLineAsync(GenerateInterfaceType(generationTarget));
                await writer.WriteLineAsync();
            }

            if(generationTarget.GenerateClass)
            {
                await writer.WriteLineAsync(GenerateClassType(generationTarget));
                await writer.WriteLineAsync();
            }
        }

        private string GenerateClassType(GenerationTarget generationTarget)
        {
            var body = new StringBuilder();
            var ctorFunction = new StringBuilder();

            if (generationTarget.GenericArguments.Any())
            {
                var genericArgs = generationTarget.GenericArguments;
                ctorFunction.AppendLine($"    constructor({string.Join(", ", genericArgs.Select(a => $"type{a}: (new() => {a})"))}) {{");
            }

            foreach (var tsProp in generationTarget.TypeScriptProperties)
            {
                var propNameDeclaration = $"{tsProp.PropertyName.ToCamelCase()}{(tsProp.IsOptional ? "?" : string.Empty)}";
                if (tsProp.IsPropertyTypeAGenericParameter)
                {
                    body.AppendLine($"    public {propNameDeclaration}: {GeneratorUtils.GetTypeScriptPropertyType(tsProp)};");
                    ctorFunction.AppendLine($"        this.{tsProp.PropertyName.ToCamelCase()} = {GeneratorUtils.GetPropertyValueInitializationSnippet(tsProp)};");
                }
                else
                {
                    body.AppendLine($"    public {propNameDeclaration}: {GeneratorUtils.GetTypeScriptPropertyType(tsProp)} = {GeneratorUtils.GetPropertyValueInitializationSnippet(tsProp)};");
                }
            }

            //close constructor function
            if (generationTarget.GenericArguments.Any())
            {
                ctorFunction.AppendLine("    }");
            }

            // begin generating source code
            var sourceCode = new StringBuilder();

            var classTypeName = GeneratorUtils.GetTypeName(generationTarget.Type);
            var ifaceImplementation = generationTarget.GenerateInterface ? $" implements I{classTypeName}" : string.Empty;
            sourceCode.AppendLine($"export class {GeneratorUtils.GetTypeName(generationTarget.Type)}{ifaceImplementation} {{");

            if (generationTarget.GenericArguments.Any())
            {
                sourceCode.AppendLine(ctorFunction.ToString());
                sourceCode.AppendLine();
            }

            sourceCode.AppendLine(body.ToString());
            sourceCode.AppendLine("}");

            return sourceCode.ToString();
        }

        private string GenerateInterfaceType(GenerationTarget generationTarget)
        {
            var iface = new StringBuilder();

            iface.AppendLine($"export interface I{GeneratorUtils.GetTypeName(generationTarget.Type)} {{");

            foreach (var tsProp in generationTarget.TypeScriptProperties)
            {
                iface.AppendLine($"    {tsProp.PropertyName.ToCamelCase()}{(tsProp.IsOptional ? "?" : string.Empty)}: {GeneratorUtils.GetTypeScriptPropertyType(tsProp)};");
            }

            iface.AppendLine("}");

            return iface.ToString();
        }

        private async Task GenerateEnum(GenerationTarget generationTarget)
        {
            var enumType = generationTarget.Type;

            var dirPath = Path.Combine(_rootPath, Path.Combine(generationTarget.SourcePath.ToArray()));
            Directory.CreateDirectory(dirPath);
            var filePath = Path.Combine(dirPath, $"{enumType.Name.ToKebabCase()}.ts");

            var sb = new StringBuilder();
            sb.AppendLine($"export enum {enumType.Name} {{");
            foreach (Enum value in enumType.GetEnumValues())
            {
                sb.AppendLine($"    {Enum.GetName(enumType, value)} = {value.ToString("D")},");
            }
            sb.AppendLine("}");
            sb.AppendLine();

            using var writer = new StreamWriter(filePath);
            await writer.WriteAsync(sb.ToString());
        }

        private TypeScriptType ToTypeScriptType(
            GenerationTarget generationTarget,
            PropertyInfo prop)
        {
            var tsType = new TypeScriptType
            {
                PropertyName = prop.Name,
            };

            var propType = prop.PropertyType;
            if (propType.IsGenericType &&
                propType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                TypeScriptNonValue? regardNullableAs = null;
                var nullableAs = prop.GetCustomAttribute<NullableAsTypeScriptAttribute>();
                if (nullableAs is null)
                {
                    nullableAs = generationTarget.Type.GetCustomAttribute<NullableAsTypeScriptAttribute>();
                }
                if (nullableAs is not null)
                {
                    regardNullableAs = nullableAs.NonValueType;
                }
                if (regardNullableAs is null)
                {
                    regardNullableAs = generationTarget.AreaConfig.RegardNativeNullablesAs is not null ?
                        generationTarget.AreaConfig.RegardNativeNullablesAs :
                        generationTarget.AreaConfig.AssemblyConfig!.RegardNativeNullablesAs is not null ?
                        generationTarget.AreaConfig.AssemblyConfig.RegardNativeNullablesAs : _regardNativeNullablesAs;
                }

                if ((regardNullableAs & TypeScriptNonValue.Undefined) == TypeScriptNonValue.Undefined)
                {
                    tsType.IsUndefinable = true;
                }
                if ((regardNullableAs & TypeScriptNonValue.Null) == TypeScriptNonValue.Null)
                {
                    tsType.IsNullable = true;
                }
                if ((regardNullableAs & TypeScriptNonValue.Optional) == TypeScriptNonValue.Optional)
                {
                    tsType.IsOptional = true;
                }
                propType = propType.GetGenericArguments()[0];
            }

            tsType.IsNullable = tsType.IsNullable || prop.GetCustomAttribute<TypeScriptNullableAttribute>() is not null;
            tsType.IsOptional = tsType.IsOptional || prop.GetCustomAttribute<TypeScriptOptionalAttribute>() is not null;
            tsType.IsUndefinable = tsType.IsUndefinable || tsType.IsOptional || prop.GetCustomAttribute<TypeScriptUndefinableAttribute>() is not null;

            ConfigureTypeScriptType(tsType, propType, generationTarget.GenericArguments);

            return tsType;
        }

        private TypeScriptType ToTypeScriptType(
            Type type,
            IEnumerable<string>? genericParameterNames = null)
        {
            var tsType = new TypeScriptType();

            tsType.IsNullable = type.GetCustomAttribute<TypeScriptNullableAttribute>() is not null;
            tsType.IsOptional = type.GetCustomAttribute<TypeScriptOptionalAttribute>() is not null;
            tsType.IsUndefinable = tsType.IsOptional || type.GetCustomAttribute<TypeScriptUndefinableAttribute>() is not null;

            ConfigureTypeScriptType(tsType, type, genericParameterNames);

            return tsType;
        }

        private void ConfigureTypeScriptType(
            TypeScriptType tsType,
            Type type,
            IEnumerable<string>? genericParameterNames = null)
        {
            if (_propertyTypeMappings.ContainsKey(type))
            {
                type = _propertyTypeMappings[type];
            }

            if (genericParameterNames?.Contains(type.Name) == true)
            {
                tsType.TypeName = type.Name;
                tsType.IsPropertyTypeAGenericParameter = true;
            }
            else if (type == typeof(bool))
            {
                tsType.TypeName = "boolean";
            }
            else if (type == typeof(string))
            {
                tsType.TypeName = "string";
            }
            else if (type == typeof(long) ||
                type == typeof(int) ||
                type == typeof(short) ||
                type == typeof(ulong) ||
                type == typeof(uint) ||
                type == typeof(ushort) ||
                type == typeof(decimal) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                (type.IsEnum && _typesToGenerate.Any(t => t.Type.AssemblyQualifiedName == type.AssemblyQualifiedName) == false))
            {
                tsType.TypeName = "number";
            }
            else if (type.IsEnum && _typesToGenerate.Any(t => t.Type.AssemblyQualifiedName == type.AssemblyQualifiedName))
            {
                tsType.TypeName = type.Name;
                tsType.AssemblyQualifiedName = type.AssemblyQualifiedName!;
                tsType.DefaultEnumValue = type.GetEnumNames()[0];
            }
            else if (type == typeof(DateTime))
            {
                tsType.TypeName = "Date";
            }

            if (string.IsNullOrWhiteSpace(tsType.TypeName) == false) { return; }

            var arrType = GeneratorUtils.GetIEnumerableType(type);
            if (arrType is not null)
            {
                tsType.IsArray = true;
                tsType.GenericArguments.Add(ToTypeScriptType(arrType, genericParameterNames));
            }
            else if (type.IsClass && _typesToGenerate.Any(t => t.Type.AssemblyQualifiedName == type.AssemblyQualifiedName))
            {
                tsType.TypeName = type.Name;
                tsType.AssemblyQualifiedName = type.AssemblyQualifiedName!;
                tsType.IsClass = true;

                if (type.IsGenericType)
                {
                    tsType.GenericArguments = type.GetGenericArguments()
                        .Select(a => ToTypeScriptType(a, genericParameterNames))
                        .ToList();
                }
            }
            else
            {
                throw new NotSupportedException($"The current configuration does not provide support for conversion of type '{type.FullName}' to TypeScript");
            }
        }
    }
}
