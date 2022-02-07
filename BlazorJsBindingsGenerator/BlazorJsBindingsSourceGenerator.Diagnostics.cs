using Microsoft.CodeAnalysis;

namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
#pragma warning disable RS2008
    private static class CouldNotCreateIdentifier
    {
        private static readonly DiagnosticDescriptor Descriptor = new(
            id: "BJSB1001",
            title: "Couldn't create identifier",
            messageFormat: "Couldn't create identifier for JS expression `{0}`",
            category: "Naming",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static void Report(in Signature signature, string jsPath, SourceProductionContext ctx)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Descriptor,
                                                   Location.Create(signature.SyntaxTree, signature.TextSpan),
                                                   jsPath));
        }
    }
#pragma warning restore
}
