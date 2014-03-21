//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="FavoriteFileGroup.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.DataModel
{
    using System;
    using System.Collections.ObjectModel;

    public class FavoriteFileGroup : IComparable
    {
        public FavoriteFileGroup(string name)
        {
            this.Name = name;
            this.Files = new ObservableCollection<FavoriteFile>();
        }

        public string Name { get; set; }

        public ObservableCollection<FavoriteFile> Files { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public int CompareTo(object obj)
        {
            FavoriteFileGroup tempO = obj as FavoriteFileGroup;
            if (tempO == null)
            {
                throw new ArgumentException("Object is not FavoriteFileGroup");
            }

            return this.Name.CompareTo(tempO.Name);
        }
    }
}
