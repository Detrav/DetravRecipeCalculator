using CommunityToolkit.Mvvm.ComponentModel;
using DetravRecipeCalculator.Utils;
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

            Icons.CollectionChanged += Icons_CollectionChanged;
            ReloadIcons();

        }

        private void Icons_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshFilters();
        }

        internal void ReloadIcons()
        {

            Icons.CollectionChanged -= Icons_CollectionChanged;
            Icons.Clear();
            try
            {

                Utils.Config.Instance.CreateAppDataDirectoryIfNotExists(); ;
                var path = Path.GetFullPath(Utils.Config.Instance.AppDataDirectory);
                var len = path.Length + 1;

                foreach (var file in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
                {
                    var filename = Path.GetFileNameWithoutExtension(file);
                    var category = Path.GetFileName(Path.GetDirectoryName(file));

                    var icon = new SelectIconItemVM();
                    icon.Path = file;
                    icon.Name = category + " " + filename;

                    Icons.Add(icon);
                }
            }
            finally
            {
                Icons.CollectionChanged += Icons_CollectionChanged;
                RefreshFilters();
            }
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
