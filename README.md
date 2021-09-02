# ReSource.CLI
A small C# console application which builds static resource references from ResourceDictionaries in WPF applications. **This allows XAML-defined resources to be referenced in a strongly-typed way in C#**, without using the `FindResource("Name")` method.

## Usage
`
.\ReSource.CLI.exe <source-csproj-path> <main-source-assembly> <destination-cs-file-path> <namespace>
`

The best way to run this is to set it as pre-build command in **project properties** -> **Build events** -> **Pre-build event command line** box, example:
```
ReSource.CLI.exe $(ProjectPath) $(TargetPath) $(ProjectDir)\R.cs $(ProjectName).R
```

or directly in the `.csproj` file:

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="path\\to\\ReSource.CLI.exe $(ProjectPath) $(TargetPath) $(ProjectDir)\\R.cs $(ProjectName).R" />
</Target>
```

## Example
Consider the following `App.xaml` file:
```xml
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="TCC.App">
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
   public class SVG : RH
   {
      public static Geometry SvgClose => Get<Geometry>("SvgClose");
      ...
   }
   public class Colors : RH
   {
      public static Color CardDarkColor => Get<Color>("CardDarkColor");
      ...
   }
   ...
   
   public class RH
   {
      protected static T Get<T>(string res)
      {
          return (T)Application.Current.FindResource(res);
      }
   }
}
```

Resources can then be referenced directly:
```csharp
// + NORMAL WAY
//  - prone to typos
//  - need to know resource type beforehand to properly cast it
//  - can cause runtime exceptions
var res = ((Color)App.Current.FindResource("HpColor"));


// + STRONGLY-TYPED WAY
//  - no typos due to not using string name directly
//  - type already known
//  - runtime exceptions only if the resource file is not re-generated before build (which shouldn't happen)
var res = TCC.R.Colors.HpColor;

```

## Issues
- Resources defined **directly** in the `App.xaml` file are not parsed

# SVG Viewer

![SvgViewer](https://i.imgur.com/9iZs3xW.png)

This WPF application consists of a single window which shows all the `PathGeometry` resources defined in the target assembly and its dependencies. It can be used as a Visual Studio external tool (Tools menu).

Hovering an icon will display its key and the `<Assembly>_<ResourceDictionary>` it's from. Clicking the icon will copy it's key to the clipboard.

## Known issues
Some resource dictionaries might not be parsed due to the following reasons:
- mismatch between SvgViewer's and target's (or dependencies') .NET version
- mismatch between `.csproj` name and output file name


