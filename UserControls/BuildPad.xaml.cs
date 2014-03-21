//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildPad.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;
    using ICSharpCode.AvalonEdit.Folding;
    using Microsoft.Build.Evaluation;
    using Microsoft.Win32;
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Interaction logic for BuildPad
    /// </summary>
    public partial class BuildPad
    {
        private readonly FoldingManager foldingManager;
        private readonly AbstractFoldingStrategy foldingStrategy;
        private readonly OpenFileDialog ofd = new OpenFileDialog();
        private Project proj;

        public BuildPad()
        {
            this.InitializeComponent();
            this.TextBoxPreExecute.Text = Settings.Default.DefaultBootStrapper;
            this.TextBoxParameters.Text = Settings.Default.DefaultParameters;

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            foldingUpdateTimer.Tick += this.foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            this.foldingStrategy = new XmlFoldingStrategy();
            this.textBoxXml.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            this.foldingManager = FoldingManager.Install(this.textBoxXml.TextArea);
            this.foldingStrategy.UpdateFoldings(this.foldingManager, this.textBoxXml.Document);

            if (string.IsNullOrWhiteSpace(this.textBoxXml.Text))
            {
                this.textBoxXml.Text = Settings.Default.DefaultBuildPad;
            }
        }

        public void MenuOpen()
        {
            this.ofd.Title = "Open";
            this.ofd.RestoreDirectory = true;
            this.ofd.Filter = "MSBuild Files|*.msbuild;*.proj;*.properties;*.targets;*.tasks;*.csproj;*.vbproj;*.vcxproj|All files|*.*";
            this.ofd.FilterIndex = 0;
            this.ofd.ShowDialog();
            foreach (var s in this.ofd.FileNames)
            {
                FileInfo f = new FileInfo(s);
                if (f.Exists && string.IsNullOrEmpty(s) == false)
                {
                    using (StreamReader sr = f.OpenText())
                    {
                        this.textBoxXml.Text = sr.ReadToEnd();
                    }
                }
            }
        }

        public void BuildFile()
        {
            this.ConsoleBuild();
        }

        private void ConsoleBuild()
        {
            if (string.IsNullOrEmpty(this.textBoxXml.Text))
            {
                return;
            }

            string appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            using (FileStream fileStream = File.Open(appStartPath + @"\temp.proj", FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(this.textBoxXml.Text);
                }
            }

            this.LoadFile(new FileInfo(appStartPath + @"\temp.proj"));
            string args = string.Format(CultureInfo.CurrentCulture, "\"{0}\" {1}", this.proj.FullPath, this.TextBoxParameters.Text);
            using (FileStream fileStream = File.Open(appStartPath + @"\wrap.bat", FileMode.Create))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8, 512))
                {
                    streamWriter.WriteLine(@"TITLE BuildPad");
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
        
        private void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this.foldingStrategy != null)
            {
                this.foldingStrategy.UpdateFoldings(this.foldingManager, this.textBoxXml.Document);
            }
        }

        private void LoadFile(FileInfo file)
        {
            try
            {
                using (ProjectCollection loadedProjects = new ProjectCollection())
                {
                    this.proj = loadedProjects.GetLoadedProjects(file.FullName).FirstOrDefault();

                    if (this.proj != null)
                    {
                        loadedProjects.UnloadProject(this.proj);
                    }

                    loadedProjects.LoadProject(file.FullName);
                    this.proj = loadedProjects.GetLoadedProjects(file.FullName).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Parsing Text", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
