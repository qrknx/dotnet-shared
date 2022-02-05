namespace System.Diagnostics.CodeAnalysis;

internal class NotNullWhenAttribute : Attribute
{
    public readonly bool ReturnValue;

    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
}
