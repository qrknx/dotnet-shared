using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace BlazorJsBindingsGenerator;

/// <summary>
/// https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
/// https://andrewlock.net/creating-a-source-generator-part-2-testing-an-incremental-generator-with-snapshot-testing/
/// Old <see cref="ISourceGenerator"/>:
/// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
/// </summary>
[Generator]
public partial class BlazorJsBindingsSourceGenerator : IIncrementalGenerator
{
    public const string AttributesOutputFileName = "Attributes.g.cs";
    public const string OutputFileName = "JsBindings.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            static ctx => ctx.AddSource(AttributesOutputFileName, AttributesToUse));

        IncrementalValueProvider<ImmutableArray<ClassForGeneration?>> pipeline
            = context.SyntaxProvider.CreateSyntaxProvider(
                         predicate: IsForGeneration,
                         transform: TryGetClassForGeneration)
                     .WithComparer(ClassForGenerationEqualityComparer.Instance)
                     .Where(static cfg => ShouldGenerate(cfg))
                     .Collect();

        context.RegisterSourceOutput(pipeline,
                                     static (ctx, classes) =>
                                     {
                                         if (classes.Length > 0)
                                         {
                                             string source = GenerateSourceText(classes.Select(c => c!.Value), ctx);

                                             ctx.AddSource(OutputFileName, source);
                                         }
                                     });
    }
}
