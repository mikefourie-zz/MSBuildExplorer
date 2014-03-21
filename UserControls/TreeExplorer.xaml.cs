//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeExplorer.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using MSBuildExplorer.DataModel;

    public partial class TreeExplorer
    {
        public static readonly RoutedEvent TargetClick = EventManager.RegisterRoutedEvent("TargetClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent TargetDoubleClick = EventManager.RegisterRoutedEvent("TargetDoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent PopulateEverything = EventManager.RegisterRoutedEvent("PopulateEverything", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent StartExplore = EventManager.RegisterRoutedEvent("StartExplore", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent BuildTarget = EventManager.RegisterRoutedEvent("BuildTarget", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent FinishedExplore = EventManager.RegisterRoutedEvent("FinishedExplore", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));
        public static readonly RoutedEvent FailedExplore = EventManager.RegisterRoutedEvent("FailedExplore", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeExplorer));

        public MSBuildTarget ActiveMSBuildTarget;
        public MSBuildFile RootFile;
        public Exception TreeExeption;
        private readonly ObservableCollection<MSBuildFile> files = new ObservableCollection<MSBuildFile>();

        public TreeExplorer()
        {
            InitializeComponent();
        }

        public void LoadFile(FileInfo file, bool populate)
        {
            if (this.RootFile != null && this.RootFile.ProjectFile.FullPath == file.FullName)
            {
                return;
            }

            RaiseEvent(new RoutedEventArgs(TreeExplorer.StartExplore, this));
            MSBuildFileEqualityComparer eq = new MSBuildFileEqualityComparer();
            try
            {
                this.RootFile = MSBuildHelper.GetFile(file);
            }
            catch (Exception ex)
            {
                this.TreeExeption = ex;
                RaiseEvent(new RoutedEventArgs(TreeExplorer.FailedExplore, this));
                return;
            }

            if (this.files.Contains(this.RootFile, eq))
            {
                int i = 0;
                foreach (MSBuildFile f in this.files)
                {
                    if (f.Name == this.RootFile.Name)
                    {
                        this.files.RemoveAt(i);
                        break;
                    }

                    i++;
                }
            }

            this.files.Add(this.RootFile);
            this.files.BubbleSort();
            this.tvMain.ItemsSource = this.files;
            if (populate)
            {
                RaiseEvent(new RoutedEventArgs(TreeExplorer.PopulateEverything, this));
            }

            this.textBlockTitle.Text = this.tvMain.Items.Count == 0 ? "Explore" : "Explore - " + this.tvMain.Items.Count;
            RaiseEvent(new RoutedEventArgs(TreeExplorer.FinishedExplore, this));
        }

        ////public void RefreshFile(FileInfo file, bool populate)
        ////{
        ////    RaiseEvent(new RoutedEventArgs(TreeExplorer.StartExplore, this));
        ////    try
        ////    {
        ////        //this.RootFile = MSBuildHelper.GetFile(file);
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        this.TreeExeption = ex;
        ////        RaiseEvent(new RoutedEventArgs(TreeExplorer.FailedExplore, this));
        ////        return;
        ////    }

        ////    this.files.BubbleSort();
        ////    this.tvMain.ItemsSource = this.files;
        ////    if (populate)
        ////    {
        ////        RaiseEvent(new RoutedEventArgs(TreeExplorer.PopulateEverything, this));
        ////    }

        ////    RaiseEvent(new RoutedEventArgs(TreeExplorer.FinishedExplore, this));
        ////}

        internal void CloseFile(bool closeAll)
        {
            if (closeAll)
            {
                if (this.files.Count >= 2)
                {
                    if (MessageBox.Show("Close all files?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                this.files.Clear();
            }
            else
            {
                MSBuildFile f = this.tvMain.SelectedItem as MSBuildFile;
                if (f != null)
                {
                    foreach (MSBuildFile mf in this.files)
                    {
                        if (f == mf)
                        {
                            this.files.Remove(mf);
                            break;
                        }
                    }
                }
            }

            this.textBlockTitle.Text = this.tvMain.Items.Count == 0 ? "Explore" : "Explore - " + this.tvMain.Items.Count;
        }

        private void tvMain_Drop(object sender, DragEventArgs e)
        {
            string[] files1 = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files1 != null)
            {
                foreach (var v in files1)
                {
                    FileInfo f = new FileInfo(v);
                    if (f.Exists && f.Extension != ".msbef")
                    {
                        this.LoadFile(f, true);
                    }
                }
            }
        }

        private void tvMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void tvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MSBuildFile f = this.tvMain.SelectedItem as MSBuildFile;
            if (f != null)
            {
                foreach (MSBuildFile mf in this.files)
                {
                    if (f == mf)
                    {
                        this.RootFile = mf;
                        break;
                    }
                }

                RaiseEvent(new RoutedEventArgs(TreeExplorer.PopulateEverything, this));
                return;
            }

            MSBuildTarget t = this.tvMain.SelectedItem as MSBuildTarget;
            if (t != null)
            {
                bool raise = false;
                this.ActiveMSBuildTarget = t;
                foreach (MSBuildFile mf in this.files)
                {
                    if (t.Parent == mf && t.Parent != this.RootFile)
                    {
                        this.RootFile = mf;
                        raise = true;
                        break;
                    }
                }

                if (raise)
                {
                    RaiseEvent(new RoutedEventArgs(TreeExplorer.PopulateEverything, this));
                }
                
                RaiseEvent(new RoutedEventArgs(TreeExplorer.TargetClick, this));
            }

            if (this.files.Count == 0)
            {
                this.RootFile = null;
                RaiseEvent(new RoutedEventArgs(TreeExplorer.PopulateEverything, this));
            }
        }

        private void tvMain_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (tvMain.SelectedValue != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    DragDrop.DoDragDrop(tvMain, tvMain.SelectedValue, DragDropEffects.Move);
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.CloseFile(false);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            this.CloseFile(true);
        }

        private void tvMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject src = (DependencyObject)e.OriginalSource;
            while (!(src is Control))
            {
                src = VisualTreeHelper.GetParent(src);
            }

            if (src.GetType().Name == "TreeViewItem")
            {
                MSBuildTarget f = this.tvMain.SelectedItem as MSBuildTarget;
                if (f != null)
                {
                    this.ActiveMSBuildTarget = f;
                    RaiseEvent(new RoutedEventArgs(TreeExplorer.TargetDoubleClick, this));
                }
            }
        }

        private void tvMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
