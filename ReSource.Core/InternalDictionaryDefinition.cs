using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace ReSource.Core
{
    public class InternalDictionaryDefinition : DictionaryDefinition
    {
        private readonly string _projectDir;

        public override string FullName => DictionaryName;

        public InternalDictionaryDefinition(string path, string projectDir) : base(path)
        {
            _projectDir = projectDir;
        }

        public override List<GeometryData> GetGeometries()
        {
            var ret = new List<GeometryData>();

            var dictDoc = LoadDocument();

            dictDoc.Descendants()
                .Where(x => x.Name.LocalName.ToString() == nameof(StreamGeometry) || x.Name.LocalName.ToString() == nameof(PathGeometry))
                .ToList()
                .ForEach(x =>
                {
                    var geometry = new PathGeometry
                    {
                        Figures = (PathFigureCollection)new PathFigureCollectionConverter().ConvertFromString(x.Value)
                    };
                    ret.Add(new GeometryData { Geometry = geometry, Name = x.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key").Value, Source = FullName });
                });

            return ret;
        }

        private XDocument LoadDocument()
        {
            var dictPath = Path.Combine(_projectDir, DictionaryPath);
            return XDocument.Load(dictPath);
        }

        public override string Build(Writer writer)
        {
            var sb = new StringBuilder();

            var dictDoc = LoadDocument();

            sb.AppendLine(BuildClassHeader(DictionaryName, DictionaryPath));

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

                            var key = a.Value;
                            if (key.StartsWith("{")) return;

                            var ns = x.Name.Namespace.NamespaceName.Replace("clr-namespace:", "");
                            if (ns.Contains(";")) ns = ns.Split(';')[0];
                            if (!ns.StartsWith("http")) writer.AddUsing(ns);

                            sb.AppendLine(BuildEntry(key,resType));
                        });
                });

            sb.AppendLine("\t}");

            return sb.ToString();
        }
    }
}