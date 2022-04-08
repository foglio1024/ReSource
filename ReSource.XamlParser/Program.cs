using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

var projPath = args[0];
var projDir = Path.GetDirectoryName(projPath);

var xamls = EnumerateFilesRecursive(projDir).Where(x => x.EndsWith(".xaml"));
foreach (var xaml in xamls)
{
    var xmlDoc = XDocument.Load(xaml);
    if (xmlDoc.Root?.Name.LocalName != nameof(ResourceDictionary)) continue;

    var lines = new List<string>();

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

                    lines.Add(BuildEntry(key, resType));
                });
        });
    lines.Sort();

}



static List<string> EnumerateFilesRecursive(string root)
{
    var files = new List<string>();
    var dirs = new List<string>();

    try { files.AddRange(Directory.EnumerateFiles(root)); } catch { }
    try { dirs.AddRange(Directory.EnumerateDirectories(root)); } catch { }

    files.AddRange(dirs.Where(dir => !dir.Contains("$RECYCLE.BIN") && !dir.Contains("System Volume Information")).SelectMany(dir => EnumerateFilesRecursive(dir)));

    return files;
}