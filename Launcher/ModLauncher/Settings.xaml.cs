using System;
using System.Windows;
using System.Windows.Input;

namespace ModLauncher
{
    public partial class SettingsWindow : Window
    {
        public static Settings G_Settings = new Settings();
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public static void GetError(string error)
        {
            MainWindow.CurrentWindow.GetError(error);
        }


        #region UI
        private void ModLauncher_ContentRendered(object sender, EventArgs e)
        {
            if (!G_Settings.Load())
                G_Settings.Save();

            RamUsage_Box.Text = G_Settings.Ram.ToString();
            IPAddress_Box.Text = G_Settings.Address;
            Port_Box.Text = G_Settings.Port.ToString();
        }

        private void SettingsClose_Btn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            G_Settings.Ram = int.Parse(RamUsage_Box.Text);
            G_Settings.Address = IPAddress_Box.Text;
            G_Settings.Port = int.Parse(Port_Box.Text);

            if (G_Settings.Save())
            {
                MessageBox.Show("Settings saved!");
                Close();
                return;
            }
            MainWindow.CurrentWindow.GetError("Failed to save Settings!");
        }

        private void Reset_Btn_Click(object sender, RoutedEventArgs e)
        {
            G_Settings = new Settings();

            RamUsage_Box.Text = G_Settings.Ram.ToString();
            IPAddress_Box.Text = G_Settings.Address;
            Port_Box.Text = G_Settings.Port.ToString();

            G_Settings.Save();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        #endregion

        private void LogOutMicrosoft_Btn_Click(object sender, RoutedEventArgs e)
        {
            MC_Game.XBoxLogout();
        }

        private void Refresh_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentWindow.RefreshTab();
        }
    }
}
