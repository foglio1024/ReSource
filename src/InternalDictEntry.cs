using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace ReSource
{
    public class InternalDictEntry : DictEntryBase
    {
        public override string FullName => DictionaryName;

        public InternalDictEntry(string path) : base(path)
        {
        }

        public override string Build()
        {
            var sb = new StringBuilder();

            var dictPath = Path.Combine(Program.ProjDir, DictionaryPath);
            var dictDoc = XDocument.Load(dictPath);

            sb.AppendLine($"\t// {DictionaryPath}");
            sb.AppendLine($"\tpublic static class {DictionaryName}");
            sb.AppendLine("\t{");

            dictDoc.Descendants()
                .Where(x => x.Name.LocalName.ToString() != nameof(ResourceDictionary))
                .ToList()
                .ForEach(x =>
                {
                    var resType = x.Name.LocalName;
                    if (resType == nameof(PathGeometry)) resType = nameof(Geometry);
                    x.Attributes()
                        .ToList()
                        .ForEach(a =>
                        {
                            if (a.Name.LocalName != "Key") return;
                            var resName = a.Value;
                            if (resName.StartsWith("{")) return;
                            var ns = x.Name.Namespace.NamespaceName.Replace("clr-namespace:", "");
                            if (ns.Contains(";")) ns = ns.Split(';')[0];
                            if (!ns.StartsWith("http") && !Program.Writer.Usings.Contains(ns))
                                Program.Writer.Usings.Add(ns);

                            sb.AppendLine(
                                $"\t\tpublic static {resType} {resName} => (({resType})App.Current.FindResource(\"{resName}\"));");
                        });
                });

            sb.AppendLine("\t}");

            return sb.ToString();
        }
    }
}