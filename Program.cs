using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WpfResourcesBuilder
{
    internal class Program
    {
        private static XDocument _xCsProj;
        private static string _csprojPath;
        private static string _outputPath;
        private static string _namespace;
        private static void Main(string[] args)
        {
            _csprojPath = args[0];
            _outputPath = args[1];
            _namespace = args[2];

            Console.Write($"Generating resources for {_csprojPath}...");

            _xCsProj = XDocument.Load(_csprojPath);

            var appxamlPath = GetAppXamlPathFromCsproj(_csprojPath);
            if (!File.Exists(appxamlPath)) return;
            var f = XDocument.Load(appxamlPath);
            var dictionaries = f.Descendants()
                .Where(x => x.Name.ToString().Contains("ResourceDictionary") && x.Attribute("Source") != null).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("///////////////////////////////////////////////////////");
            sb.AppendLine("//// File automatically generated from csproj file ////");
            sb.AppendLine("///////////////////////////////////////////////////////");

            if (File.Exists(_outputPath))
            {
                var destLines = File.ReadAllLines(_outputPath).ToList();
                destLines.Where(l => l.StartsWith("using")).ToList().ForEach(u => sb.AppendLine(u));
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {_namespace}");
            sb.AppendLine("{");
            dictionaries.ForEach(dict =>
            {
                var src = dict.Attribute("Source").Value;
                var external = false;
                var prName = "";
                if (src.StartsWith("pack"))
                {
                    external = true;
                    prName = GetExtProjName(src);
                    src = BuildExternalAssemblyDictPath(src);
                }
                var dictPath = Path.Combine(Path.GetDirectoryName(appxamlPath), src);
                var dictDoc = XDocument.Load(dictPath);

                var resources = dictDoc.Descendants().Where(x => x.Name.LocalName.ToString() != "ResourceDictionary")
                    .ToList();

                sb.AppendLine($"\tpublic static class {(external ? $"{prName}_" : "")}{Path.GetFileNameWithoutExtension(dictPath)}");
                sb.AppendLine("\t{");
                resources.ForEach(x =>
                {
                    var resType = x.Name.ToString().Split(new[] {'{', '}'}, StringSplitOptions.RemoveEmptyEntries)[1];
                    if (resType == "PathGeometry") resType = "Geometry";
                    x.Attributes().ToList().ForEach(a =>
                    {
                        if (a.Name.LocalName != "Key") return;
                        var resName = a.Value;
                        if (resName.StartsWith("{")) return;
                        sb.AppendLine(
                            $"\t\tpublic static {resType} {resName} => (({resType})App.Current.FindResource(\"{resName}\"));");
                    });
                });
                sb.AppendLine("\t}");
            });
            sb.AppendLine("}");

            File.WriteAllText(_outputPath, sb.ToString());
            Console.WriteLine(" Done!");
        }

        private static string GetExtProjName(string src)
        {
            var split = src.Replace("pack://application:,,,/", "")
                .Split(new[] {";component/"}, StringSplitOptions.RemoveEmptyEntries);
            return split[0];
        }

        private static string BuildExternalAssemblyDictPath(string src)
        {
            var split = src.Replace("pack://application:,,,/", "")
                .Split(new[] {";component/"}, StringSplitOptions.RemoveEmptyEntries);
            var projName = split[0];
            var path = split[1];

            var extProjPath = Path.Combine(Path.GetDirectoryName(_csprojPath), FindProjectReferencePath());

            // get root path of extProj

            var dictPath = Path.Combine(Path.GetDirectoryName(extProjPath), path);

            return dictPath;

            string FindProjectReferencePath()
            {
                var xProjRefsGroup = _xCsProj.Descendants()
                    .Single(x =>
                        x.Name.LocalName == "ItemGroup" &&
                        x.Descendants().Any<XElement>(y => y.Name.LocalName == "ProjectReference"));

                var xProjRef = xProjRefsGroup.Descendants().FirstOrDefault(x =>
                    x.Descendants().Any(y => y.Name.LocalName == "Name" && y.Value == projName));

                return xProjRef.Attribute("Include").Value;
            }
        }

        private static string GetAppXamlPathFromCsproj(string csprojPath)
        {
            var itemGroups = _xCsProj.Descendants().Where(d => d.Name.LocalName == "ItemGroup");
            var xAppDefGrp = itemGroups.Single(dd => dd.Descendants().Any(ddd => ddd.Name.LocalName == "ApplicationDefinition"));
            var xAppDef = xAppDefGrp.Descendants().Single(x => x.Name.LocalName == "ApplicationDefinition");
            var appDef = xAppDef.Attribute("Include").Value;

            var appDefPath = Path.Combine(Path.GetDirectoryName(csprojPath), appDef);

            return appDefPath;
        }
    }
}