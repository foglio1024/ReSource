using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace WpfResourcesBuilder
{
    public class DictionaryEntry : IComparable
    {
        public string DictionaryPath { get; }
        public string DictionaryName { get; }
        public string ExternalProjectName { get; }
        public string ExternalDicPath { get; }
        public string NugetPackagePath { get; private set; }
        public string FullExternalDicPath { get; private set; }
        public bool IsExternal { get; }
        public bool IsNugetPackage { get; private set; }

        public DictionaryEntry(string path)
        {
            DictionaryPath = path;
            DictionaryName = Path.GetFileNameWithoutExtension(DictionaryPath);
            IsExternal = DictionaryPath.StartsWith("pack://");

            if (!IsExternal) return;

            var pathSplit = DictionaryPath.Replace("pack://application:,,,/", "")
                .Split(new[] {";component/"}, StringSplitOptions.RemoveEmptyEntries);

            ExternalProjectName = pathSplit[0];
            ExternalDicPath = pathSplit[1];
            DictionaryName = Path.GetFileNameWithoutExtension(ExternalDicPath);
            BuildExternalAssemblyDictPath();
        }


        private void BuildExternalAssemblyDictPath()
        {
            var extProjPath = Program.ProjReader.FindReferencePath(ExternalProjectName);
            if (string.IsNullOrEmpty(extProjPath)) return;

            IsNugetPackage = extProjPath.EndsWith(".dll");

            if (!IsNugetPackage)
            {
                // get root path of extProj
                FullExternalDicPath = Path.Combine(Path.GetDirectoryName(extProjPath), ExternalDicPath);
            }
            else
            {
                NugetPackagePath = extProjPath;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\t// {DictionaryPath}");
            sb.AppendLine($"\tpublic static class {(IsExternal ? $"{ExternalProjectName}_" : "")}{DictionaryName}");
            sb.AppendLine("\t{");



            if (IsNugetPackage)
            {
                Assembly.LoadFrom(NugetPackagePath);

                var resDict = new ResourceDictionary {Source = new Uri(DictionaryPath, UriKind.Absolute)};

                foreach (var key in resDict.Keys)
                {
                    if(!(key is string)) continue;
                    var val = resDict[key];
                    var resType = val.GetType().Name;
                    if (resType == "PathGeometry") resType = "Geometry";

                    var ns = val.GetType().FullName.Replace($".{resType}", "");
                    if (!Program.Writer.Usings.Contains(ns)) Program.Writer.Usings.Add(ns);
                    sb.AppendLine($"\t\tpublic static {resType} {key} => (({resType})App.Current.FindResource(\"{key}\"));");
                }
            }
            else
            {
                var src = IsNugetPackage ? ExternalDicPath : DictionaryPath;

                var dictPath = Path.Combine(Path.GetDirectoryName(Program.DefReader.FilePath), src);
                var dictDoc = XDocument.Load(dictPath);
                var resources = dictDoc.Descendants()
                    .Where(x => x.Name.LocalName.ToString() != "ResourceDictionary")
                    .ToList();

                resources.ForEach(x =>
                {
                    var resType = x.Name.LocalName;
                    if (resType == "PathGeometry") resType = "Geometry";
                    x.Attributes()
                        .ToList()
                        .ForEach(a =>
                        {
                            if (a.Name.LocalName != "Key") return;
                            var resName = a.Value;
                            if (resName.StartsWith("{")) return;
                            var ns = x.Name.Namespace.NamespaceName.Replace("clr-namespace:", "");
                            if (ns.Contains(";")) ns = ns.Split(';')[0];
                            if (!ns.StartsWith("http") && !Program.Writer.Usings.Contains(ns)) Program.Writer.Usings.Add(ns);

                            sb.AppendLine($"\t\tpublic static {resType} {resName} => (({resType})App.Current.FindResource(\"{resName}\"));");

                        });
                });

            }


            sb.AppendLine("\t}");

            return sb.ToString();

        }

        public int CompareTo(object obj)
        {
            var other = (DictionaryEntry) obj;
            if (this.IsExternal && !other.IsExternal) return 1;
            if (!this.IsExternal && other.IsExternal) return -1;
            return (DictionaryName.CompareTo(other.DictionaryName));
        }
    }
}