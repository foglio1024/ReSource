# WPF Resources Builder
A small C# script which parses an `App.xaml` file and builds a static class containing a public static property for each resource defined in the XAML file and referenced resource dictionaries. **This allows XAML-defined resources to be referenced in a strongly-typed way in C#**, without using the `FindResource("Name")` method.

## Usage
`
.\WpfResourcesBuilder.exe <source-App.xaml-path> <destination-cs-file-path> <namespace>
`

Best way to run this is to set it as pre-build command in **project properties** -> **Build events** -> **Pre-build event command line** box, example:
```
WpfResourcesBuilder.exe $(ProjectDir)\App.xaml $(ProjectDir)\R.cs TCC.R
```

## Example
Consider the following `App.xaml` file:
```xaml
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="TCC.App"
             Startup="OnStartup">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="ResourceDictionaries/SVG.xaml"/>
                <ResourceDictionary Source="ResourceDictionaries/Colors.xaml"/>
                ...
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```
The script will parse it as XML and produce the following output:
```csharp
////////////////////////////////////////////////////
//// File automatically generated from App.xaml ////
////////////////////////////////////////////////////

namespace TCC.R
{
   public static class SVG
   {
      public static Geometry SvgClose => ((Geometry)App.Current.FindResource("SvgClose"));
      ...
   }
   public static class Colors
   {
      public static Color CardDarkColor => ((Color)App.Current.FindResource("CardDarkColor"));
      ...
   }
   ...
}
```

Resources can then be referenced directly:
```csharp
// normal way 
//  - prone to typos
//  - need to know resource type beforehand to properly cast it
//  - can cause runtime exceptions
var res = ((Color)App.Current.FindResource("HpColor"));


// strongly-typed way 
// - no typos due to not using string name directly
// - type already known
// - runtime exceptions only if the resource file is not re-generated before build (which shouldn't happen)
var res = TCC.R.Colors.HpColor;

```

## Issues
- Resources defined **directly** in the `App.xaml` file are not parsed
- `using` directives are not generated: this will cause compile errors at the first execution. When this happens the generated file should be opened to add the missing directives. Manually added `usings` will be kept at the next execution of the script.
- Dictionaries defined in different assemblies may cause errors (not tested)
