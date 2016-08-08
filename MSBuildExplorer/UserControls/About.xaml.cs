//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="About.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Interaction logic for About
    /// </summary>
    public partial class About
    {
        public About()
        {
            this.InitializeComponent();
            this.labelVersion.Content = GetFileVersion(Assembly.GetExecutingAssembly()).ToString();
        }

        private static Version GetFileVersion(Assembly asm)
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(asm.Location);
            return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
        }
    }
}
