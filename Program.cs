using System;
using System.IO;

namespace WpfResourcesBuilder
{
    internal class Program
    {
        private static string _csprojPath;
        private static string _outputPath;
        private static string _namespace;

        public static CsProjReader ProjReader;
        public static AppDefReader DefReader;
        public static Writer Writer;

        private static void Main(string[] args)
        {
            _csprojPath = args[0];
            _outputPath = args[1];
            _namespace = args[2];

            if (!UriParser.IsKnownScheme("pack"))
                _ = new System.Windows.Application();

            Console.WriteLine($"[WRB] Generating resources for {_csprojPath}");

            Writer = new Writer(Path.GetFileNameWithoutExtension(_csprojPath));

            ProjReader = new CsProjReader(_csprojPath);
            var appDefPath = ProjReader.GetAppDefPath();
            if (!File.Exists(appDefPath)) return;

            DefReader = new AppDefReader(appDefPath);

            Writer.StartMainNamespace(_namespace);

            DefReader.GetDictionaries().ForEach(d =>
            {
                Console.WriteLine($"[WRB] Processing {(d.IsExternal ? $"{d.ExternalProjectName}_" : "")}{d.DictionaryName}");
                Writer.Add(d.ToString());
            });

            Writer.EndMainNamespace();

            Writer.Save(_outputPath);

            Console.WriteLine("[WRB] Done!");
        }
    }
}