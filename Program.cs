using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WpfResourcesBuilder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write($"Generating resources for {args[0]}...");
            var appxamlPath = args[0];
            if (!File.Exists(appxamlPath)) return;
            var f = XDocument.Load(appxamlPath);
            var dictionaries = f.Descendants()
                .Where(x => x.Name.ToString().Contains("ResourceDictionary") && x.Attribute("Source") != null).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("////////////////////////////////////////////////////");
            sb.AppendLine("//// File automatically generated from App.xaml ////");
            sb.AppendLine("////////////////////////////////////////////////////");

            if (File.Exists(args[1]))
            {
                var destLines = File.ReadAllLines(args[1]).ToList();
                destLines.Where(l => l.StartsWith("using")).ToList().ForEach(u => sb.AppendLine(u));
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {args[2]}");
            sb.AppendLine("{");
            dictionaries.ForEach(dict =>
            {
                var dictPath = Path.Combine(Path.GetDirectoryName(appxamlPath), dict.Attribute("Source").Value);
                var dictDoc = XDocument.Load(dictPath);

                var resources = dictDoc.Descendants().Where(x => x.Name.LocalName.ToString() != ("ResourceDictionary")).ToList();

                sb.AppendLine($"\tpublic static class {Path.GetFileNameWithoutExtension(dictPath)}");
                sb.AppendLine("\t{");
                resources.ForEach(x =>
                {
                    var resType = x.Name.ToString().Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    if (resType == "PathGeometry") resType = "Geometry";
                    x.Attributes().ToList().ForEach(a =>
                    {
                        if (a.Name.LocalName != "Key") return;
                        var resName = a.Value;
                        if (resName.StartsWith("{")) return;
                        sb.AppendLine($"\t\tpublic static {resType} {resName} => (({resType})App.Current.FindResource(\"{resName}\"));");
                    });
                });
                sb.AppendLine("\t}");
            });
            sb.AppendLine("}");
            
            File.WriteAllText(args[1], sb.ToString());
            Console.WriteLine(" Done!");
        }
    }
}