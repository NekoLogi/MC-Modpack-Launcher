using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ModLauncher
{
    public partial class MainWindow : Window
    {
        public static MainWindow CurrentWindow;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void AppStartup()
        {
            CurrentWindow = this;
            RefreshTab();
        }

        public void RefreshTab()
        {
            FileServer.Connect(0);
            BC_CheckModpack();

            if (FileServer.isConnected)
            {
                Dispatcher.Invoke(() =>
                {
                    AvalVerLabel_BC.Content = FileServer.Pack.Version;
                    DescriptionLabel.Text = FileServer.Pack.Description;
                    TitleLabel.Content = FileServer.Pack.Name;
                    CurrVerLabel_BC.Content = FileManager.Pack.Version;
                    PatchBox_BC.Text = FileManager.Pack.Log;
                });
            }
            else
            {
                BC_SetOffline();
            }

        }

        public void GetError(string error)
        {
            Dispatcher.Invoke(() =>
            {
                GetError(error);
            });
        }

        private void DeleteModpack()
        {
            if (Directory.Exists($"Launcher/{FileManager.Pack.Name}"))
            {
                try
                {
                    BC_ClientState(3);
                    Directory.Delete($"Launcher/{FileManager.Pack.Name}", true);
                    File.Delete(Modpack.PATH);

                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"{FileManager.Pack.Name} deleted!");
                        CurrVerLabel_BC.Content = "No Version";
                        BC_ClientState(0);
                    });
                } catch (Exception)
                {
                    BC_ClientState(4);
                    GetError($"Failed to delete {FileManager.Pack.Name}!");
                }
            }
            else
            {
                GetError(
                $"Failed to delete {FileManager.Pack.Name}!\n" +
                    $"{FileManager.Pack.Name} doesn't exist.");
                BC_ClientState(4);
            }
        }

        #region BlockyCrafters
        // 0 = Install | 1 = Update | 2 = Play | 3 = In Progress | 4 = Error
        private void BC_ClientState(int state)
        {
            Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case 0:
                        StartBtn_BC.Content = "Installieren";
                        CurrVerLabel_BC.Content = "No Version";
                        DeleteBtn_BC.IsEnabled = true;
                        StartBtn_BC.IsEnabled = true;
                        break;

                    case 1:
                        StartBtn_BC.Content = "Update";
                        CurrVerLabel_BC.Content = FileManager.Pack.Version;
                        DeleteBtn_BC.IsEnabled = true;
                        StartBtn_BC.IsEnabled = true;
                        break;

                    case 2:
                        StartBtn_BC.Content = "Spielen";
                        CurrVerLabel_BC.Content = FileManager.Pack.Version;
                        DeleteBtn_BC.IsEnabled = true;
                        StartBtn_BC.IsEnabled = true;
                        break;

                    case 3:
                        DeleteBtn_BC.IsEnabled = false;
                        StartBtn_BC.IsEnabled = false;
                        break;

                    case 4:
                        DeleteBtn_BC.IsEnabled = true;
                        StartBtn_BC.IsEnabled = true;
                        break;
                }
            });
        }

        private void BC_CheckModpack()
        {
            bool isUpToDate = FileManager.CheckVersion();
            Dispatcher.Invoke(() =>
            {
                CurrVerLabel_BC.Content = FileManager.Pack.Version;
            });


            if (FileServer.isConnected)
            {
                if (Directory.Exists($"Launcher/{FileManager.Pack.Name}"))
                {
                    if (isUpToDate)
                    {
                        BC_ClientState(2);
                    }
                    else
                    {
                        BC_ClientState(1);
                    }
                }
                else
                {
                    BC_ClientState(0);
                }
            }
            else
            {
                if (Directory.Exists($"Launcher/{FileManager.Pack.Name}"))
                {
                    BC_ClientState(2);
                }
                else
                {
                    BC_ClientState(0);
                }
            }
        }

        private void BC_SetOffline()
        {
            Dispatcher.Invoke(() =>
            {
                AvalVerLabel_BC.Content = FileManager.Pack.Version;
                PatchBox_BC.Text = FileManager.Pack.Log;
                DescriptionLabel.Text = FileManager.Pack.Description;
                TitleLabel.Content = FileManager.Pack.Name;
                CurrVerLabel_BC.Content = FileManager.Pack.Version;
            });
        }

        private void BC_UpdateGame()
        {
            try
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar_BC.Visibility = Visibility.Visible;
                    });
                    BC_ClientState(3);
                    FileServer.Connect(2);

                    BC_ClientState(2);
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar_BC.Visibility = Visibility.Hidden;
                        MessageBox.Show($"{FileManager.Pack.Name} updated!");
                    });
                });
            } catch (Exception)
            {
                File.Delete($"Cache/{FileManager.Pack.Name.ToLower()}_update.7z");

                Dispatcher.Invoke(() =>
                {
                    ProgressBar_BC.Visibility = Visibility.Hidden;
                });
                GetError($"Failed to update {FileManager.Pack.Name}!\n" +
                    "Check for internet connection and firewall.");
                BC_ClientState(2);
            }
        }

        private void BC_InstallGame()
        {
            if (Directory.Exists($"Launcher/{FileManager.Pack.Name}"))
            {
                Dispatcher.Invoke(() =>
                {
                    StartBtn_BC.Content = "Spielen";
                    MessageBox.Show($"{FileManager.Pack.Name} already installed!");
                });
                BC_ClientState(4);
            }
            else
            {
                try
                {
                    Task.Run(() =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar_BC.Visibility = Visibility.Visible;
                            ProgressLabel.Visibility = Visibility.Visible;
                        });

                        FileServer.Connect(1);

                        BC_ClientState(2);
                        Dispatcher.Invoke(() =>
                        {
                            ProgressBar_BC.Visibility = Visibility.Hidden;
                            ProgressLabel.Visibility = Visibility.Hidden;
                            MessageBox.Show($"{FileServer.Pack.Name} installed!");
                        });
                    });
                    RefreshTab();
                    RenderLogo();
                } catch (Exception)
                {
                    File.Delete($"Cache/{FileManager.Pack.Name.ToLower()}.7z");

                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar_BC.Visibility = Visibility.Hidden;
                        ProgressLabel.Visibility = Visibility.Hidden;
                    });
                    GetError($"Failed to install {FileManager.Pack.Name}!\n" +
                        "Check for internet connection and firewall.");

                    BC_ClientState(4);
                }
            }
        }

        private void BC_StartGame()
        {
            if (Directory.Exists($"Launcher/{FileManager.Pack.Name}"))
            {
                BC_ClientState(3);
                MCClient_Login.main = this;
                MCClient_Login mcClient = new MCClient_Login();
                mcClient.Show();
            }
            else
            {
                BC_ClientState(0);

                Dispatcher.Invoke(() =>
                {
                    GetError($"Failed to start {FileManager.Pack.Name}!");
                });
            }
        }

        #endregion

        #region UI

        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (FileServer.isConnected)
            {
                switch ((sender as Button).Content.ToString())
                {
                    case "Installieren":
                        BC_InstallGame();
                        break;

                    case "Update":
                        BC_UpdateGame();
                        break;

                    case "Spielen":
                        BC_StartGame();
                        break;
                }
            }
            else
            {
                GetError(
                    "Connection failed!\n" +
                    "Please check your connection and firewall settings.");
            }
        }

        public void ChangeInstallStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressLabel.Content = message;
            });
        }

        private void ModLauncher_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ModLauncher_ContentRendered(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (SettingsWindow.G_Settings.Load())
                    SettingsWindow.G_Settings.Save();

                AppStartup();
                RenderLogo();
            });
        }

        private void RenderLogo()
        {
            string path = $"Launcher/{FileManager.Pack.Name}/Logo.png";

            if (!File.Exists(path))
                return;

            Dispatcher.Invoke(() =>
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                LogoImg.Source = bitmap;
            });
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Btn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Btn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Settings_Btn_Click(object sender, RoutedEventArgs e)
        {
            var menu = new SettingsWindow();
            menu.Show();
        }

        private void Info_Btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Created by NekoLogi\nLauncher version: 4.1.0");
        }

        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            DeleteModpack();
        }

        #endregion
    }
}
