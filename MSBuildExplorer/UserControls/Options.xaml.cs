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
            this.checkBoxPromptDefaultTargetExecution.IsChecked = Settings.Default.PromptDefaultTargetExecution;
            this.TextBoxPreExecute.Text = Settings.Default.DefaultBootStrapper;
            this.TextBoxParameters.Text = Settings.Default.DefaultParameters;
        }

        private void buttonOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings.Default.PauseConsoleAfterExecution = (bool)this.checkBoxPauseConsoleAfterExecution.IsChecked;
            Settings.Default.PromptDefaultTargetExecution = (bool)this.checkBoxPromptDefaultTargetExecution.IsChecked;
            Settings.Default.DefaultBootStrapper = this.TextBoxPreExecute.Text;
            Settings.Default.DefaultParameters = this.TextBoxParameters.Text;
            Settings.Default.Save();
        }
    }
}
