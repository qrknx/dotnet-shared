using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.JSInterop;
using Xunit;
using Xunit.Abstractions;

namespace BlazorJsBindingsGenerator.Tests;

public partial class BlazorJsBindingsSourceGeneratorTests
{
    private static readonly string SdkDllsLocation = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

    private static readonly MetadataReference[] MetadataReferences =
    {
        MetadataReference.CreateFromFile(Path.Combine(SdkDllsLocation, "System.Private.CoreLib.dll")),
        MetadataReference.CreateFromFile(Path.Combine(SdkDllsLocation, "System.Runtime.dll")),
        MetadataReference.CreateFromFile(typeof(IJSRuntime).Assembly.Location),
    };

    // /define:SOME compilation options belong to CSharpParseOptions.
    // They are among parameters of CSharpSyntaxTree.ParseText and CSharpGeneratorDriver.Create methods.
    private static readonly CSharpCompilationOptions CompilationOptions
        = new(OutputKind.DynamicallyLinkedLibrary,
              specificDiagnosticOptions: CSharpCommandLineParser.Default
                                                                .Parse(args: new[] { "/warnaserror:nullable" },
                                                                       baseDirectory: Environment.CurrentDirectory,
                                                                       sdkDirectory: Environment.CurrentDirectory)
                                                                .CompilationOptions
                                                                .SpecificDiagnosticOptions,
              nullableContextOptions: NullableContextOptions.Enable,
              optimizationLevel: OptimizationLevel.Release);

    private static readonly (string Name, string Contents, bool IsGenerated)[] InvalidNameTestData
        = GetEmbeddedTestData("BlazorJsBindingsGenerator.Tests.DiagnosticsTestData.InvalidName.").ToArray();

    private static GeneratorRunResult RunGenerator(IEnumerable<string> sources)
    {
        return RunGenerator(sources, outDll: Stream.Null);
    }

    private static GeneratorRunResult RunGenerator(IEnumerable<string> sources, Stream outDll)
    {
        List<SyntaxTree> sourceSyntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

        CSharpCompilation compilation = CreateCompilation(sourceSyntaxTrees);

        BlazorJsBindingsSourceGenerator generator = new();

        GeneratorDriverRunResult generatorDriverRunResult = CSharpGeneratorDriver.Create(generator)
                                                                                 .RunGenerators(compilation)
                                                                                 .GetRunResult();

        GeneratorRunResult runResult = Assert.Single(generatorDriverRunResult.Results);

        Assert.Null(runResult.Exception);

        IEnumerable<SyntaxTree> allSources = sourceSyntaxTrees.Concat(
            runResult.GeneratedSources.Select(s => s.SyntaxTree));

        CSharpCompilation fullCompilation = CreateCompilation(allSources);

        EmitResult emitResult = fullCompilation.Emit(outDll);

        Assert.True(emitResult.Success);
        Assert.Empty(emitResult.Diagnostics);

        return runResult;
    }

    private static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create(
        assemblyName: "Tests",
        syntaxTrees: sources,
        references: MetadataReferences,
        options: CompilationOptions);

    private static Action<GeneratedSourceResult> AssertGeneratedAttributes()
    {
        return AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.AttributesOutputFileName,
                               sourceText: BlazorJsBindingsSourceGenerator.AttributesToUse);
    }

    private static Action<GeneratedSourceResult> AssertGenerated(string hintName, string sourceText)
    {
        return actual =>
        {
            Assert.Equal(expected: hintName, actual: actual.HintName);

            string actualSourceText = actual.SourceText.ToString();

            if (sourceText != actualSourceText)
            {
                DiffPaneModel diff = InlineDiffBuilder.Instance.BuildDiffModel(sourceText,
                                                                               actualSourceText,
                                                                               ignoreWhitespace: false,
                                                                               ignoreCase: false,
                                                                               LineChunker.Instance);

                StringBuilder sb = new();

                sb.AppendLine("Actual and expected values differ. Expected shown in baseline of diff:");

                if (!diff.Lines.Any(line => line.Type is ChangeType.Inserted or ChangeType.Deleted))
                {
                    // We have a failure only caused by line ending differences; recalculate with line endings visible
                    diff = InlineDiffBuilder.Instance.BuildDiffModel(sourceText,
                                                                     actualSourceText,
                                                                     ignoreWhitespace: false,
                                                                     ignoreCase: false,
                                                                     LineEndingsPreservingChunker.Instance);
                }

                foreach (DiffPiece line in diff.Lines)
                {
                    sb.Append(line.Type switch
                    {
                        ChangeType.Inserted => '+',
                        ChangeType.Deleted => '-',
                        _ => ' ',
                    });

                    sb.AppendLine(line.Text
                                      .Replace("\r", "<CR>")
                                      .Replace("\n", "<LF>"));
                }

                Assert.True(false, sb.ToString());
            }
        };
    }

    private static void AssertInvalidName(Diagnostic actual)
    {
        Assert.Equal(expected: DiagnosticSeverity.Error, actual.DefaultSeverity);
        Assert.Equal(expected: "BJSBG1001", actual.Id);
    }

    private static IEnumerable<(string Name, string Contents, bool IsGenerated)> GetEmbeddedTestData(string prefix)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        return from name in assembly.GetManifestResourceNames()
               where name.StartsWith(prefix, StringComparison.Ordinal)
               select (name, ReadFile(name), IsGenerated(name));

        string ReadFile(string name)
        {
            using Stream stream = assembly.GetManifestResourceStream(name)!;
            using StreamReader reader = new(stream);

            return reader.ReadToEnd();
        }

        static bool IsGenerated(string name)
            => name.EndsWith(BlazorJsBindingsSourceGenerator.OutputFileName, StringComparison.Ordinal);
    }

    private class JsBindingsTestDataProvider : TheoryData<TestCase>
    {
        private const string TestDataPrefix = "BlazorJsBindingsGenerator.Tests.JsBindingsTestData.";

        public JsBindingsTestDataProvider()
        {
            var testCases =
                from testData in GetEmbeddedTestData(TestDataPrefix)
                let caseNameLength = testData.Name.Skip(TestDataPrefix.Length).TakeWhile(c => c != '.').Count()
                let caseName = testData.Name[TestDataPrefix.Length..(TestDataPrefix.Length + caseNameLength + 1)]
                group testData by caseName
                into caseFiles
                select new TestCase(Name: caseFiles.Key,
                                    Sources: caseFiles.Where(f => !f.IsGenerated)
                                                      .Select(f => f.Contents),
                                    Generated: caseFiles.Single(f => f.IsGenerated).Contents);

            foreach (TestCase testCase in testCases)
            {
                Add(testCase);
            }
        }
    }

    public record struct TestCase(string Name, IEnumerable<string> Sources, string Generated)
        : IXunitSerializable
    {
        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Sources = JsonSerializer.Deserialize<List<string>>(info.GetValue<string>(nameof(Sources)))!;
            Generated = info.GetValue<string>(nameof(Generated));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Sources), JsonSerializer.Serialize(Sources));
            info.AddValue(nameof(Generated), Generated);
        }

        public override string ToString() => Name.TrimEnd('.');
    }
}
