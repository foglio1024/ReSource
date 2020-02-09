using System;
using System.IO;

namespace ReSource
{
    public class DictEntryBase : IComparable
    {
        protected string DictionaryPath { get; }
        protected string DictionaryName { get; set; }
        protected string ProjectName { get; set; }

        public virtual string FullName => DictionaryName;

        protected DictEntryBase(string path)
        {
            ProjectName = Program.ProjName;
            DictionaryPath = path;
            DictionaryName = Path.GetFileNameWithoutExtension(DictionaryPath);
        }


        public virtual string Build()
        {
            return "";
        }


        public int CompareTo(object obj)
        {
            var other = (DictEntryBase) obj;
            if (this is ExternalDictEntry && !(other is ExternalDictEntry)) return 1;
            if (!(this is ExternalDictEntry) && other is ExternalDictEntry) return -1;
            return string.Compare(DictionaryName, other.DictionaryName, StringComparison.Ordinal);
        }
    }
}