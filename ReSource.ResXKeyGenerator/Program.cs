using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ReSource.ResXKeyGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var inputPath = args[0];
            var outputPath = inputPath.Replace("Designer.cs", "Keys.cs");
            var className = Path.GetFileNameWithoutExtension(inputPath).Replace(".Designer", "");
            var sb = new StringBuilder();

            var lines = File.ReadAllLines(inputPath).ToList();

            var namespaceLine = lines.FirstOrDefault(x => x.Trim().StartsWith("namespace"));



            var propLines = lines.Where(l => l.Trim().StartsWith("public static string ")).ToList();


            sb.AppendLine(namespaceLine);
            sb.AppendLine($"\tpublic class {className}K");
            sb.AppendLine("\t{");

            propLines.ForEach(l =>
            {
                var propName = l.Trim().Split(" ")[3];
                sb.AppendLine($"\t\t{l.Replace("{", "").Trim()} => nameof({propName});");
            });
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            File.WriteAllText(outputPath, sb.ToString());
        }
    }
}
