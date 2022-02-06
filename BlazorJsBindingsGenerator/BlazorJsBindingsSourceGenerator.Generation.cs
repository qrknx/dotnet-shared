using System.Text;
using Microsoft.CodeAnalysis;

namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
    private static string GenerateSourceText(IEnumerable<ClassForGeneration> classes, SourceProductionContext ctx)
    {
        StringBuilder code = new(@"// Auto-generated
#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
");

        foreach (ClassForGeneration classForGeneration in classes)
        {
            code.AppendLine();

            GenerateClass(classForGeneration, code, ctx);
        }

        return code.ToString();
    }

    private static void GenerateClass(in ClassForGeneration classForGeneration,
                                      StringBuilder code,
                                      SourceProductionContext ctx)
    {
        code.Append($@"namespace {classForGeneration.Type.ContainingParentsPath}
{{
");

        string access = classForGeneration.Type.IsPublic ? "public" : "internal";

        code.Append($@"    {access} static partial class {classForGeneration.Type.ShortId}
    {{");

        foreach (Signature signature in classForGeneration.Signatures)
        {
            if (TryGetCSharpId(signature.JsMember, signature, ctx) is {} csharpId)
            {
                code.AppendLine();
                GenerateMethod(csharpId, signature, code);
            }
        }

        code.Append(@"    }
");

        code.Append(@"}
");
    }

    private static void GenerateMethod(string csharpId, in Signature signature, StringBuilder code)
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

        string access = signature.IsPublic
            ? "public"
            : "internal";

        code.Append($"        {access} static async {returnType} {csharpId}(this IJSRuntime js");

        foreach (Param param in signature.Params)
        {
            code.Append($", {param.Type.AnnotatedFullId} {param.Name}");
        }

        code.Append(@", CancellationToken token)
        {
            ");

        code.Append(hasResult 
                        ? $"return await js.InvokeAsync<{fullName}>"
                        : "await js.InvokeVoidAsync")
            .Append($"(\"{GetFullJsPath(signature)}\", token");

        switch (signature.Params.Count)
        {
            case > 1:
                foreach (Param param in signature.Params)
                {
                    code.Append($", {param.Name}");
                }
                break;

            case 1:
                Param singleParam = signature.Params[0];

                code.Append(singleParam.Type switch
                {
                    { IsArray: false } => $", {singleParam.Name}",
                    { IsNullable: false } => $", (object){singleParam.Name}",
                    { IsNullable: true } => $", (object?){singleParam.Name}",
                });
                break;
        }

        code.Append(@");
        }
");
    }

    private static string GetFullJsPath(in Signature signature) => signature.JsPrefix != ""
        ? $"{signature.JsPrefix}.{signature.JsMember}"
        : signature.JsMember;

    private unsafe static string? TryGetCSharpId(string jsPath, in Signature signature, SourceProductionContext ctx)
    {
        string jsId = Sanitize(jsPath);

        const string asyncSuffix = "Async";

        // (511 - 5) is related to CS0645 and asyncSuffix.Length.
        if (jsId.Length is 0 or > (511 - 5))
        {
            CouldNotCreateIdentifier.Report(signature, jsPath, ctx);

            return null;
        }

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

        static string Sanitize(string id)
        {
            int i = 0;

            while (i < id.Length && !char.IsLetter(id[i]) && id[i] != '_')
            {
                ++i;
            }

            if (i < id.Length)
            {
                StringBuilder sb = new(capacity: id.Length - i);

                sb.Append(id[i]);
                ++i;

                while (i < id.Length)
                {
                    char c = id[i];

                    if (char.IsLetter(c) || char.IsDigit(c) || c == '_')
                    {
                        sb.Append(c);
                    }

                    ++i;
                }

                return sb.ToString();
            }

            return "";
        }
    }
}
