using Nostrum.WPF;
using Nostrum.WPF.ThreadSafe;
using ReSource.Core;
using System.Windows;
using System.Windows.Input;

namespace ReSource.SvgViewer.ViewModels
{
    public class GeometryViewModel : ThreadSafePropertyChanged
    {
        public GeometryData Data { get; }

        public ICommand CopyCommand { get; }

        public GeometryViewModel(GeometryData data)
        {
            Data = data;

            CopyCommand = new RelayCommand(() =>
            {
                Clipboard.SetText(Data.Name);
            });
        }
    }
}