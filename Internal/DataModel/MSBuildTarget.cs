//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildTarget.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    public class MSBuildTarget : SimpleObject, INotifyPropertyChanged
    {
        private readonly Regex formatRegex = new Regex(@"\s");
        private bool isSelected;

        public MSBuildTarget(ProjectTargetInstance pti, Project project, MSBuildFile parent = null)
        {
            this.Target = pti;
            this.Name = pti.Name;
            this.Inputs = pti.Inputs;
            this.Outputs = pti.Outputs;
            this.Location = pti.FullPath;
            this.DependsOnTargets = pti.DependsOnTargets;

            this.TargetColor = "Black";
            this.FontStyle = "Normal";

            if (!string.IsNullOrEmpty(this.Target.Condition))
            {
                this.TargetCondition = this.Target.Condition;
                this.TargetColor = "Gray";
                this.FontStyle = "Italic";
            }

            if (project.Xml.DefaultTargets.Contains(pti.Name))
            {
                this.TargetColor = "MediumBlue";
            }

            if (project.Xml.InitialTargets.Contains(pti.Name))
            {
                this.TargetColor = "LimeGreen";
            }

            if (project.Xml.InitialTargets.Contains(pti.Name) && project.Xml.DefaultTargets.Contains(pti.Name))
            {
                this.TargetColor = "Gold";
            }
            
            this.Targets = new ObservableCollection<MSBuildTarget>();

            string targets = project.ExpandString(pti.DependsOnTargets);
            targets = this.formatRegex.Replace(targets, string.Empty);
            int i = 0;
            foreach (var target in targets.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                this.Targets.Add(new MSBuildTarget(target));
                i++;
            }

            foreach (var x in project.Xml.Targets.Where(x => x.Name == pti.Name))
            {
                this.BeforeTargets = x.BeforeTargets;
                this.AfterTargets = x.AfterTargets;
            }

            if (parent != null)
            {
                this.Parent = parent;
            }
        }

        public MSBuildTarget(string name, string color = "black")
        {
            this.Name = name;
            this.TargetColor = color;
        }

        public MSBuildFile Parent { get; set; }

        public string Name { get; set; }

        public string TargetColor { get; set; }

        public string BeforeTargets { get; set; }

        public string AfterTargets { get; set; }

        public string DependsOnTargets { get; set; }

        public string Inputs { get; set; }

        public string Outputs { get; set; }

        public string Location { get; set; }

        public string TargetCondition { get; set; }

        public string FontStyle { get; set; }

        public ProjectTargetInstance Target { get; set; }

        public ObservableCollection<MSBuildTarget> Targets { get; set; }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                this.isSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
