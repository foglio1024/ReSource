using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfResourcesBuilder
{
    public class AppDefReader : ReaderBase
    {
        public string FilePath => _path;
        public AppDefReader(string path) : base(path) { }

        public List<DictionaryEntry> GetDictionaries()
        {
            var ret = _doc.Descendants()
                .Where(x =>
                    x.Name.LocalName == "ResourceDictionary" &&
                    x.Attribute("Source") != null)
                .Select(x => new DictionaryEntry(x.Attribute("Source")?.Value))
                .ToList();
            ret.Sort();
            Console.WriteLine($"[WRB] Found {ret.Count} ResourceDictionaries.");
            return ret;
        }


    }
}