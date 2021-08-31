﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ReSource.Core
{
    public class ExternalDictionaryDefinition : DictionaryDefinition
    {
        private readonly string _assemblyDir;
        public override string FullName => $"{ProjectName}_{DictionaryName}";

        public ExternalDictionaryDefinition(string path, string assemblyDir) : base(path)
        {
            var pathSplit = DictionaryPath.Replace("pack://application:,,,/", "")
                .Split(new[] { ";component/" }, StringSplitOptions.RemoveEmptyEntries);

            ProjectName = pathSplit[0];
            _assemblyDir = assemblyDir;
        }

        public override List<GeometryData> GetGeometries()
        {
            var ret = new List<GeometryData>();

            var resDict = GetMainResourceDictionary();
            if (resDict != null)
            {

                foreach (var key in resDict.Keys)
                {
                    if (key is not string k) continue;

                    var val = resDict[key];
                    var resType = val.GetType().Name;
                    if (resType != nameof(StreamGeometry)) continue;

                    ret.Add(new GeometryData { Geometry = (Geometry)val, Name = k, Source = FullName });
                }
            }

            return ret;
        }

        public override string Build(Writer writer)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\t// {DictionaryPath}");
            var className = FullName;
            if (className.Contains("."))
            {
                className = className.Replace(".", "_");
            }
            sb.AppendLine($"\tpublic static class {className}");
            sb.AppendLine("\t{");

            var resDict = GetMainResourceDictionary();
            if (resDict != null)
            {
                foreach (var key in resDict.Keys)
                {
                    if (!(key is string)) continue;
                    var val = resDict[key];
                    var resType = val.GetType().Name;
                    if (resType == "PathGeometry") resType = "Geometry";

                    var ns = val.GetType().FullName.Replace($".{resType}", "");
                    if (!writer.Usings.Contains(ns)) writer.Usings.Add(ns);
                    sb.AppendLine(
                        $"\t\tpublic static {resType} {key} => (({resType})App.Current.FindResource(\"{key}\"));");
                }
            }

            sb.AppendLine("\t}");

            return sb.ToString();
        }

        private ResourceDictionary GetMainResourceDictionary()
        {
            // assume assemblies are in same folder as executable
            _ = Assembly.LoadFrom(Path.Combine(_assemblyDir, $"{ProjectName}.dll"));
            try
            {
                return new ResourceDictionary { Source = new Uri(DictionaryPath, UriKind.Absolute) };

            }
            catch
            {
                return null;
            }
        }
    }
}