//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildFile.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using Microsoft.Build.Evaluation;

    public class MSBuildFile : SimpleObject, IComparable
    {
        public MSBuildFile(Project proj)
        {
            FileInfo f = new FileInfo(proj.FullPath);
            this.ProjectFile = proj;
            this.Name = f.Name;
            this.Source = proj.FullPath;
            this.Targets = new ObservableCollection<MSBuildTarget>();
            this.Properties = new ObservableCollection<MSBuildProperties>();
            this.Items = new ObservableCollection<MSBuildItems>();
            this.Usings = new ObservableCollection<MSBuildUsing>();
            this.Imports = new ObservableCollection<MSBuildImport>();
			this.GlobalConfigurations = new ObservableCollection<String>();
			this.GlobalPlatforms = new ObservableCollection<String>();
        }

        public Project ProjectFile { get; set; }
        
        public string Name { get; set; }

        public string Source { get; set; }

        public string ToolsVersion { get; set; }

        public ObservableCollection<MSBuildTarget> Targets { get; set; }

        public ObservableCollection<MSBuildProperties> Properties { get; set; }

		public ObservableCollection<String> GlobalConfigurations { get; set; }

		public ObservableCollection<String> GlobalPlatforms { get; set; }

		public ObservableCollection<MSBuildUsing> Usings { get; set; }

        public ObservableCollection<MSBuildItems> Items { get; set; }

        public ObservableCollection<MSBuildImport> Imports { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public int CompareTo(object obj)
        {
            MSBuildFile tempO = obj as MSBuildFile;
            if (tempO == null)
            {
                throw new ArgumentException("Object is not MSBuildFile");
            }

            return this.Name.CompareTo(tempO.Name);
        }
    }
}
