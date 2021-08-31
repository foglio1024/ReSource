using Nostrum.WPF.Extensions;
using ReSource.SvgViewer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ReSource.SvgViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TryDragMove();
        }
    }
}