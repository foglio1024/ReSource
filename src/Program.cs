using System;
using System.IO;
using System.Windows;

namespace ReSource
{
    internal class Program
    {
        private static string _csprojPath;
        private static string _assemblyPath;
        private static string _outputPath;
        private static string _namespace;

        public static AssemblyReader Reader;

        public static string ProjDir { get; private set; }
        public static string ProjName { get; private set; }

        public static Writer Writer;

        [STAThread]
        private static void Main(string[] args)
        {
            _csprojPath = args[0];
            if (!File.Exists(_csprojPath))
            {
                Console.WriteLine($"[RS] Error: {_csprojPath} not found");
                Environment.Exit(-1);
            }

            _assemblyPath = args[1];
            if (!File.Exists(_assemblyPath))
            {
                Console.WriteLine($"[RS] Warning: {_assemblyPath} not found, skipping.");
                Environment.Exit(0);
            }

            _outputPath = args[2];
            _namespace = args[3];

            ProjDir = Path.GetDirectoryName(_csprojPath);
            ProjName = Path.GetFileNameWithoutExtension(_csprojPath);


            if (!UriParser.IsKnownScheme("pack"))
                _ = new Application();

            Console.WriteLine($"[RS] Generating resources for {_csprojPath}");

            Writer = new Writer(ProjName);
            Reader = new AssemblyReader(_assemblyPath);

            Writer.StartMainNamespace(_namespace);

            Reader.GetDictionaries().ForEach(d =>
            {
                Console.WriteLine($"[RS] Processing {d.FullName}");
                Writer.Add(d.Build());
            });

            Writer.EndMainNamespace();

            Writer.Save(_outputPath);

            Console.WriteLine("[RS] Done!");
        }
    }
}