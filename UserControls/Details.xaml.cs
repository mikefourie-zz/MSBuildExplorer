//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="Details.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Build.Execution;

namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;
    using ICSharpCode.AvalonEdit.Folding;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;
    using MSBuildExplorer.DataModel;
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Details
    /// </summary>
    public partial class Details
    {
        public ObservableCollection<MSBuildTarget> TargetsToBuild = new ObservableCollection<MSBuildTarget>();
        private readonly FoldingManager foldingManager;
        private readonly AbstractFoldingStrategy foldingStrategy;
        private readonly FlowDocument flowDoc = new FlowDocument();
        private int indent;
        private string windowTitle;
        private ObservableCollection<MSBuildProperties> properties;
        private ObservableCollection<MSBuildItems> items;
        private ObservableCollection<MSBuildImport> imports;
        private ObservableCollection<MSBuildUsing> usings;
        private ObservableCollection<MSBuildTargetDetails> targetDetails;

        public Details()
        {
            InitializeComponent();
            this.Reset();
            this.treeviewTargets.ItemsSource = this.TargetsToBuild;

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            foldingUpdateTimer.Tick += this.foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            this.foldingStrategy = new XmlFoldingStrategy();
            this.textBoxXml.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            this.foldingManager = FoldingManager.Install(this.textBoxXml.TextArea);
            this.foldingStrategy.UpdateFoldings(this.foldingManager, this.textBoxXml.Document);
        }

        public ObservableCollection<MSBuildUsing> UsingsCollection
        {
            get { return this.usings; }
        }

        public ObservableCollection<MSBuildImport> ImportsCollection
        {
            get { return this.imports; }
        }

        public ObservableCollection<MSBuildProperties> PropertiesCollection
        {
            get { return this.properties; }
        }

        public ObservableCollection<MSBuildItems> ItemsCollection
        {
            get { return this.items; }
        }

        public ObservableCollection<MSBuildTargetDetails> TargetDetailsCollection
        {
            get { return this.targetDetails; }
        }

        public void LogMessage(string message)
        {
            this.TextBoxLog.Text += DateTime.Now + "\t" + message + Environment.NewLine;
        }
        
        public void PopulateAll(MSBuildFile f)
        {
            this.SelectedProject = f;
            this.ClearTargetDetails();
            this.PopulateUsings(f);
            this.PopulateProperties(f);
            this.PopulateItems(f);
            this.PopulateImports(f);
            this.PopulateXml(f);
        }

        public MSBuildFile SelectedProject { get; set; }

        public void PopulateProperties(MSBuildFile f)
        {
            this.properties = new ObservableCollection<MSBuildProperties>();
            if (f != null)
            {
                this.properties = f.Properties;
            }

            this.dataGridProperties.ItemsSource = this.properties;
            foreach (TabItem t in from TabItem t in this.tabControlDetails.Items where t.Header.ToString().StartsWith("Properties") select t)
            {
                t.Header = this.properties.Count == 0 ? "Properties" : "Properties - " + this.properties.Count;
            }
        }

        public void PopulateItems(MSBuildFile f)
        {
            this.items = new ObservableCollection<MSBuildItems>();
            if (f != null)
            {
                this.items = f.Items;
            }

            this.dataGridItems.ItemsSource = this.items;
            foreach (TabItem t in from TabItem t in this.tabControlDetails.Items where t.Header.ToString().StartsWith("Items") select t)
            {
                t.Header = this.items.Count == 0 ? "Items" : "Items - " + this.items.Count;
            }
        }

        public void PopulateUsings(MSBuildFile f)
        {
            this.usings = new ObservableCollection<MSBuildUsing>();
            if (f != null)
            {
                this.usings = f.Usings;
            }

            this.dataGridUsings.ItemsSource = this.usings;
            foreach (TabItem t in from TabItem t in this.tabControlDetails.Items where t.Header.ToString().StartsWith("Usings") select t)
            {
                t.Header = this.usings.Count == 0 ? "Usings" : "Usings - " + this.usings.Count;
            }
        }

        public void PopulateImports(MSBuildFile f)
        {
            this.imports = new ObservableCollection<MSBuildImport>();
            if (f != null)
            {
                this.imports = f.Imports;
            }

            this.dataGridImports.ItemsSource = this.imports;
            foreach (TabItem t in from TabItem t in this.tabControlDetails.Items where t.Header.ToString().StartsWith("Imports") select t)
            {
                t.Header = this.imports.Count == 0 ? "Imports" : "Imports - " + this.imports.Count;
            }
        }

        public void PopulateXml(MSBuildFile f)
        {
            this.textBoxXml.Text = f != null ? f.ProjectFile.Xml.RawXml : string.Empty;
        }

        internal void Reset()
        {
            this.TextBoxPreExecute.Text = Settings.Default.DefaultBootStrapper;
            this.TextBoxParameters.Text = Settings.Default.DefaultParameters;
            this.TargetsToBuild.Clear();
        }

        internal void AddTargetToBuild(object p)
        {
            MSBuildTarget sourceTask = (MSBuildTarget)p;

            if (!this.TargetsToBuild.Contains(sourceTask))
            {
                this.TargetsToBuild.Add(sourceTask);
            }
        }

        internal void LoadTargetSet(TargetSet targetSet)
        {
            this.TextBoxParameters.Text = targetSet.Parameters;
            this.TextBoxPreExecute.Text = targetSet.Strapper;
        }

        internal void Build(Project proj, bool consoleBuild, string targetToBuild, string title = "")
        {
            if (!string.IsNullOrEmpty(title))
            {
                this.windowTitle = title;
            }

            if (string.IsNullOrEmpty(targetToBuild))
            {
                this.windowTitle = new FileInfo(proj.FullPath).Name;
            }

            if (consoleBuild)
            {
                string targets = targetToBuild;
                if (!string.IsNullOrEmpty(targets))
                {
                    targets = "/t:" + targets;
                }
                else if (this.treeviewTargets.Items.Count > 0)
                {
                    targets = (from object target in this.treeviewTargets.Items select target as MSBuildTarget).Aggregate(targets, (current, t) => current + (t.Name + ";"));
                    targets = "/t:" + targets.Remove(targets.LastIndexOf(';'), 1);
                }

                string appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string args = string.Format(CultureInfo.CurrentCulture, "\"{0}\" {1} {2}", proj.FullPath, targets, this.TextBoxParameters.Text);
                using (FileStream fileStream = File.Open(appStartPath + @"\wrap.bat", FileMode.Create))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(@"TITLE " + this.windowTitle);
                        streamWriter.WriteLine(@"ECHO Off");
                        streamWriter.Write(this.TextBoxPreExecute.Text);
                        streamWriter.WriteLine();
                        streamWriter.WriteLine(@"msbuild.exe " + args);
                        if (Settings.Default.PauseConsoleAfterExecution)
                        {
                            streamWriter.WriteLine(@"pause");
                        }
                    }
                }

                // configure the process we need to run
                Process buildwrapper = new Process { StartInfo = { FileName = appStartPath + @"\wrap.bat", UseShellExecute = false, Arguments = args } };
                
                // start the process
                buildwrapper.Start();
            }
            else
            {
                try
                {
                    MSBuildExplorerLogger logger = new MSBuildExplorerLogger { Verbosity = LoggerVerbosity.Diagnostic };
                    logger.OnMessage += this.LogBuildMessage;
                    if (this.treeviewTargets.Items.Count > 0)
                    {
                        string[] targets = new string[this.treeviewTargets.Items.Count];
                        int i = 0;
                        foreach (var target in this.treeviewTargets.Items)
                        {
                            MSBuildTarget t = target as MSBuildTarget;
                            if (t != null)
                            {
                                targets[i] = t.Name;
                            }

                            i++;
                        }

                        System.Collections.Generic.IEnumerable<ILogger> tt = new ILogger[] { logger };
                        proj.Build(targets, tt);
                    }
                    else
                    {
                        proj.Build(logger);
                    }

                    // this.TextBoxOutput.Document = this.flowDoc;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        internal void ClearTargetDetails()
        {
            this.targetDetails = new ObservableCollection<MSBuildTargetDetails>();
            this.dataGridTargetDetails.ItemsSource = this.targetDetails;
        }

        internal void PopulateTargetDetails(MSBuildTarget m)
        {
            this.targetDetails = new ObservableCollection<MSBuildTargetDetails>();
            this.targetDetails.Add(new MSBuildTargetDetails("Name", m.Name));
            this.targetDetails.Add(new MSBuildTargetDetails("Location", m.Location));
            this.targetDetails.Add(new MSBuildTargetDetails("Condition", m.TargetCondition));
            this.targetDetails.Add(new MSBuildTargetDetails("DependsOnTargets", m.DependsOnTargets));
            this.targetDetails.Add(new MSBuildTargetDetails("Inputs", m.Inputs));
            this.targetDetails.Add(new MSBuildTargetDetails("Outputs", m.Outputs));
            this.targetDetails.Add(new MSBuildTargetDetails("BeforeTargets", m.BeforeTargets));
            this.targetDetails.Add(new MSBuildTargetDetails("AfterTargets", m.AfterTargets));
            this.dataGridTargetDetails.ItemsSource = this.targetDetails;
        }

        private void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this.foldingStrategy != null)
            {
                this.foldingStrategy.UpdateFoldings(this.foldingManager, this.textBoxXml.Document);
            }
        }

        private void LogBuildMessage(object sender, EventArgs e)
        {
            ValEventArgs valArgs = (ValEventArgs)e;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                string indentation = string.Empty;
                this.indent = valArgs.Indentation;
                for (int i = this.indent; i > 0; i--)
                {
                    indentation += "\t";
                }

                Paragraph myParagraph1 = new Paragraph(new Run(indentation + valArgs.Message)) { LineHeight = 5 };
                Section mySection = new Section();
                switch (valArgs.Type)
                {
                    case Helper.MessageType.Info:
                        mySection.Foreground = Brushes.White;
                        break;
                    case Helper.MessageType.Error:
                        mySection.Foreground = Brushes.Red;
                        break;
                    case Helper.MessageType.Warning:
                        mySection.Foreground = Brushes.Yellow;
                        break;
                    case Helper.MessageType.Success:
                        mySection.Foreground = Brushes.LawnGreen;
                        break;
                }

                mySection.Blocks.Add(myParagraph1);
                this.flowDoc.Blocks.Add(mySection);
            }));
        }

        private void treeviewTargets_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MSBuildTarget)))
            {
                MSBuildTarget sourceTask = (MSBuildTarget)e.Data.GetData(typeof(MSBuildTarget));

                if (!this.TargetsToBuild.Contains(sourceTask))
                {
                    this.TargetsToBuild.Add(sourceTask);
                }

                this.windowTitle = "MSBuild Explorer Custom Target Run";
            }
        }

        private void treeviewTargets_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MSBuildTarget f = this.treeviewTargets.SelectedItem as MSBuildTarget;
            if (f == null)
            {
                return;
            }

            foreach (MSBuildTarget mf in this.TargetsToBuild.Where(mf => f == mf))
            {
                this.TargetsToBuild.Remove(mf);
                break;
            }
        }

        private void menuRemoveOthers_Click(object sender, RoutedEventArgs e)
        {
            MSBuildTarget f = this.treeviewTargets.SelectedItem as MSBuildTarget;
            if (f == null)
            {
                return;
            }

            ObservableCollection<MSBuildTarget> targetsToBuildTemp = new ObservableCollection<MSBuildTarget>();
            foreach (MSBuildTarget mf in this.TargetsToBuild.Where(mf => f != mf))
            {
                targetsToBuildTemp.Add(mf);
            }

            foreach (MSBuildTarget mf in targetsToBuildTemp)
            {
                this.TargetsToBuild.Remove(mf);
            }
        }

        private void menuRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            this.TargetsToBuild.Clear();
        }

        private void ImageClearTargets_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.TargetsToBuild.Clear();
        }

        private void ImageMoveDown_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MSBuildTarget selectedfile = this.treeviewTargets.SelectedItem as MSBuildTarget;
            if (selectedfile != null)
            {
                int index = this.TargetsToBuild.IndexOf(selectedfile);
                if (index < this.TargetsToBuild.Count - 1)
                {
                    this.TargetsToBuild.Remove(selectedfile);
                    this.TargetsToBuild.Insert(index + 1, selectedfile);
                    selectedfile.IsSelected = true;
                    this.treeviewTargets.Items.Refresh();
                }
            }
        }

        private void ImageMoveUp_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MSBuildTarget selectedfile = this.treeviewTargets.SelectedItem as MSBuildTarget;
            if (selectedfile != null)
            {
                int index = this.TargetsToBuild.IndexOf(selectedfile);
                if (index > 0)
                {
                    this.TargetsToBuild.Remove(selectedfile);
                    this.TargetsToBuild.Insert(index - 1, selectedfile);
                    selectedfile.IsSelected = true;
                    this.treeviewTargets.Items.Refresh();
                }
            }
        }

        private void evaluateParameters(object sender, RoutedEventArgs e)
        {
            if (this.SelectedProject == null)
                return;

            var parameters = TextBoxParameters.Text.Split(new char[] {' '});
            var globalProps = new Dictionary<string, string>();
            foreach (var prop in parameters)
            {
                if (prop.StartsWith("/p:"))
                {
                    var pair = prop.Substring(3).Split('=');
                    if (pair.Length == 2)
                    {
                        globalProps.Add(pair[0], pair[1]);
                    }
                }
            }

            var projInst = new ProjectInstance(this.SelectedProject.ProjectFile.FullPath, globalProps,
                this.SelectedProject.ToolsVersion);
            foreach (var p in this.PropertiesCollection)
            {
                p.EvaluatedValue = projInst.GetPropertyValue(p.Name);
            }

            this.dataGridProperties.ItemsSource = null;
            this.dataGridProperties.ItemsSource = this.PropertiesCollection;
            this.tabControlDetails.SelectedIndex = 1;
        }
    }
}