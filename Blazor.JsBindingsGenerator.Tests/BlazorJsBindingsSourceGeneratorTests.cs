using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using JsBindingsGenerator;
using Xunit;
using Xunit.Abstractions;

namespace Blazor.JsBindingsGenerator.Tests;

public class BlazorJsBindingsSourceGeneratorTests
{
    [Fact]
    public async Task AttributesForConsumer_Generated()
    {
        BlazorJsBindingsSourceGeneratorWrapper wrapper = new();

        await wrapper.RunAsync();
    }

    [Theory]
    [ClassData(typeof(TestDataProvider))]
    public async Task Bindings_Generated(TestCase @case)
    {
        BlazorJsBindingsSourceGeneratorWrapper wrapper = new()
        {
            WithSources = @case.Sources,
            GeneratedJsBindings = @case.Generated,
        };

        await wrapper.RunAsync();
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
