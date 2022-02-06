# dotnet-shared
Repository for libraries, analyzers and other useful things.

## Repository contents
1. [dotnet-shared](#dotnet-shared)
   1. [Repository contents](#repository-contents)
   1. [Projects](#projects)
      1. [BlazorJsBindingsGenerator (for .NET 6+ projects and VS 2022+)](#blazorjsbindingsgenerator-for-net-6-projects-and-vs-2022)

## Projects
### BlazorJsBindingsGenerator (for .NET 6+ projects and VS 2022+)
*Install NuGet from https://github.com/qrknx/dotnet-shared/packages/1236876*

Creates extension methods for `IJSRuntime` to call JS methods from Blazor WASM
application.
Generation is triggered by special attributes which are also added by generator.
For example such source:
```csharp
[JsBindingContext(JsPrefix = "BlazorBindings")]
[JsBind("someJSMethod1", Params = typeof((int i, double d)), Returns = typeof(int))]
[JsBind("someJSMethod2", Params = typeof((IDisposable d, int)), ResetJsPrefix = true)]
public static partial class JsRuntimeExtensions {}
```
generates the following extension methods:
```csharp
public static partial class JsRuntimeExtensions
{
    public async Task<int> SomeJSMethod1Async(this IJSRuntime js, int i, double d, CancellationToken token)
    {
        return await js.InvokeAsync<int>("BlazorBindings.someJSMethod1", token, i, d);
    }

    public async Task SomeJSMethod2Async(this IJSRuntime js, global::System.IDisposable d, CancellationToken token)
    {
        await js.InvokeVoidAsync("someJSMethod2", token, d);
    }
}
```
Some notes:
1. A class marked with one or more `JsBind` attributes:
   - should be defined as static partial,
   - should be non-generic,
   - shouldn't be nested in some other type.
1. `[JsBindingContext(JsPrefix = "BlazorBindings")]` is optional.
JsPrefix denotes common JS path for the following `[JsBind]` attributes.
`[JsBindingContext]` attribute can be applied several times in source file (see
[NamingConventions_WithContext](BlazorJsBindingsGenerator.Tests/JsBindingsTestData/NamingConventions_WithContext/Source.cs) test).
1. `Params = typeof((IDisposable d, int))`.
Here unnamed tuple item is used to pass a single param in C# tuple literal.
Unnamed items do not generate parameters in extension methods.
1. `ResetJsPrefix = true`.
Value of `[JsBindingContext(JsPrefix = ...)]` is ignored.

For more examples explore: [BlazorJsBindingsGenerator.Tests/JsBindingsTestData](BlazorJsBindingsGenerator.Tests/JsBindingsTestData).

**Important!**
Server project in Blazor WASM project template depends on Client project by
default.
It means that after some NuGet package is added to Client project
Server also has access to it.
In case of generators such behaviour is not desirable because generator will
be also launched for Server project and thus produce extra load on IDE.
Update package reference in `Client.csproj` with `PrivateAssets="all"` in
order to disable transitive reference of generator in Server project:
```xml
<PackageReference Include="BlazorJsBindingsGenerator" Version="X.X.X" PrivateAssets="all" />
```
