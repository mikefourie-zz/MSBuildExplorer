//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="UserPreferences.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
namespace MSBuildExplorer
{
    internal class UserPreferences
    {
        private System.Windows.WindowState windowState;

        public UserPreferences()
        {
            this.Load();
            this.SizeToFit();
            this.MoveIntoView();
        }

        public double WindowRightColumn { get; set; }

        public double WindowLeftColumn { get; set; }

        public double WindowTop { get; set; }

        public double WindowLeft { get; set; }

        public double WindowHeight { get; set; }

        public double WindowWidth { get; set; }

        public System.Windows.WindowState WindowState { get; set; }

        public void Save()
        {
            if (this.windowState != System.Windows.WindowState.Minimized)
            {
                Properties.Settings.Default.WindowTop = this.WindowTop;
                Properties.Settings.Default.WindowLeft = this.WindowLeft;
                Properties.Settings.Default.WindowHeight = this.WindowHeight;
                Properties.Settings.Default.WindowWidth = this.WindowWidth;
                Properties.Settings.Default.RightColumnWidth = this.WindowRightColumn;
                Properties.Settings.Default.LeftColumnWidth = this.WindowLeftColumn;
                Properties.Settings.Default.WindowState = this.windowState;
                Properties.Settings.Default.Save();
            }
        }

        public void MoveIntoView()
        {
            if (this.WindowTop + (this.WindowHeight / 2) > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                this.WindowTop = System.Windows.SystemParameters.VirtualScreenHeight - this.WindowHeight;
            }

            if (this.WindowLeft + (this.WindowWidth / 2) > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                this.WindowLeft = System.Windows.SystemParameters.VirtualScreenWidth - this.WindowWidth;
            }

            if (this.WindowTop < 0)
            {
                this.WindowTop = 0;
            }

            if (this.WindowLeft < 0)
            {
                this.WindowLeft = 0;
            }
        }

        public void SizeToFit()
        {
            if (this.WindowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                this.WindowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (this.WindowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                this.WindowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }
        }

        private void Load()
        {
            this.WindowTop = Properties.Settings.Default.WindowTop;
            this.WindowLeft = Properties.Settings.Default.WindowLeft;
            this.WindowHeight = Properties.Settings.Default.WindowHeight;
            this.WindowWidth = Properties.Settings.Default.WindowWidth;
            this.WindowRightColumn = Properties.Settings.Default.RightColumnWidth;
            this.WindowLeftColumn = Properties.Settings.Default.LeftColumnWidth;
            this.windowState = Properties.Settings.Default.WindowState;
        }
    }
}
