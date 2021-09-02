using System;
using System.Collections.Generic;
using System.IO;

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

        protected string BuildEntry(string key, string resType)
        {
            //return $"\t\tpublic static {resType} {key} => ({resType})Application.Current.FindResource(\"{key}\");";

            return $"\t\tpublic static {resType} {key} => Get<{resType}>(\"{key}\");";
        }

        protected string BuildClassHeader(string className, string path)
        {
            return $"\t// {path}\n" + 
                   $"\tpublic class {className} : RH\n" +
                    "\t{";
        }
    }
}