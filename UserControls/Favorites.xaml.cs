//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="Favorites.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.IO;
    using System.Windows;
    using System.Xml.Serialization;
    using MSBuildExplorer.DataModel;
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Favorites
    /// </summary>
    public partial class Favorites
    {
        public static readonly RoutedEvent FavoriteClick = EventManager.RegisterRoutedEvent("FavoriteClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Favorites));
        public static readonly RoutedEvent TargetSetClick = EventManager.RegisterRoutedEvent("TargetSetClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Favorites));
        public static readonly RoutedEvent TargetSetDoubleClick = EventManager.RegisterRoutedEvent("TargetSetDoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Favorites));
        public FavoriteFile ActiveFavoriteFile;
        public TargetSet ActiveTargetSet;
        private readonly ObservableCollection<FavoriteFile> favoriteFiles = new ObservableCollection<FavoriteFile>();
        private readonly ObservableCollection<FavoriteFileGroup> favoriteFileFroups = new ObservableCollection<FavoriteFileGroup>();

        public Favorites()
        {
            this.InitializeComponent();
            this.LoadFavorites();
        }

        public void LoadFavorites()
        {
            StringCollection favs = new StringCollection();
            if (Settings.Default.Favorites != null)
            {
                this.favoriteFiles.Clear();
                this.favoriteFileFroups.Clear();
                foreach (var s in Settings.Default.Favorites)
                {
                    if (!File.Exists(s))
                    {
                        continue;
                    }

                    try
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(FavoriteEntity));
                        FavoriteEntity foundFavourite;
                        using (FileStream favStream = new FileStream(s, FileMode.Open, FileAccess.Read))
                        {
                            foundFavourite = (FavoriteEntity)deserializer.Deserialize(favStream);
                        }

                        FavoriteFile f = new FavoriteFile(s, foundFavourite);
                        this.favoriteFiles.Add(f);
                        favs.Add(s);
                    }
                    catch (Exception ex)
                    {
                        string inner = string.Empty;
                        if (ex.InnerException != null)
                        {
                            inner = ex.InnerException.Message;
                        }

                        MessageBox.Show(string.Format("There was an error opening the Favorite\n{0}\n\n{1} {2}", s, ex.Message, inner), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                this.ResolveFavoriteGroups();
                this.favoriteFileFroups.BubbleSort();
                this.treeViewFavorites.ItemsSource = this.favoriteFileFroups;

                Settings.Default.Favorites = favs;
                Settings.Default.Save();
            }
        }

        private void ResolveFavoriteGroups()
        {
            FavoriteFileGroup ungrouped = new FavoriteFileGroup("Ungrouped");

            this.favoriteFiles.BubbleSort();

            foreach (FavoriteFile ffile in this.favoriteFiles)
            {
                bool inserted = false;
                foreach (FavoriteFileGroup group in this.favoriteFileFroups)
                {
                    if (string.Compare(group.Name, ffile.GroupName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        group.Files.Add(ffile);
                        inserted = true;
                        break;
                    }
                }

                if (!inserted)
                {
                    if (string.IsNullOrEmpty(ffile.GroupName))
                    {
                        ungrouped.Files.Add(ffile);
                    }
                    else
                    {
                        FavoriteFileGroup nogroup = new FavoriteFileGroup(ffile.GroupName);
                        nogroup.Files.Add(ffile);
                        this.favoriteFileFroups.Add(nogroup);
                    }
                }
            }

            if (ungrouped.Files.Count > 0)
            {
                this.favoriteFileFroups.Add(ungrouped);
            }
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FavoriteFile ff = this.treeViewFavorites.SelectedItem as FavoriteFile;
            if (ff != null)
            {
                StringCollection favs = Settings.Default.Favorites;
                favs.Remove(ff.FullPath);
                Settings.Default.Save();
                this.LoadFavorites();
                return;
            }

            FavoriteFileGroup ffg = this.treeViewFavorites.SelectedItem as FavoriteFileGroup;
            if (ffg != null)
            {
                if (MessageBox.Show("Are you sure you would like to close this whole Favorite group?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                StringCollection favs = Settings.Default.Favorites;
                foreach (FavoriteFile f in ffg.Files)
                {
                    favs.Remove(f.FullPath);
                }

                Settings.Default.Save();
                this.LoadFavorites();
            }
        }

        private void CloseAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you would like to close all favorites?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            StringCollection favs = Settings.Default.Favorites;
            favs.Clear();
            Settings.Default.Save();
            this.LoadFavorites();
        }

        private void treeViewFavorites_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TargetSet ts = this.treeViewFavorites.SelectedItem as TargetSet;
            if (ts != null)
            {
                this.ActiveTargetSet = ts;
                RaiseEvent(new RoutedEventArgs(Favorites.TargetSetClick, this));
                return;
            }

            FavoriteFile ff = this.treeViewFavorites.SelectedItem as FavoriteFile;
            if (ff != null)
            {
                this.ActiveFavoriteFile = ff;
                RaiseEvent(new RoutedEventArgs(Favorites.FavoriteClick, this));
            }
        }

        private void treeViewFavorites_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TargetSet ts = this.treeViewFavorites.SelectedItem as TargetSet;
            if (ts != null)
            {
                this.ActiveTargetSet = ts;
                RaiseEvent(new RoutedEventArgs(Favorites.TargetSetClick, this));
                return;
            }

            FavoriteFile ff = this.treeViewFavorites.SelectedItem as FavoriteFile;
            if (ff != null)
            {
                this.ActiveFavoriteFile = ff;
                RaiseEvent(new RoutedEventArgs(Favorites.FavoriteClick, this));
            }
        }

        private void treeViewFavorites_Drop(object sender, DragEventArgs e)
        {
            string[] files1 = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files1 != null)
            {
                foreach (var v in files1)
                {
                    FileInfo f = new FileInfo(v);
                    if (f.Exists && f.Extension == ".msbef")
                    {
                        StringCollection favs = Settings.Default.Favorites;
                        favs.Add(f.FullName);
                        Settings.Default.Save();
                    }
                }

                this.LoadFavorites();
            }
        }

        private void treeViewFavorites_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
    }
}
