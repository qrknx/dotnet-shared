using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.JSInterop;
using Xunit;
using Xunit.Abstractions;

namespace JsBindingsGenerator.Tests;

public class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public void AttributesForConsumer_Generated()
    {
        GeneratorDriverRunResult result = RunGenerator(sources: Enumerable.Empty<string>());

        Assert.Single(result.Results);

        GeneratorRunResult runResult = result.Results.First();

        Assert.Null(runResult.Exception);

        Assert.Collection(runResult.GeneratedSources, AssertGeneratedAttributes());
    }

    [Theory]
    [ClassData(typeof(TestDataProvider))]
    public void Bindings_Generated(TestCase @case)
    {
        GeneratorDriverRunResult result = RunGenerator(sources: @case.Sources);

        Assert.Single(result.Results);

        GeneratorRunResult runResult = result.Results.First();

        Assert.Null(runResult.Exception);

        Assert.Collection(runResult.GeneratedSources,
                          AssertGeneratedAttributes(),
                          AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.OutputFileName,
                                          sourceText: @case.Generated));
    }

    private static GeneratorDriverRunResult RunGenerator(IEnumerable<string> sources)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: sources.Select(s => CSharpSyntaxTree.ParseText(s)),
            references: GetMetadataReferences());

        BlazorJsBindingsSourceGenerator generator = new();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation)
                     .GetRunResult();
    }

    private static MetadataReference[] GetMetadataReferences() => new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(IJSRuntime).Assembly.Location),
    };

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

    private class TestDataProvider : TheoryData<TestCase>
    {
        private const string TestDataPrefix = "Blazor.JsBindingsGenerator.Tests.JsBindingsTestData.";

        public TestDataProvider()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var testCases =
                from name in assembly.GetManifestResourceNames()
                where name.StartsWith(TestDataPrefix, StringComparison.Ordinal)
                let caseNameLength = name.Skip(TestDataPrefix.Length).TakeWhile(c => c != '.').Count()
                let caseName = name[TestDataPrefix.Length..(TestDataPrefix.Length + caseNameLength + 1)]
                let contents = ReadFile(name)
                group (name, contents) by caseName
                into caseFiles
                select new TestCase(
                    Name: caseFiles.Key,
                    Sources: caseFiles.Where(f => !IsGenerated(f.name)).Select(f => f.contents),
                    Generated: caseFiles.Single(f => IsGenerated(f.name)).contents);

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
