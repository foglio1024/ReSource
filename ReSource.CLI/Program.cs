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
            var csprojPath = args[0];
            if (!File.Exists(csprojPath))
            {
                Console.WriteLine($"[RS] Error: {csprojPath} not found");
                Environment.Exit(-1);
            }

            var assemblyPath = args[1];
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"[RS] Warning: {assemblyPath} not found, skipping.");
                Environment.Exit(0);
            }

            var outputPath = args[2];
            var @namespace = args[3];

            if (!UriParser.IsKnownScheme("pack"))
                _ = new Application();

            Console.WriteLine($"[RS] Generating resources for {csprojPath}");

            var writer = new Writer(Path.GetFileNameWithoutExtension(csprojPath));

            writer.AddExistingUsings(outputPath);

            writer.StartMainNamespace(@namespace);

            new AssemblyReader(assemblyPath, csprojPath)
            .GetDictionaries().ForEach(d =>
            {
                Console.WriteLine($"[RS] Processing {d.FullName}");
                writer.Add(d.Build(writer));
            });

            writer.EndMainNamespace();

            writer.Save(outputPath);

            Console.WriteLine("[RS] Done!");
        }
    }
}