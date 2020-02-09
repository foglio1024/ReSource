using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace ReSource
{
    public class ExternalDictEntry : DictEntryBase
    {
        public override string FullName => $"{ProjectName}_{DictionaryName}";

        public ExternalDictEntry(string path) : base(path)
        {
            var pathSplit = DictionaryPath.Replace("pack://application:,,,/", "")
                .Split(new[] {";component/"}, StringSplitOptions.RemoveEmptyEntries);

            ProjectName = pathSplit[0];
        }

        public override string Build()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\t// {DictionaryPath}");
            sb.AppendLine($"\tpublic static class {FullName}");
            sb.AppendLine("\t{");

            // assume assemblies are in same folder as executable
            Assembly.LoadFrom(Path.Combine(Program.Reader.AssemblyDir, $"{ProjectName}.dll"));

            var resDict = new ResourceDictionary {Source = new Uri(DictionaryPath, UriKind.Absolute)};

            foreach (var key in resDict.Keys)
            {
                if (!(key is string)) continue;
                var val = resDict[key];
                var resType = val.GetType().Name;
                if (resType == "PathGeometry") resType = "Geometry";

                var ns = val.GetType().FullName.Replace($".{resType}", "");
                if (!Program.Writer.Usings.Contains(ns)) Program.Writer.Usings.Add(ns);
                sb.AppendLine(
                    $"\t\tpublic static {resType} {key} => (({resType})App.Current.FindResource(\"{key}\"));");
            }

            sb.AppendLine("\t}");

            return sb.ToString();
        }
    }
}