//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System.Windows;

    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WpfSingleInstance.Make();

            base.OnStartup(e);
        }
    }
}
