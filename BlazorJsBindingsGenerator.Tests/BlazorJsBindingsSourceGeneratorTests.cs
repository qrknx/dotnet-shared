using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace BlazorJsBindingsGenerator.Tests;

public partial class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public void AttributesForConsumer_Generated()
    {
        GeneratorDriverRunResult result = RunGenerator(sources: Enumerable.Empty<string>());

        Assert.Single(result.Results);

        GeneratorRunResult runResult = result.Results.First();

        Assert.Null(runResult.Exception);
        Assert.Empty(runResult.Diagnostics);

        Assert.Collection(runResult.GeneratedSources, AssertGeneratedAttributes());
    }

    [Theory]
    [ClassData(typeof(JsBindingsTestDataProvider))]
    public void Bindings_Generated(TestCase @case)
    {
        GeneratorDriverRunResult result = RunGenerator(sources: @case.Sources);

        Assert.Single(result.Results);

        GeneratorRunResult runResult = result.Results.First();

        Assert.Null(runResult.Exception);
        Assert.Empty(runResult.Diagnostics);

        Assert.Collection(runResult.GeneratedSources,
                          AssertGeneratedAttributes(),
                          AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.OutputFileName,
                                          sourceText: @case.Generated));
    }

    [Fact]
    public void Diagnostic_InvalidName_Reported()
    {
        List<(string Name, string Contents, bool IsGenerated)> testData
            = GetEmbeddedTestData("BlazorJsBindingsGenerator.Tests.DiagnosticsTestData.InvalidName.").ToList();

        GeneratorDriverRunResult result = RunGenerator(sources: testData.Where(d => !d.IsGenerated)
                                                                        .Select(d => d.Contents));

        Assert.Single(result.Results);

        GeneratorRunResult runResult = result.Results.First();

        Assert.Null(runResult.Exception);

        Assert.Collection(runResult.GeneratedSources,
                          AssertGeneratedAttributes(),
                          AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.OutputFileName,
                                          sourceText: testData.Single(d => d.IsGenerated).Contents));

        Assert.Collection(runResult.Diagnostics,
                          AssertInvalidName,
                          AssertInvalidName);
    }
}
