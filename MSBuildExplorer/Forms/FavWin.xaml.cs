//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="FavWin.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.Forms
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Xml.Serialization;
    using Microsoft.Win32;
    using MSBuildExplorer.DataModel;
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Interaction logic for Favorite
    /// </summary>
    public partial class FavWin
    {
        private readonly ObservableCollection<FavoriteFile> favoriteFiles = new ObservableCollection<FavoriteFile>();
        private readonly bool guessName;
        private readonly string favTargets;
        private readonly string favParameters;
        private readonly string favStrapper;
        private readonly string favFileName;

        public FavWin(ObservableCollection<MSBuildTarget> targetsToBuild, string parameters, string strapper, string fileName)
        {
            this.favTargets = targetsToBuild.Aggregate(this.favTargets, (current, target) => current + (target.Name + ";"));
            this.favTargets = this.favTargets.Remove(this.favTargets.LastIndexOf(';'), 1);
            if (targetsToBuild.Count == 1)
            {
                this.guessName = true;
            }

            this.InitializeComponent();
            this.listBoxExistingFavorites.ItemsSource = this.favoriteFiles;
            this.LoadFavorites();
            this.favParameters = parameters;
            this.favStrapper = strapper;
            this.favFileName = fileName;
        }

        public void LoadFavorites()
        {
            if (Settings.Default.Favorites != null)
            {
                this.favoriteFiles.Clear();
                foreach (var s in Settings.Default.Favorites)
                {
                    if (!File.Exists(s))
                    {
                        continue;
                    }

                    try
                    {
                        XmlSerializer deserializer = new XmlSerializer(typeof(FavoriteEntity));
                        FavoriteEntity foundFavorite;
                        using (FileStream favStream = new FileStream(s, FileMode.Open, FileAccess.Read))
                        {
                            foundFavorite = (FavoriteEntity)deserializer.Deserialize(favStream);
                        }

                        FavoriteFile f = new FavoriteFile(s, foundFavorite);
                        this.favoriteFiles.Add(f);
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.guessName)
            {
                this.txtTargetSetName.Text = this.favTargets;
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            TargetSet tset = new TargetSet { file = this.favFileName, Name = this.txtTargetSetName.Text, Parameters = this.favParameters, Strapper = this.favStrapper, Targets = this.favTargets };

            if (this.listBoxExistingFavorites.SelectedIndex >= 0)
            {
                FavoriteFile f = this.listBoxExistingFavorites.SelectedItem as FavoriteFile;
                XmlSerializer deserializer = new XmlSerializer(typeof(FavoriteEntity));
                FavoriteEntity foundFavorite;
                using (FileStream favStream = new FileStream(f.FullPath, FileMode.Open, FileAccess.Read))
                {
                    foundFavorite = (FavoriteEntity)deserializer.Deserialize(favStream);
                }

                TargetSet[] temp = new TargetSet[foundFavorite.TargetSet.Length + 1];
                int i = 0;
                foreach (TargetSet t in foundFavorite.TargetSet)
                {
                    temp[i] = t;
                    i++;
                }

                temp[i] = tset;
                foundFavorite.TargetSet = temp;

                XmlSerializer serializer = new XmlSerializer(typeof(FavoriteEntity));
                using (FileStream fs = new FileStream(f.FullPath, FileMode.Create))
                {
                    TextWriter writer = new StreamWriter(fs, new UTF8Encoding());
                    serializer.Serialize(writer, foundFavorite);
                }

                this.Close();
                return;
            }

            SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog { DefaultExt = ".msbef", Filter = "MSBuild Explorer Favorite (.msbef)|*.msbef" };
            if (dlg.ShowDialog() == true)
            {
                FavoriteEntity fe = new FavoriteEntity();
                fe.groupName = this.txtFavoriteGroupName.Text;
                TargetSet[] tsetcol = new TargetSet[1];
                tsetcol[0] = tset;
                fe.TargetSet = tsetcol;
                fe.file = this.favFileName;
                fe.friendlyName = this.txtFavoriteName.Text;

                XmlSerializer serializer = new XmlSerializer(typeof(FavoriteEntity));
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                {
                    TextWriter writer = new StreamWriter(fs, new UTF8Encoding());
                    serializer.Serialize(writer, fe);
                }

                StringCollection favs = new StringCollection();
                foreach (string s in from string s in Settings.Default.Favorites where !favs.Contains(s) select s)
                {
                    favs.Add(s);
                }

                if (!favs.Contains(dlg.FileName))
                {
                    favs.Add(dlg.FileName);
                }

                Settings.Default.Favorites = favs;

                Settings.Default.Save();
                
                this.Close();
            }
        }
    }
}
