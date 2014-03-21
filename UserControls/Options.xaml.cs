//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="Options.xaml.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer.UserControls
{
    using MSBuildExplorer.Properties;

    /// <summary>
    /// Interaction logic for Options
    /// </summary>
    public partial class Options
    {
        public Options()
        {
            this.InitializeComponent();
            this.checkBoxPauseConsoleAfterExecution.IsChecked = Settings.Default.PauseConsoleAfterExecution;
            this.checkBoxMinimizeToSystemTray.IsChecked = Settings.Default.MinimizeToSystemTray;
            this.checkBoxPromptDefaultTargetExecution.IsChecked = Settings.Default.PromptDefaultTargetExecution;
            this.checkBoxMinimizeToSystemTrayOnClose.IsChecked = Settings.Default.MinimizeOnClose;
            this.TextBoxPreExecute.Text = Settings.Default.DefaultBootStrapper;
            this.TextBoxParameters.Text = Settings.Default.DefaultParameters;
        }

        private void buttonOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.PauseConsoleAfterExecution = (bool)this.checkBoxPauseConsoleAfterExecution.IsChecked;
            Settings.Default.MinimizeToSystemTray = (bool)this.checkBoxMinimizeToSystemTray.IsChecked;
            Settings.Default.PromptDefaultTargetExecution = (bool)this.checkBoxPromptDefaultTargetExecution.IsChecked;
            Settings.Default.MinimizeOnClose = (bool)this.checkBoxMinimizeToSystemTrayOnClose.IsChecked;
            Settings.Default.DefaultBootStrapper = this.TextBoxPreExecute.Text;
            Settings.Default.DefaultParameters = this.TextBoxParameters.Text;
            Settings.Default.Save();
        }
    }
}
