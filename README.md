# WpfResourcesBuilder
A small .NET application which parses an `App.xaml` file and builds a static class containing a public static property for each resource defined in the XAML file and referenced resource dictionaries. **This allows XAML-defined resources to be referenced in a strongly-typed way in C#**, without using 
```cs
((ResourceType)App.Current.FindResource("ResourceName"));
```

## Usage
`
.\WpfResourcesBuilder.exe <source-App.xaml-path> <destination-cs-file-path>
`

Best way to run this is to set it as pre-build command in **project properties** -> **Build events** -> **Pre-build event command line** box, example:
```
WpfResourcesBuilder.exe $(ProjectDir)\App.xaml $(ProjectDir)\R.cs
```

## Issues
- Resources defined **directly** in the `App.xaml` file are not parsed
- `using` directives are not generated: this will cause compile errors at the first execution. When this happens the generated file should be opened to add the missing directives. Manually added `usings` will be kept at the next execution of the script.
