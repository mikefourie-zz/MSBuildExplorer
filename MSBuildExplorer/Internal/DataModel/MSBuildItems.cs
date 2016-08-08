//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildItems.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    public class MSBuildItems : SimpleObject
    {
        public MSBuildItems(string identity, string include, string finalinclude, bool isImported)
        {
            this.Identity = identity;
            this.Include = include;
            this.FinalInclude = finalinclude;
            this.IsImported = isImported;
        }

        public string Identity { get; set; }

        public bool IsImported { get; set; }

        public string FinalInclude { get; set; }

        public string Include { get; set; }

        public override string ToString()
        {
            return this.Identity;
        }
    }
}
