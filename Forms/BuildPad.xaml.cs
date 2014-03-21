//--------------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildPad.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;
    using Microsoft.Win32;
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Interaction logic for BuildPad
    /// </summary>
    public partial class BuildPad
    {
        private readonly OpenFileDialog ofd = new OpenFileDialog();
        private Project proj;
        private FlowDocument flowDoc;
        private int indent;

        public BuildPad()
        {
            this.InitializeComponent();
            this.TextBoxPreExecute.Text = Settings.Default.DefaultBootStrapper;
            this.TextBoxParameters.Text = Settings.Default.DefaultParameters;
        }

        private void menuFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = e.Source as MenuItem;
            switch (mi.Name)
            {
                case "menuOpen":
                    this.ofd.Title = "Open";
                    this.ofd.Multiselect = false;
                    this.ofd.RestoreDirectory = true;
                    this.ofd.Filter = "MSBuild Files|*.msbuild;*.proj;*.properties;*.targets;*.tasks;*.csproj;*.vbproj;*.vcxproj|All files|*.*";
                    this.ofd.FilterIndex = 0;
                    this.ofd.ShowDialog();

                    this.LoadFile(new FileInfo(this.ofd.FileName));

                    break;
                case "menuClose":
                    this.Close();
                    break;
            }
        }

        private void LoadFile(FileInfo file)
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

            this.textBoxXml.Text = this.proj.Xml.RawXml;
        }

        private void menuConsoleBuild_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxXml.Text))
            {
                return;
            }

            string appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string args = string.Format(CultureInfo.CurrentCulture, "\"{0}\" {1}", this.proj.FullPath, this.TextBoxParameters.Text);
            using (FileStream fileStream = File.Open(appStartPath + @"\wrap.bat", FileMode.Create))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);

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

            // configure the process we need to run
            Process buildwrapper = new Process { StartInfo = { FileName = appStartPath + @"\wrap.bat", UseShellExecute = false, Arguments = args } };

            // start the process
            buildwrapper.Start();
        }

        private void menuApiBuild_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBoxXml.Text))
            {
                return;
            }

            if (this.proj == null)
            {
                return;
            }

            try
            {
                this.flowDoc = new FlowDocument();
                MSBuildExplorerLogger logger = new MSBuildExplorerLogger { Verbosity = LoggerVerbosity.Diagnostic };
                logger.OnMessage += this.LogBuildMessage;
                this.proj.Build(logger);
                this.TextBoxOutput.Document = this.flowDoc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

                Paragraph myParagraph1 = new Paragraph(new Run(indentation + valArgs.Message)) { LineHeight = 2 };
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
    }
}
