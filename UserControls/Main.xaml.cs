//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="Main.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Windows;
    using Microsoft.Win32;
    using MSBuildExplorer.DataModel;
    using MSBuildExplorer.Forms;
    using MSBuildExplorer.Properties;
    using Microsoft.Build.Exceptions;

    /// <summary>
    /// Interaction logic for Main
    /// </summary>
    public partial class Main
    {
        private readonly OpenFileDialog ofd = new OpenFileDialog();

        public Main()
        {
            InitializeComponent();
            var userPrefs = new UserPreferences();
            GridLengthConverter myGridLengthConverter = new GridLengthConverter();
            GridLength gl1 = (GridLength)myGridLengthConverter.ConvertFromString(userPrefs.WindowLeftColumn.ToString());
            this.columnLeft.Width = gl1;

            gl1 = (GridLength)myGridLengthConverter.ConvertFromString(userPrefs.WindowRightColumn.ToString());
            this.columnRight.Width = gl1;

            this.F1.AddHandler(Favorites.TargetSetClick, new RoutedEventHandler(this.TargetSetClickHandler));
            this.F1.AddHandler(Favorites.FavoriteClick, new RoutedEventHandler(this.FavoriteClickEventHandler));
            this.T1.AddHandler(TreeExplorer.PopulateEverything, new RoutedEventHandler(this.PopulateEverythingHandler));
            this.T1.AddHandler(TreeExplorer.TargetDoubleClick, new RoutedEventHandler(this.TargetDoubleClickHandler));
            this.T1.AddHandler(TreeExplorer.TargetClick, new RoutedEventHandler(this.TargetClickHandler));
            this.T1.AddHandler(TreeExplorer.StartExplore, new RoutedEventHandler(this.StartExploreHandler));
            this.T1.AddHandler(TreeExplorer.FinishedExplore, new RoutedEventHandler(this.FinishedExploreHandler));
            this.T1.AddHandler(TreeExplorer.FailedExplore, new RoutedEventHandler(this.FailedExploreHandler));
        }

        public void StartExploreHandler(object sender, RoutedEventArgs e)
        {
            this.LogMessage("Started exploring");
        }

        public void FailedExploreHandler(object sender, RoutedEventArgs e)
        {
            if (this.T1.RootFile != null)
            {
                this.LogMessage("Failed to explore " + this.T1.RootFile.ProjectFile.FullPath + this.T1.TreeExeption);
                MessageBox.Show(
                    string.Format("Failed to explore {0}.\n\n{1}.",
                        this.T1.RootFile.ProjectFile.FullPath,
                        this.T1.TreeExeption.Message),
                    "Explore Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                this.LogMessage("Failed to explore " + this.T1.TreeExeption);
                MessageBox.Show(
                    this.T1.TreeExeption.Message,
                    "Explore Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            if(this.T1.TreeExeption is InvalidProjectFileException)
            {
                var exception = this.T1.TreeExeption as InvalidProjectFileException;
                this.LogMessage("Line " + exception.LineNumber);
            }
        }

        public void FinishedExploreHandler(object sender, RoutedEventArgs e)
        {
            this.LogMessage("Finished exploring - " + this.T1.RootFile.ProjectFile.FullPath);
        }

        public void TargetSetClickHandler(object sender, RoutedEventArgs e)
        {
            if (this.F1.ActiveTargetSet.file == null)
            {
                MessageBox.Show(
                    "No file is associated with this TargetSet. Please remove this favorite or manually repair it",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            this.T1.LoadFile(new FileInfo(this.F1.ActiveTargetSet.file), true);
            if (this.T1.RootFile != null)
            {
                string[] targets = this.F1.ActiveTargetSet.Targets.Split(
                    new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries);
                this.D1.Reset();
                foreach (string t in targets)
                {
                    foreach (MSBuildTarget m in this.T1.RootFile.Targets)
                    {
                        if (t == m.Name)
                        {
                            this.D1.AddTargetToBuild(m);
                        }
                    }
                }

                this.D1.LoadTargetSet(this.F1.ActiveTargetSet);
            }
        }

        public void FavoriteClickEventHandler(object sender, RoutedEventArgs e)
        {
            this.T1.LoadFile(new FileInfo(this.F1.ActiveFavoriteFile.ProjectFile), true);
            this.D1.Reset();
        }

        public void TargetDoubleClickHandler(object sender, RoutedEventArgs e)
        {
            MSBuildTarget m = this.T1.ActiveMSBuildTarget;
            this.D1.AddTargetToBuild(m);
        }

        public void TargetClickHandler(object sender, RoutedEventArgs e)
        {
            MSBuildTarget m = this.T1.ActiveMSBuildTarget;
            this.D1.PopulateTargetDetails(m);
        }

        public void PopulateEverythingHandler(object sender, RoutedEventArgs e)
        {
            this.D1.PopulateAll(this.T1.RootFile);
            this.D1.Reset();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow == null)
            {
                return;
            }

            if (this.T1.RootFile == null)
            {
                parentWindow.Title = "MSBuild Explorer 3";
            }
            else
            {
                parentWindow.Title = "MSBuild Explorer 3 - " + this.T1.RootFile.ProjectFile.FullPath;
            }
        }

        public void Reload()
        {
            this.T1.LoadFile(new FileInfo(this.T1.RootFile.ProjectFile.FullPath), true);
        }

        public void Build()
        {
            if (this.T1.RootFile != null)
            {
                this.D1.Build(this.T1.RootFile.ProjectFile, true, string.Empty);
            }
        }

        public void SaveFavorite()
        {
            if (D1.TargetsToBuild.Count > 0)
            {
                FavWin f = new FavWin(
                    D1.TargetsToBuild,
                    D1.TextBoxParameters.Text,
                    D1.TextBoxPreExecute.Text,
                    this.T1.RootFile.ProjectFile.FullPath);
                f.ShowDialog();
                this.F1.LoadFavorites();
            }
        }

        public void MenuOpen()
        {
            bool updatedFav = false;

            this.ofd.Title = "Open";
            this.ofd.Multiselect = true;
            this.ofd.RestoreDirectory = true;
            this.ofd.Filter = "MSBuild Files|*.msbuild;*.proj;*.properties;*.targets;*.tasks;*.csproj;*.vbproj;*.vcxproj;*.msbef|All files|*.*";
            this.ofd.FilterIndex = 0;
            this.ofd.ShowDialog();
            foreach (var s in this.ofd.FileNames)
            {
                FileInfo f = new FileInfo(s);
                if (f.Exists && f.Extension == ".msbef")
                {
                    StringCollection favs = Settings.Default.Favorites;
                    if (!favs.Contains(f.FullName))
                    {
                        favs.Add(f.FullName);
                    }

                    Settings.Default.Save();
                    updatedFav = true;
                }
                else if (f.Exists && string.IsNullOrEmpty(s) == false)
                {
                    this.T1.LoadFile(new FileInfo(s), true);
                }
            }

            if (updatedFav)
            {
                this.F1.LoadFavorites();
            }
        }

        private void LogMessage(string message)
        {
            this.D1.LogMessage(message);
        }
    }
}
