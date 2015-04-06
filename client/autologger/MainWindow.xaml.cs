using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace autologger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App app;
        private ToolTip tooltip;

        public MainWindow()
        {
            InitializeComponent();

            app = Application.Current as App;
            app.StateChanged += new EventHandler<AppChangeStateArgs>(app_StateChanged);
            app.PropertyChanged += new EventHandler(app_PropertyChanged);

            tooltip = new System.Windows.Controls.ToolTip();

            txtURL.Text = app.Server;
            txtURL.TextChanged += new TextChangedEventHandler(txtURL_TextChanged);

            txtUsername.Text = app.Username;
            txtPassword.Password = app.Password;
            txtPassword.PasswordChanged += new RoutedEventHandler(txtPassword_PasswordChanged);

            chkStartupLaunch.IsChecked = app.LaunchOnStartup;
            chkStartupLaunch.Checked += new RoutedEventHandler(chkStartupLaunch_Checked);
            chkStartupLaunch.Unchecked += new RoutedEventHandler(chkStartupLaunch_Unchecked);

            app_StateChanged(app, new AppChangeStateArgs(app.State));
        }

        void app_PropertyChanged(object sender, EventArgs e)
        {
            chkStartupLaunch.IsChecked = app.LaunchOnStartup;
        }

        void app_StateChanged(object sender, AppChangeStateArgs e)
        {
            txtPassword.ToolTip = null;
            buttonRequest.IsEnabled = true;
            switch (e.State)
            {
                case AppState.INVALID_PASSWORD:
                    txtPassword.ToolTip = tooltip;
                    tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Custom;
                    tooltip.Content = "Update password for this Windows user";
                    tooltip.IsOpen = true;
                    break;
                case AppState.CANT_CONNECT_SERVER:
                    break;
                case AppState.NEED_LOGIN:
                    buttonRequest.Content = "Login";
                    break;
                case AppState.LOGGED_IN:
                    buttonRequest.Content = "Logout";
                    break;
                case AppState.LOGGED_OUT:
                    buttonRequest.Content = "Login";
                    buttonRequest.IsEnabled = false;
                    break;
            }
        }

        void txtURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            app.Server = txtURL.Text;
        }

        void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            app.Password = txtPassword.Password;
        }

        void chkStartupLaunch_Checked(object sender, RoutedEventArgs e)
        {
            if(!app.LaunchOnStartup)
                app.LaunchOnStartup = true;
        }

        void chkStartupLaunch_Unchecked(object sender, RoutedEventArgs e)
        {
            if(app.LaunchOnStartup)
                app.LaunchOnStartup = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (app.State != AppState.LOGGED_IN)
            {
                if(app.State != AppState.LOGGED_OUT)
                    app.Login();   
            }
            else
            {
                app.Logout();
            }
        }

    }
}
