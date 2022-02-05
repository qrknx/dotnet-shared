using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorJsBindingsGenerator;

internal static class SemanticModelExtensions
{
    public static bool TryGetConstValue<T>(this SemanticModel semantics,
                                           ExpressionSyntax syntax,
                                           [NotNullWhen(returnValue: true)]out T? result)
        where T : notnull
    {
        if (semantics.GetConstantValue(syntax) is
            {
                HasValue: true,
                Value: T value,
            })
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }
}
