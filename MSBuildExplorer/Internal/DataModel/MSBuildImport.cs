//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildImport.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System.IO;
    using Microsoft.Build.Evaluation;

    public class MSBuildImport : SimpleObject
    {
        public MSBuildImport(ResolvedImport imp)
        {
            FileInfo f = new FileInfo(imp.ImportedProject.FullPath);
            this.Name = f.Name;
            this.Source = imp.ImportedProject.FullPath;
            this.ToolsVersion = imp.ImportedProject.ToolsVersion;
            this.Condition = imp.ImportingElement.Condition;
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public string ToolsVersion { get; set; }

        public string Condition { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
