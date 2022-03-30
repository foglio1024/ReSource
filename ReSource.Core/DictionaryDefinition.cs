using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReSource.Core
{
    public class DictionaryDefinition : IComparable
    {
        protected string DictionaryPath { get; }
        protected string DictionaryName { get; set; }

        protected string ProjectName { get; set; }

        public virtual string FullName => DictionaryName;

        protected DictionaryDefinition(string path)
        {
            DictionaryPath = path;
            DictionaryName = Path.GetFileNameWithoutExtension(DictionaryPath);
        }

        public virtual string Build(Writer writer)
        {
            return "";
        }

        public virtual List<GeometryData> GetGeometries()
        {
            return new List<GeometryData>();
        }

        public int CompareTo(object obj)
        {
            var other = (DictionaryDefinition)obj;

            return (this, other) switch
            {
                (ExternalDictionaryDefinition, InternalDictionaryDefinition) => 1,
                (InternalDictionaryDefinition, ExternalDictionaryDefinition) => -1,
                _ => string.Compare(DictionaryName, other.DictionaryName, StringComparison.Ordinal)
            };
        }

        protected string BuildEntry(string key, string resType, int additionalIndent = 0)
        {
            //return $"\t\tpublic static {resType} {key} => ({resType})Application.Current.FindResource(\"{key}\");";
            var tabs = "\t\t";
            Enumerable.Range(0, additionalIndent).ToList().ForEach(x => tabs += "\t");

            return $"{tabs}public static {resType} {key} => Get<{resType}>(\"{key}\");";
        }

        protected string BuildClassHeader(string className, string path, int additionalIndent = 0)
        {
            var tabs = "\t";
            Enumerable.Range(0, additionalIndent).ToList().ForEach(x => tabs += "\t");
            return $"{tabs}// {path}\n" + 
                   $"{tabs}public class {className} : RH\n" +
                   $"{tabs}{{";
        }
    }
}