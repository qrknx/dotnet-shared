# dotnet-shared
Repository for libraries, analyzers and other useful things.

## Repository contents
1. [BlazorJsBindingsGenerator (for .NET 6+ projects and VS 2022+)]

## BlazorJsBindingsGenerator (for .NET 6+ projects and VS 2022+)
Source generator which creates extension methods for `IJSRuntime` to call JS
methods from Blazor WASM application.
Generation is triggered by special attributes which are also added by generator.
For example such source:
```csharp
[JsBindingContext("BlazorBindings")]
[JsBind("someJSMethod1", Params = typeof((int i, double d)), Returns = typeof(int))]
[JsBind("someJSMethod2", Params = typeof((IDisposable d, int)), ResetContext = true]
public static partial class JsRuntimeExtensions {}
```
generates the following extension methods:
```csharp
public static partial class JsRuntimeExtensions
{
    public async Task<int> SomeJSMethod1Async(this IJSRuntime js, int i, double d, CancellationToken token)
    {
        return await js.InvokeAsync<int>("BlazorBindings.someJSMethod1" token, i, d);
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
1. `[JsBindingContext("BlazorBindings")]` is optional.
1. `Params = typeof((IDisposable d, int))`. Here unnamed tuple item is used to
pass a single param in C# tuple literal. Unnamed items do not generate parameters
in extension methods.
1. `ResetContext = true`. Value of `[JsBindingContext(...)]` is ignored.

For more examples explore: [BlazorJsBindingsGenerator.Tests/JsBindingsTestData](BlazorJsBindingsGenerator.Tests/JsBindingsTestData).

**Important!**
In Blazor WASM project template Server project depends on Client project by
default.
It means that after some NuGet package is added to Client project
Server also has access to it.
In case of generators such behaviour is not desirable because generator will
be also launched for Server project and thus produce extra load on IDE.
Update `Client.csproj` with `PrivateAssets="all"` in order to disable
transitive reference of generator in Server project:
```xml
<PackageReference Include="BlazorJsBindingsGenerator" Version="X.X.X" PrivateAssets="all" />
```

