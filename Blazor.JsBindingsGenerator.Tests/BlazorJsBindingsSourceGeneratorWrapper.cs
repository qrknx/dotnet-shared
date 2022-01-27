using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using JsBindingsGenerator;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Blazor.JsBindingsGenerator.Tests;

internal class BlazorJsBindingsSourceGeneratorWrapper
{
    private const string GeneratedAttributes = @"// Auto-generated
#nullable enable

using System;

namespace JsBindingsGenerator;

[AttributeUsage(AttributeTargets.Class)]
internal class JsBindingContextAttribute : Attribute
{
    public JsBindingContextAttribute(string jsContext) {}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class JsBindAttribute : Attribute
{
    public Type? Params { get; init; }

    public Type Returns { get; init; } = typeof(void);

    public Type ReturnsNullable { get; init; } = typeof(void);

    public bool ResetContext { get; init; }

    public JsBindAttribute(string member) {}
}
";

    public readonly CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator> Verifier = new()
    {
        TestState =
        {
            GeneratedSources =
            {
                GeneratedSource(BlazorJsBindingsSourceGenerator.AttributesOutputFileName, GeneratedAttributes),
            },
        },
        // todo net6.0
        ReferenceAssemblies
            = ReferenceAssemblies.Net
                                 .Net50
                                 .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.JSInterop",
                                                                        "5.0.13"))),
    };

    public IEnumerable<string> WithSources
    {
        init
        {
            foreach (string source in value)
            {
                Sources.Add(source);
            }
        }
    }

    public SourceFileList Sources => Verifier.TestState.Sources;

    public string GeneratedJsBindings
    {
        init => Verifier.TestState
                        .GeneratedSources.Add(GeneratedSource(BlazorJsBindingsSourceGenerator.OutputFileName, value));
    }

    public async Task RunAsync() => await Verifier.RunAsync();

    private static (Type, string, SourceText) GeneratedSource(string fileName, string generated) =>
    (
        typeof(BlazorJsBindingsSourceGenerator),
        fileName,
        SourceText.From(generated, Encoding.UTF8)
    );
}
