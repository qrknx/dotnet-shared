using System.Text;
using Microsoft.CodeAnalysis;

namespace JsBindingsGenerator;

[Generator]
internal partial class BlazorJsBindingsSourceGenerator : ISourceGenerator
{
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
            StringBuilder source = GenerateClasses(receiver);

            context.AddSource("JsBindings.g.cs", source.ToString());
        }
    }
}
