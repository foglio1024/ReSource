using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Xml.Linq;

namespace ReSource
{
    public class AssemblyReader
    {
        private Assembly _assembly { get; }
        public string AssemblyDir { get; }

        public AssemblyReader(string path)
        {
            _assembly = Assembly.LoadFrom(path);
            AssemblyDir = Path.GetDirectoryName(path);
        }


        public List<DictEntryBase> GetDictionaries()
        {
            var ret = new List<DictEntryBase>();

            var stream = _assembly.GetManifestResourceStream(_assembly.GetName().Name + ".g.resources");
            if (stream == null) return ret;

            using var reader = new ResourceReader(stream);
            (from DictionaryEntry entry in reader
             where entry.Key.ToString().EndsWith(".baml")
             select entry.Key.ToString().Replace(".baml", ".xaml")).ToList()
            .ForEach(r =>
            {
                var dictPath = Path.Combine(Program.ProjDir, r);
                if (!File.Exists(dictPath)) return;

                var dictDoc = XDocument.Load(dictPath);
                if (dictDoc.Root?.Name.LocalName != nameof(Application)) return;

                ret = dictDoc.Descendants()
                    .Where(x =>
                        x.Name.LocalName == nameof(ResourceDictionary) &&
                        x.Attribute(nameof(ResourceDictionary.Source))?.Value != null)
                    .Select<XElement, DictEntryBase>(x =>
                    {
                        var src = x.Attribute(nameof(ResourceDictionary.Source)).Value;
                        if (src.StartsWith("pack:"))
                            return new ExternalDictEntry(src);
                        else
                            return new InternalDictEntry(src);
                    }).ToList();
                ret.Sort();
                
                Console.WriteLine($"[WRB] Found {ret.Count} ResourceDictionaries.");
            });

            return ret;
        }
    }
}