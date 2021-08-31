using System;
using System.IO;
using System.Windows;
using ReSource.Core;

namespace ReSource
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var _csprojPath = args[0];
            if (!File.Exists(_csprojPath))
            {
                Console.WriteLine($"[RS] Error: {_csprojPath} not found");
                Environment.Exit(-1);
            }

            var _assemblyPath = args[1];
            if (!File.Exists(_assemblyPath))
            {
                Console.WriteLine($"[RS] Warning: {_assemblyPath} not found, skipping.");
                Environment.Exit(0);
            }

            var _outputPath = args[2];
            var _namespace = args[3];

            if (!UriParser.IsKnownScheme("pack"))
                _ = new Application();

            Console.WriteLine($"[RS] Generating resources for {_csprojPath}");

            var writer = new Writer(Path.GetFileNameWithoutExtension(_csprojPath));

            writer.StartMainNamespace(_namespace);

            new AssemblyReader(_assemblyPath, _csprojPath)
            .GetDictionaries().ForEach(d =>
            {
                Console.WriteLine($"[RS] Processing {d.FullName}");
                writer.Add(d.Build(writer));
            });

            writer.EndMainNamespace();

            writer.Save(_outputPath);

            Console.WriteLine("[RS] Done!");
        }
    }
}