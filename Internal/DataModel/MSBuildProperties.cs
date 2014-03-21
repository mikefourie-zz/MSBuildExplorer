//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildProperties.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------

namespace MSBuildExplorer.DataModel
{
    using Microsoft.Build.Evaluation;

    public class MSBuildProperties : SimpleObject
    {
        public MSBuildProperties(ProjectProperty property)
        {
            this.Name = property.Name;
            this.InitialValue = property.UnevaluatedValue;
            this.EvaluatedValue = property.EvaluatedValue;
            this.IsEnvironmentProperty = property.IsEnvironmentProperty;
            this.IsGlobalProperty = property.IsGlobalProperty;
            this.IsImported = property.IsImported;
            this.IsReservedProperty = property.IsReservedProperty;
            if (property.Xml != null)
            {
                this.Condition = property.Xml.Condition;
                this.ContainingProject = property.Xml.ContainingProject.FullPath;
            }
        }

        public string Name { get; set; }

        public bool IsEnvironmentProperty { get; set; }

        public bool IsGlobalProperty { get; set; }

        public bool IsImported { get; set; }

        public bool IsReservedProperty { get; set; }

        public string EvaluatedValue { get; set; }

        public string InitialValue { get; set; }

        public string Condition { get; set; }

        public string ContainingProject { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
