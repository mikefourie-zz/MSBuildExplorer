//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildUsing.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;

    public class MSBuildUsing : SimpleObject
    {
        public MSBuildUsing(Project project, ProjectUsingTaskElement pute)
        {
            FileInfo f = new FileInfo(pute.TaskName);
            this.Name = f.Name;
            this.AssemblyFile = pute.AssemblyFile;
            this.TaskFactory = pute.TaskFactory;
            this.Source = project.ExpandString(pute.AssemblyFile);
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public string AssemblyFile { get; set; }

        public string TaskFactory { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
