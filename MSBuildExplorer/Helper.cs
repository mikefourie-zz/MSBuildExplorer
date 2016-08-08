//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System;

    /// <summary>
    /// Class with various utility functions
    /// </summary>
    public static class Helper
    {
        internal enum MessageType
        {
            /// <summary>
            /// Info
            /// </summary>
            Info = 0,

            /// <summary>
            /// Warning
            /// </summary>
            Warning = 1,

            /// <summary>
            /// Error
            /// </summary>
            Error = 2,

            /// <summary>
            /// Success
            /// </summary>
            Success = 3,
        }
    }
}
