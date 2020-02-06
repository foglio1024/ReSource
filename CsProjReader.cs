using System.IO;
using System.Linq;

namespace WpfResourcesBuilder
{
    public class CsProjReader : ReaderBase
    {

        public CsProjReader(string path) : base(path) { }

        public string GetAppDefPath()
        {
            var appDef = _doc.Descendants()
                .Single(x =>
                    x.Name.LocalName == "ItemGroup" &&
                    x.Descendants().Any(y => y.Name.LocalName == "ApplicationDefinition"))
                .Descendants().Single(x => x.Name.LocalName == "ApplicationDefinition")
                .Attribute("Include")
                ?.Value;

            return Path.Combine(Path.GetDirectoryName(_path), appDef);
        }

        public string FindReferencePath(string projName)
        {
            var foundPath = "";
            try
            {
                foundPath = _doc.Descendants().Single(x =>
                x.Name.LocalName == "ItemGroup" &&
                x.Descendants().Any(y => y.Name.LocalName == "ProjectReference"))
            .Descendants().Single(x => x.Name.LocalName == "Name" &&
                                       x.Value == projName)?.Attribute("Include")?.Value;
            }
            catch { }
            if (string.IsNullOrEmpty(foundPath))
            {
                try
                {
                    foundPath = _doc.Descendants().Single(x =>
                            x.Name.LocalName == "ItemGroup" &&
                            x.Descendants().Any(y => y.Name.LocalName == "Reference"))
                        .Descendants().Single(x => x.Attribute("Include")?.Value == projName)
                        .Descendants().Single(x => x.Name.LocalName == "HintPath").Value;
                }
                catch
                {
                }

            }

            if (string.IsNullOrEmpty(foundPath)) return "";

            return Path.Combine(Path.GetDirectoryName(_path), foundPath);
        }
    }
}