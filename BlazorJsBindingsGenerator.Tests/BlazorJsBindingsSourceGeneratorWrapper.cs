// May be used when standard testing suite for IIncrementalGenerator will be provided.


//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Text;
//using System.Threading.Tasks;
//using BlazorJsBindingsGenerator;
//using Microsoft.CodeAnalysis.Testing;
//using Microsoft.CodeAnalysis.Text;

//namespace Blazor.JsBindingsGenerator.Tests;

//[Fact]
//public async Task AttributesForConsumer_Generated()
//{
//    BlazorJsBindingsSourceGeneratorWrapper wrapper = new();

//    await wrapper.RunAsync();
//}

//[Theory]
//[ClassData(typeof(TestDataProvider))]
//public async Task Bindings_Generated(TestCase @case)
//{
//    BlazorJsBindingsSourceGeneratorWrapper wrapper = new()
//    {
//        WithSources = @case.Sources,
//        GeneratedJsBindings = @case.Generated,
//    };

//    await wrapper.RunAsync();
//}

//public class CSharpSourceGeneratorVerifier<T> : CSharpSourceGeneratorTest<T, XUnitVerifier>
//    where T : ISourceGenerator, new()
//{
//    protected override CompilationOptions CreateCompilationOptions()
//    {
//        var options = (CSharpCompilationOptions)base.CreateCompilationOptions();

//        return options
//            .WithSpecificDiagnosticOptions(options.SpecificDiagnosticOptions.SetItems(GetNullableWarnings()))
//            .WithNullableContextOptions(NullableContextOptions.Enable);
//    }

//    protected override ParseOptions CreateParseOptions()
//    {
//        return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.CSharp10);
//    }

//    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarnings()
//    {
//        CSharpCommandLineArguments args
//            = CSharpCommandLineParser.Default.Parse(args: new[] { "/warnaserror:nullable" },
//                                                    baseDirectory: Environment.CurrentDirectory,
//                                                    sdkDirectory: Environment.CurrentDirectory);

//        ImmutableDictionary<string, ReportDiagnostic> nullableWarnings
//            = args.CompilationOptions.SpecificDiagnosticOptions;

//        return nullableWarnings;
//    }
//}

//internal class BlazorJsBindingsSourceGeneratorWrapper
//{
//    public readonly CSharpSourceGeneratorVerifier<BlazorJsBindingsSourceGenerator> Verifier = new()
//    {
//        TestState =
//        {
//            GeneratedSources =
//            {
//                GeneratedSource(BlazorJsBindingsSourceGenerator.AttributesOutputFileName,
//                                BlazorJsBindingsSourceGenerator.AttributesToUse),
//            },
//        },
//        // todo net6.0
//        ReferenceAssemblies
//            = ReferenceAssemblies.Net
//                                 .Net50
//                                 .AddPackages(ImmutableArray.Create(new PackageIdentity("Microsoft.JSInterop",
//                                                                        "5.0.13"))),
//    };

//    public IEnumerable<string> WithSources
//    {
//        init
//        {
//            foreach (string source in value)
//            {
//                Sources.Add(source);
//            }
//        }
//    }

//    public SourceFileList Sources => Verifier.TestState.Sources;

//    public string GeneratedJsBindings
//    {
//        init => Verifier.TestState
//                        .GeneratedSources.Add(GeneratedSource(BlazorJsBindingsSourceGenerator.OutputFileName, value));
//    }

//    public async Task RunAsync() => await Verifier.RunAsync();

//    private static (Type, string, SourceText) GeneratedSource(string fileName, string generated) =>
//    (
//        typeof(BlazorJsBindingsSourceGenerator),
//        fileName,
//        SourceText.From(generated, Encoding.UTF8)
//    );
//}
