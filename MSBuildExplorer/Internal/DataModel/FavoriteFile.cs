//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="FavoriteFile.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System;
    using System.Collections.ObjectModel;

    public class FavoriteFile : IComparable
    {
        public FavoriteFile(string path, FavoriteEntity fav)
        {
            this.Name = fav.friendlyName;
            this.FullPath = path;
            this.GroupName = fav.groupName;
            this.ProjectFile = fav.file;
            this.TargetSets = new ObservableCollection<TargetSet>();
            foreach (TargetSet s in fav.TargetSet)
            {
                this.TargetSets.Add(s);
            }
        }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public string ProjectFile { get; set; }

        public string FullPath { get; set; }

        public ObservableCollection<TargetSet> TargetSets { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public int CompareTo(object obj)
        {
            FavoriteFile tempO = obj as FavoriteFile;
            if (tempO == null)
            {
                throw new ArgumentException("Object is not FavoriteFile");
            }

            return this.Name.CompareTo(tempO.Name);
        }
    }
}
