using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Blazor.JsBindingsGenerator.Tests;

public class CSharpSourceGeneratorVerifier<T> : CSharpSourceGeneratorTest<T, XUnitVerifier>
    where T : ISourceGenerator, new()
{
    protected override CompilationOptions CreateCompilationOptions()
    {
        var options = (CSharpCompilationOptions)base.CreateCompilationOptions();
        
        return options
            .WithSpecificDiagnosticOptions(options.SpecificDiagnosticOptions.SetItems(GetNullableWarnings()))
            .WithNullableContextOptions(NullableContextOptions.Enable);
    }

    protected override ParseOptions CreateParseOptions()
    {
        return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.CSharp10);
    }

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarnings()
    {
        CSharpCommandLineArguments args
            = CSharpCommandLineParser.Default.Parse(args: new[] { "/warnaserror:nullable" },
                                                    baseDirectory: Environment.CurrentDirectory,
                                                    sdkDirectory: Environment.CurrentDirectory);

        ImmutableDictionary<string, ReportDiagnostic> nullableWarnings
            = args.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }
}
