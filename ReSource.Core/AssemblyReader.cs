using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Xml.Linq;

namespace ReSource.Core
{
    public class AssemblyReader
    {
        private readonly string _projectDir;
        private readonly string _assemblyDir;
        private readonly string _projectName;
        private readonly Assembly _assembly;

        public AssemblyReader(string path, string projectPath)
        {
            _projectDir = Path.GetDirectoryName(projectPath);
            _projectName = Path.GetFileNameWithoutExtension(projectPath);
            _assembly = Assembly.LoadFrom(path);
            _assemblyDir = Path.GetDirectoryName(path);
        }

        public List<DictionaryDefinition> GetDictionaries()
        {
            var ret = new List<DictionaryDefinition>();

            var stream = _assembly.GetManifestResourceStream(_assembly.GetName().Name + ".g.resources");
            if (stream == null) return ret;

            using var reader = new ResourceReader(stream);
            (from System.Collections.DictionaryEntry entry in reader
             where entry.Key.ToString().EndsWith(".baml")
             select entry.Key.ToString().Replace(".baml", ".xaml")).ToList()
            .ForEach(r =>
            {
                var dictPath = Path.Combine(_projectDir, r);
                if (!File.Exists(dictPath)) return;

                var dictDoc = XDocument.Load(dictPath);
                if (dictDoc.Root?.Name.LocalName != nameof(Application)) return;

                ret = dictDoc.Descendants()
                    .Where(x =>
                        x.Name.LocalName == nameof(ResourceDictionary) &&
                        x.Attribute(nameof(ResourceDictionary.Source))?.Value != null)
                    .Select<XElement, DictionaryDefinition>(x =>
                    {
                        var src = x.Attribute(nameof(ResourceDictionary.Source)).Value;
                        return src.StartsWith("pack:")
                            ? new ExternalDictionaryDefinition(src, _assemblyDir)
                            : new InternalDictionaryDefinition(src, _projectDir);
                    }).ToList();
                ret.Sort();

                Console.WriteLine($"[RS] Found {ret.Count} ResourceDictionaries.");
            });

            return ret;
        }
    }
}