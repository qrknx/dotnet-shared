using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace BlazorJsBindingsGenerator.Tests;

public partial class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public void AttributesForConsumer_Generated()
    {
        GeneratorRunResult runResult = RunGenerator(sources: Enumerable.Empty<string>());

        Assert.Empty(runResult.Diagnostics);

        Assert.Collection(runResult.GeneratedSources, AssertGeneratedAttributes());
    }

    [Theory]
    [ClassData(typeof(JsBindingsTestDataProvider))]
    public void Bindings_Generated(TestCase @case)
    {
        GeneratorRunResult runResult = RunGenerator(sources: @case.Sources);

        Assert.Empty(runResult.Diagnostics);

        Assert.Collection(runResult.GeneratedSources,
                          AssertGeneratedAttributes(),
                          AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.OutputFileName,
                                          sourceText: @case.Generated));
    }

    [Fact]
    public void Diagnostic_InvalidName_Reported()
    {
        GeneratorRunResult runResult = RunGenerator(sources: InvalidNameTestData.Sources);

        Assert.Collection(runResult.GeneratedSources,
                          AssertGeneratedAttributes(),
                          AssertGenerated(hintName: BlazorJsBindingsSourceGenerator.OutputFileName,
                                          sourceText: InvalidNameTestData.Generated));

        Assert.Collection(runResult.Diagnostics,
                          AssertInvalidName,
                          AssertInvalidName);
    }

    [Fact]
    public void Attributes_NotVisible()
    {
        RunGenerator(sources: InvalidNameTestData.Sources, out Assembly assembly);

        Type type = assembly.GetType("N.C", throwOnError: true)!;

        Assert.NotNull(type);

        Assert.Collection(type.GetCustomAttributes(),
                          attribute => Assert.IsType<ExtensionAttribute>(attribute));
    }
}
