using System.Xml.Linq;

namespace WpfResourcesBuilder
{
    public class ReaderBase
    {
        protected readonly string _path;
        protected readonly XDocument _doc;
        public ReaderBase(string path)
        {
            _path = path;
            _doc = XDocument.Load(path);

        }
    }
}