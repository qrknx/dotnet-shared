using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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

    private static readonly CSharpCompilationOptions CompilationOptions
        = new(OutputKind.DynamicallyLinkedLibrary,
              specificDiagnosticOptions: CSharpCommandLineParser.Default
                                                                .Parse(args: new[] { "/warnaserror:nullable" },
                                                                       baseDirectory: Environment.CurrentDirectory,
                                                                       sdkDirectory: Environment.CurrentDirectory)
                                                                .CompilationOptions
                                                                .SpecificDiagnosticOptions,
              nullableContextOptions: NullableContextOptions.Enable);

    private static GeneratorDriverRunResult RunGenerator(IEnumerable<string> sources)
    {
        List<SyntaxTree> sourceSyntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();

        CSharpCompilation compilation = CreateCompilation(sourceSyntaxTrees);

        BlazorJsBindingsSourceGenerator generator = new();

        GeneratorDriverRunResult runResult = CSharpGeneratorDriver.Create(generator)
                                                                  .RunGenerators(compilation)
                                                                  .GetRunResult();

        CSharpCompilation fullCompilation = CreateCompilation(sourceSyntaxTrees.Concat(runResult.GeneratedTrees));

        EmitResult emitResult = fullCompilation.Emit(Stream.Null);

        Assert.True(emitResult.Success);

        return runResult;
    }

    private static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> sources)
    {
        return CSharpCompilation.Create(assemblyName: "Tests",
                                        syntaxTrees: sources,
                                        references: MetadataReferences,
                                        options: CompilationOptions);
    }

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
            Assert.Equal(expected: sourceText, actual: actual.SourceText.ToString());
        };
    }

    private class JsBindingsTestDataProvider : TheoryData<TestCase>
    {
        private const string TestDataPrefix = "BlazorJsBindingsGenerator.Tests.JsBindingsTestData.";

        public JsBindingsTestDataProvider()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var testCases =
                from name in assembly.GetManifestResourceNames()
                where name.StartsWith(TestDataPrefix, StringComparison.Ordinal)
                let caseNameLength = name.Skip(TestDataPrefix.Length).TakeWhile(c => c != '.').Count()
                let caseName = name[TestDataPrefix.Length..(TestDataPrefix.Length + caseNameLength + 1)]
                group (name, Contents: ReadFile(name)) by caseName
                into caseFiles
                select new TestCase(Name: caseFiles.Key,
                                    Sources: caseFiles.Where(f => !IsGenerated(f.name))
                                                      .Select(f => f.Contents),
                                    Generated: caseFiles.Single(f => IsGenerated(f.name)).Contents);

            foreach (var testCase in testCases)
            {
                Add(testCase);
            }

            string ReadFile(string name)
            {
                using Stream stream = assembly.GetManifestResourceStream(name)!;
                using StreamReader reader = new(stream);

                return reader.ReadToEnd();
            }

            static bool IsGenerated(string name)
                => name.EndsWith(BlazorJsBindingsSourceGenerator.OutputFileName, StringComparison.Ordinal);
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
