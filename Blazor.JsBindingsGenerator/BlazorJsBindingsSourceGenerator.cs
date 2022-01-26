using Microsoft.CodeAnalysis;

namespace JsBindingsGenerator;

[Generator]
public partial class BlazorJsBindingsSourceGenerator : ISourceGenerator
{
    public const string OutputFileName = "JsBindings.g.cs";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        context.RegisterForPostInitialization(ctx => ctx.AddSource("Attributes.g.cs", AttributesToUse));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (SyntaxContextReceiver)context.SyntaxContextReceiver!;

        if (receiver.ClassesForGeneration.Any())
        {
            string source = GenerateClasses(receiver);

            context.AddSource(OutputFileName, source);
        }
    }
}
