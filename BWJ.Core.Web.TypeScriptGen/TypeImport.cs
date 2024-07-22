using BWJ.Core.Web.TypeScriptGen.Annotation;

namespace BWJ.Core.Web.TypeScriptGen
{
    internal class TypeImport
    {
        public TypeImport(GenerationTarget generationTarget)
        {
            GenerationTarget = generationTarget;
        }

        public GenerationTarget GenerationTarget { get; }
        public TypeScriptObjectAsset? ImportedTypes { get; set; }
    }
}
