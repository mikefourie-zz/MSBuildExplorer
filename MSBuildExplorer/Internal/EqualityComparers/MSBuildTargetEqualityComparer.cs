//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildTargetEqualityComparer.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;
    using System.Collections.Generic;
    using MSBuildExplorer.DataModel;

    internal sealed class MSBuildTargetEqualityComparer : IEqualityComparer<MSBuildTarget>
    {
        private MSBuildTargetEqualityComparer()
        {
        }

        public bool Equals(MSBuildTarget x, MSBuildTarget y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(MSBuildTarget obj)
        {
            throw new NotImplementedException();
        }
    }
}
