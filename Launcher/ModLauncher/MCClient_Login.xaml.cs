using System.Windows;
using System.Windows.Input;

namespace ModLauncher
{
    public partial class MCClient_Login : Window
    {
        public static MainWindow main;

        public MCClient_Login()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void GetError(string error)
        {
            MainWindow.CurrentWindow.GetError(error);
        }

        private void LoginClose_Btn_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                main.DeleteBtn_BC.IsEnabled = true;
                main.StartBtn_BC.IsEnabled = true;
            });
            Close();
        }

        private void XBoxLogin_Btn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Dispatcher.Invoke(() =>
            {
                main.DeleteBtn_BC.IsEnabled = true;
                main.StartBtn_BC.IsEnabled = true;
            });
            MC_Game.XBoxLogin();
            Close();
        }
    }
}
