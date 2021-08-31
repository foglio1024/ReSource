using System.IO;
using System.Windows;

namespace ReSource.SvgViewer
{
    public partial class App : Application
    {
        public static string AssemblyPath { get; private set; }
        public static string ProjectPath { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AssemblyPath = e.Args[0];
            AssemblyPath = AssemblyPath.Replace("/obj/", "/bin/").Replace("\\obj\\", "\\bin\\");
            ProjectPath = e.Args[1];// Path.GetDirectoryName(e.Args[1]);
        }
    }
}