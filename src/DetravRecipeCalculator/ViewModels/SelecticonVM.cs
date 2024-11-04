using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.ViewModels
{
    public partial class SelectIconVM : ViewModelBase
    {
        public static SelectIconVM Instance { get; } = new SelectIconVM();

        public ObservableCollection<SelectIconItemVM> Icons { get; } = new ObservableCollection<SelectIconItemVM>();
        [ObservableProperty]
        private IEnumerable<SelectIconItemVM>? filteredIcons;

        public XLocItem WindowTitle { get; } = new XLocItem("__SelectIcon_WindowTitle");
        public XLocItem WindowOk { get; } = new XLocItem("__Dialog_BtnOk");
        public XLocItem WindowCancel { get; } = new XLocItem("__Dialog_BtnCancel");
        [ObservableProperty]
        private string? filterForName;

        partial void OnFilterForNameChanged(string? value)
        {
            RefreshFilters();
        }

        private void RefreshFilters()
        {
            if (String.IsNullOrEmpty(FilterForName))
            {
                FilteredIcons = Icons;
            }
            else
            {
                FilteredIcons = Icons.Where(m => m.Name == null || m.Name.Contains(FilterForName, StringComparison.OrdinalIgnoreCase)).ToArray();
            }
        }

        public SelectIconVM()
        {
            var path = Path.GetFullPath("game-icons");
            var len = path.Length + 1;

            foreach (var file in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
            {
                var filename = file.Substring(len);

                filename = filename.Substring(0, filename.Length - 4);
                filename = filename.Replace("/", " ").Replace("\\", " ");

                var icon = new SelectIconItemVM();
                icon.Path = file;
                icon.Name = filename;

                Icons.Add(icon);
            }

            Icons.CollectionChanged += Icons_CollectionChanged;
            RefreshFilters();
        }

        private void Icons_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshFilters();
        }
    }

    public partial class SelectIconItemVM : ViewModelBase
    {
        [ObservableProperty]
        string? name;
        [ObservableProperty]
        string? path;
        [ObservableProperty]
        object? bitmap;
        [ObservableProperty]
        bool isSelected;
    }
}
