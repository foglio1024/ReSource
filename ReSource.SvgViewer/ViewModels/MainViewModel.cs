using Nostrum.WPF;
using Nostrum.WPF.Factories;
using Nostrum.WPF.ThreadSafe;
using ReSource.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace ReSource.SvgViewer.ViewModels
{
    public class MainViewModel : ThreadSafePropertyChanged
    {
        public static string Title => "ReSource SVG Viewer - " + Path.GetFileName(App.AssemblyPath);
        
        public ObservableCollection<DictionaryViewModel> Dictionaries { get; }
        
        public ICollectionView DictionariesView { get; }

        public ICommand CloseCommand { get; }

        public MainViewModel()
        {
            Dictionaries = new ObservableCollection<DictionaryViewModel>();

            DictionariesView = CollectionViewFactory.CreateCollectionView(Dictionaries, sortDescr: new[] {
                new SortDescription($"{nameof(DictionaryViewModel.Name)}.{nameof(GeometryData.Source)}", ListSortDirection.Ascending),
                new SortDescription($"{nameof(GeometryViewModel.Data)}.{nameof(GeometryData.Name)}", ListSortDirection.Ascending),
            });

            CloseCommand = new RelayCommand(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            new AssemblyReader(App.AssemblyPath, App.ProjectPath).GetDictionaries().ForEach(d =>
            {
                var dictVm = new DictionaryViewModel { Name = d.FullName };
                d.GetGeometries().ForEach(g => dictVm.Geometries.Add(new GeometryViewModel(g)));
                if (dictVm.Geometries.Count > 0) Dictionaries.Add(dictVm);
            });
        }
    }
}