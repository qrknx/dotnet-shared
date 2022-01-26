using System.Text;

namespace JsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private static string GenerateClasses(SyntaxContextReceiver receiver)
    {
        StringBuilder source = new(@"// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
");

        foreach (ClassForGeneration classForGeneration in receiver.ClassesForGeneration)
        {
            source.AppendLine();

            GenerateClass(classForGeneration, source);
        }

        return source.ToString();
    }

    private static void GenerateClass(in ClassForGeneration classForGeneration, StringBuilder source)
    {
        source.Append($@"namespace {classForGeneration.Name.ContainingNodePath}
{{
");

        string access = classForGeneration.IsPublic ? "public" : "internal";

        source.Append($@"    {access} static partial class {classForGeneration.Name.Id}
    {{");

        foreach (Signature signature in classForGeneration.Signatures)
        {
            bool hasResult = !signature.ReturnTypeName.Equals(TypeName.Void);

            string returnType;
            string fullName;

            if (hasResult)
            {
                fullName = signature.ReturnTypeName.FullName;
                returnType = $"Task<{fullName}>";
            }
            else
            {
                fullName = "";
                returnType = "Task";
            }

            string normalizedName = NormalizeId(signature.JsMember);

            source.Append($@"
        public static async {returnType} {normalizedName}(this IJSRuntime js");

            foreach (Param param in signature.Params)
            {
                source.Append($", {param.TypeName.FullName} {param.Name}");
            }

            source.Append(@", CancellationToken token)
        {
            ");

            if (hasResult)
            {
                source.Append($"return await js.InvokeAsync<{fullName}>(");
            }
            else
            {
                source.Append("await js.InvokeVoidAsync(");
            }

            source.Append(!signature.ResetJsContext
                              ? $"\"{classForGeneration.JsContext}.{signature.JsMember}\""
                              : $"\"{signature.JsMember}\"")
                  .Append(", token");

            foreach (Param param in signature.Params)
            {
                source.Append($", {param.Name}");
            }

            source.Append(@");
        }
");
        }

        source.Append(@"    }
");

        source.Append(@"}
");
    }

    private static string NormalizeId(string jsId)
    {
        switch (jsId)
        {
            case { Length: 0 }:
                throw new Exception("Empty string cannot be used as identifier.");

            // CS0645
            case { Length: > 511 }:
                throw new Exception("Identifier too long.");
        }

        const string asyncSuffix = "Async";

        Span<char> chars = stackalloc char[jsId.Length + asyncSuffix.Length];

        char firstChar = jsId[0];

        chars[0] = char.IsLetter(firstChar) && char.IsLower(firstChar)
            ? char.ToUpper(firstChar)
            : firstChar;

        jsId.AsSpan(start: 1).CopyTo(chars[1..]);

        if (!jsId.EndsWith(asyncSuffix, StringComparison.Ordinal))
        {
            asyncSuffix.CopyTo(chars[^asyncSuffix.Length..]);
        }
        else
        {
            chars = chars[..^asyncSuffix.Length];
        }

        return new string(chars);
    }
}
