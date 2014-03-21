//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildFileEqualityComparer.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;
    using System.Collections.Generic;
    using MSBuildExplorer.DataModel;

    internal class MSBuildFileEqualityComparer : IEqualityComparer<MSBuildFile>
    {
        public bool Equals(MSBuildFile x, MSBuildFile y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(MSBuildFile obj)
        {
            throw new NotImplementedException();
        }
    }
}
