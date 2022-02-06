using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BlazorJsBindingsGenerator;

public partial class BlazorJsBindingsSourceGenerator
{
#pragma warning disable CS8618, CS0649

    private struct ClassForGeneration
    {
        public TypeView Type;
        public List<Signature> Signatures;
    }

    private sealed class ClassForGenerationEqualityComparer : IEqualityComparer<ClassForGeneration?>
    {
        public static readonly ClassForGenerationEqualityComparer Instance = new();

        public bool Equals(ClassForGeneration? x, ClassForGeneration? y)
        {
            if (x is {} left && y is {} right)
            {
                return left.Type.Equals(right.Type)
                       && left.Signatures.SequenceEqual(right.Signatures);
            }

            return x is null && y is null;
        }

        public int GetHashCode(ClassForGeneration? obj)
        {
            if (obj is {} cfg)
            {
                unchecked
                {
                    int hashCode = cfg.Type.GetHashCode();
                    hashCode = (hashCode * 397) ^ cfg.Signatures.GetHashCode();
                    return hashCode;
                }
            }

            return 0;
        }
    }

    private struct Signature : IEquatable<Signature>
    {
        public List<Param> Params;
        public string JsMember;
        public string JsPrefix;
        public TypeView ReturnType;

        public SyntaxTree SyntaxTree;
        public TextSpan TextSpan;

        public bool IsPublic => ReturnType.IsPublic && Params.TrueForAll(p => p.Type.IsPublic);

        public bool Equals(Signature other) => JsMember == other.JsMember
                                               && ReturnType.Equals(other.ReturnType)
                                               && JsPrefix == other.JsPrefix
                                               && Params.SequenceEqual(other.Params);

        public override bool Equals(object? obj) => obj is Signature other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Params.GetHashCode();
                hashCode = (hashCode * 397) ^ JsMember.GetHashCode();
                hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
                hashCode = (hashCode * 397) ^ JsPrefix.GetHashCode();
                return hashCode;
            }
        }
    }

    private struct Param : IEquatable<Param>
    {
        public TypeView Type;
        public string Name;

        public bool Equals(Param other) => Type.Equals(other.Type) && Name == other.Name;

        public override bool Equals(object? obj) => obj is Param other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }
    }

    private struct TypeView : IEquatable<TypeView>
    {
        public static readonly TypeView Void = new()
        {
            FullId = "void",
            IsPublic = true,
        };

        public string FullId;
        public string ShortId;
        public string ContainingParentsPath;
        public bool IsNullable;
        public bool IsPublic;
        public bool IsArray;

        public string AnnotatedFullId => $"{FullId}{(IsNullable ? "?" : "")}";

        public bool Equals(TypeView other) => FullId == other.FullId
                                              && IsNullable == other.IsNullable
                                              && IsPublic == other.IsPublic
                                              && IsArray == other.IsArray;

        public override bool Equals(object? obj) => obj is TypeView other && Equals(other);

        public override int GetHashCode()
        {
            // In .NET 6:
            // return HashCode.Combine(...);
            unchecked
            {
                int hashCode = FullId.GetHashCode();
                hashCode = (hashCode * 397) ^ IsNullable.GetHashCode();
                hashCode = (hashCode * 397) ^ IsPublic.GetHashCode();
                hashCode = (hashCode * 397) ^ IsArray.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => AnnotatedFullId;
    }

#pragma warning restore
}
