using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReSource.Core
{
    public class Writer
    {
        private readonly StringBuilder _headerSb;
        private readonly StringBuilder _contentSb;

        public List<string> Usings { get; }

        public Writer(string projectName)
        {
            _headerSb = new StringBuilder();
            _contentSb = new StringBuilder();
            Usings = new List<string>();

            WriteHeader(projectName);
        }

        private void WriteHeader(string projectName)
        {
            var mainLine = $"//// File automatically generated from {projectName}.csproj ////";
            var decoLine = "";
            for (var i = 0; i < mainLine.Length; i++)
            {
                decoLine += "/";
            }
            _headerSb.AppendLine(decoLine);
            _headerSb.AppendLine(mainLine);
            _headerSb.AppendLine(decoLine);
            _headerSb.AppendLine();
        }

        public void StartMainNamespace(string ns)
        {
            _contentSb.AppendLine($"namespace {ns}");
            _contentSb.AppendLine("{");

        }

        private void AddHelperClass()
        {
            _contentSb.AppendLine("\tpublic class RH");
            _contentSb.AppendLine("\t{");
            _contentSb.AppendLine("\t\tprotected static T Get<T>(string res)");
            _contentSb.AppendLine("\t\t{");
            _contentSb.AppendLine("\t\t\treturn (T)Application.Current.FindResource(res);");
            _contentSb.AppendLine("\t\t}");
            _contentSb.AppendLine("\t}");
        }

        public void EndMainNamespace()
        {
            AddHelperClass();

            _contentSb.AppendLine("}");
        }

        public void Save(string outputPath)
        {
            Console.WriteLine($"[RS] Adding {Usings.Count} usings");
            Usings.Sort();
            Usings.ForEach(ns => _headerSb.AppendLine($"using {ns};"));
            _headerSb.AppendLine();
            _headerSb.Append(_contentSb);

            Console.WriteLine($"[RS] Saving resources to {outputPath}");
            File.WriteAllText(outputPath, _headerSb.ToString());
        }

        public void Add(string dict)
        {
            _contentSb.AppendLine(dict);
        }

        internal void AddUsing(string ns)
        {
            if (!Usings.Contains(ns)) Usings.Add(ns);
        }
    }
}