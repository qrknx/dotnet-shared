using System.Text;

namespace JsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private static string GenerateClasses(SyntaxContextReceiver receiver)
    {
        StringBuilder code = new(@"// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
");

        foreach (ClassForGeneration classForGeneration in receiver.ClassesForGeneration)
        {
            code.AppendLine();

            GenerateClass(classForGeneration, code);
        }

        return code.ToString();
    }

    private static void GenerateClass(in ClassForGeneration classForGeneration, StringBuilder code)
    {
        code.Append($@"namespace {classForGeneration.Name.ContainingNodePath}
{{
");

        string access = classForGeneration.IsPublic ? "public" : "internal";

        code.Append($@"    {access} static partial class {classForGeneration.Name.Id}
    {{");

        foreach (Signature signature in classForGeneration.Signatures)
        {
            code.AppendLine();
            GenerateMethod(signature, classForGeneration, code);
        }

        code.Append(@"    }
");

        code.Append(@"}
");
    }

    private static void GenerateMethod(in Signature signature,
                                       in ClassForGeneration classForGeneration,
                                       StringBuilder code)
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

        code.Append($"        public static async {returnType} {normalizedName}(this IJSRuntime js");

        foreach (Param param in signature.Params)
        {
            code.Append($", {param.TypeName.FullName} {param.Name}");
        }

        code.Append(@", CancellationToken token)
        {
            ");

        if (hasResult)
        {
            code.Append($"return await js.InvokeAsync<{fullName}>(");
        }
        else
        {
            code.Append("await js.InvokeVoidAsync(");
        }

        code.Append(classForGeneration.JsContext != "" && !signature.ResetJsContext
                        ? $"\"{classForGeneration.JsContext}.{signature.JsMember}\""
                        : $"\"{signature.JsMember}\"")
            .Append(", token");

        foreach (Param param in signature.Params)
        {
            code.Append($", {param.Name}");
        }

        code.Append(@");
        }
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
