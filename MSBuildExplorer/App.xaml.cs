//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    using System.Windows;

    /// <summary>
    /// App
    /// </summary>
    public partial class App
    {
        public App()
        {
            this.Dispatcher.UnhandledException += this.OnDispatcherUnhandledException;
        }

        internal void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string exception = e.Exception.ToString();
            if (e.Exception.InnerException != null)
            {
                exception += ". Inner Exception = " + e.Exception.InnerException;
            }

            string errorMessage = string.Format("{0}. Please report this error", exception);
            MessageBox.Show(errorMessage, "Sorry, this was not meant to happen.", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            WpfSingleInstance.Make();
            base.OnStartup(e);
        }
    }
}
