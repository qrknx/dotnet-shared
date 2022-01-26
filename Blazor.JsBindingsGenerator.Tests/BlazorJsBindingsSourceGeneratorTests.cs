using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsBindingsGenerator;
using Xunit;

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
    [ClassData(typeof(TestData))]
    public async Task Class_Generated(IEnumerable<string> sources, string generated)
    {
        BlazorJsBindingsSourceGeneratorWrapper wrapper = new()
        {
            WithSources = sources,
            GeneratedJsBindings = generated,
        };

        await wrapper.RunAsync();
    }

    private class TestData : TheoryData<IEnumerable<string>, string>
    {
        private const string TestDataPrefix = "Blazor.JsBindingsGenerator.Tests.JsBindingsTestData.";

        public TestData()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var data =
                from name in assembly.GetManifestResourceNames()
                where name.StartsWith(TestDataPrefix, StringComparison.Ordinal)
                let caseNameLength = name.Skip(TestDataPrefix.Length).TakeWhile(c => c != '.').Count()
                let caseName = name[TestDataPrefix.Length..(TestDataPrefix.Length + caseNameLength + 1)]
                let contents = ReadFile(name)
                group (name, contents) by caseName
                into caseFiles
                select (caseFiles.Where(f => !IsGenerated(f.name)).Select(f => f.contents),
                        caseFiles.Single(f => IsGenerated(f.name)).contents);

            foreach ((IEnumerable<string> sources, string generated) in data)
            {
                Add(sources, generated);
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
}
