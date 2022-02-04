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
        code.Append($@"namespace {classForGeneration.Type.ContainingParentsPath}
{{
");

        string access = classForGeneration.Type.IsPublic ? "public" : "internal";

        code.Append($@"    {access} static partial class {classForGeneration.Type.ShortId}
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
        bool hasResult = !signature.ReturnType.Equals(TypeView.Void);

        string fullName;
        string returnType;

        if (hasResult)
        {
            fullName = signature.ReturnType.AnnotatedFullId;
            returnType = $"Task<{fullName}>";
        }
        else
        {
            fullName = "";
            returnType = "Task";
        }

        string normalizedName = NormalizeId(signature.JsMember);

        string access = signature.IsPublic
            ? "public"
            : "internal";

        code.Append($"        {access} static async {returnType} {normalizedName}(this IJSRuntime js");

        foreach (Param param in signature.Params)
        {
            code.Append($", {param.Type.AnnotatedFullId} {param.Name}");
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

        if (signature.Params.Count is 0 or > 1)
        {
            foreach (Param param in signature.Params)
            {
                code.Append($", {param.Name}");
            }
        }
        else
        {
            Param param = signature.Params[0];

            code.Append(param is { Type.IsArray: true }
                            ? $", (object){param.Name}"
                            : $", {param.Name}");
        }

        code.Append(@");
        }
");
    }

    private unsafe static string NormalizeId(string jsId)
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

        jsId.AsSpan(start: 1).CopyTo(chars.Slice(1));

        if (!jsId.EndsWith(asyncSuffix, StringComparison.Ordinal))
        {
            asyncSuffix.AsSpan().CopyTo(chars.Slice(chars.Length - asyncSuffix.Length));
        }
        else
        {
            chars = chars.Slice(0, chars.Length - asyncSuffix.Length);
        }

        fixed (char* ptr = chars)
        {
            return new string(ptr, 0, chars.Length);
        }
    }
}
