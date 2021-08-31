using Nostrum.WPF.Factories;
using Nostrum.WPF.ThreadSafe;
using ReSource.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReSource.SvgViewer.ViewModels
{
    public class DictionaryViewModel : ThreadSafePropertyChanged
    {
        public string Name { get; init; }

        public ObservableCollection<GeometryViewModel> Geometries { get; }

        public ICollectionView GeometriesView { get; }

        public DictionaryViewModel()
        {
            Geometries = new ObservableCollection<GeometryViewModel>();

            GeometriesView = CollectionViewFactory.CreateCollectionView(Geometries, sortDescr: new[] {
                new SortDescription($"{nameof(GeometryViewModel.Data)}.{nameof(GeometryData.Source)}", ListSortDirection.Ascending),
                new SortDescription($"{nameof(GeometryViewModel.Data)}.{nameof(GeometryData.Name)}", ListSortDirection.Ascending),
            });
        }
    }
}