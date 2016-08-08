//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildTargetDetails.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    public class MSBuildTargetDetails : SimpleObject
    {
        public MSBuildTargetDetails(string name, string data)
        {
            this.Name = name;
            this.Data = data;
        }

        public string Name { get; set; }

        public string Data { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
