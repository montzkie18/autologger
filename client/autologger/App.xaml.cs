using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Win32;

namespace autologger
{
    public enum AppState
    {
        NEED_LOGIN,
        INVALID_USERNAME,
        INVALID_PASSWORD,
        CANT_CONNECT_SERVER,
        LOGGED_IN,
        LOGGED_OUT
    }

    public class AppChangeStateArgs : EventArgs
    {
        private AppState _state;

        public AppState State { get { return _state; } }

        public AppChangeStateArgs(AppState state)
        {
            _state = state;
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string APP_NAME = "autologger";

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        Settings settings = new Settings();

        private MainWindow mainWindow;
        private TrayMenu trayMenu;

        private string _server = "";
        private string _username = "";
        private string _password = "";
        private AppState _state = AppState.NEED_LOGIN;
        private DateTime _lastLoggedDate;

        public event EventHandler<AppChangeStateArgs> StateChanged;
        public event EventHandler PropertyChanged;

        public string Username
        {
            get { return _username; }
        }

        public string Password
        {
            get { return _password; }
            set 
            { 
                _password = value;
                settings.Data.accounts[_username] = _password;
                if (State == AppState.INVALID_PASSWORD)
                    State = AppState.NEED_LOGIN;
            }
        }

        public string Server
        {
            get { return _server; }
            set
            {
                _server = value.TrimEnd('/');
                settings.Data.server = _server;
            }
        }

        public bool LaunchOnStartup
        {
            get { return (rkApp.GetValue(APP_NAME) != null); }
            set 
            {
                if (value)
                    rkApp.SetValue(APP_NAME, System.Reflection.Assembly.GetEntryAssembly().Location);
                else
                    rkApp.DeleteValue(APP_NAME);

                if (PropertyChanged != null)
                    PropertyChanged(this, new EventArgs());
            }
        }

        public AppState State
        {
            get { return _state; }
            protected set
            {
                _state = value;
                if (StateChanged != null)
                    StateChanged(this, new AppChangeStateArgs(_state));
            }
        }

        public DateTime LastLoggedDate
        {
            get { return _lastLoggedDate; }
        }

        private void AppStart(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SessionEnding += new SessionEndingCancelEventHandler(App_SessionEnding);
            LoadSettings();

            trayMenu = new TrayMenu();
            trayMenu.Initialize();

            Login();
        }

        private void LoadSettings()
        {
            settings.Load();

            _server = settings.Data.server;
            _username = Environment.UserName;

            if (settings.Data != null && settings.Data.accounts != null)
            {
                if (settings.Data.accounts.ContainsKey(_username))
                    _password = settings.Data.accounts[_username];
            }            
        }

        private void ShowMainWindow()
        {
            mainWindow = new MainWindow();
            mainWindow.Closed += new EventHandler(mainWindow_Closed);
            mainWindow.Show();
        }

        public void ActivateMainWindow()
        {
            if (mainWindow == null)
            {
                ShowMainWindow();
            }
            else
            {
                if (mainWindow.Visibility != Visibility.Visible)
                    mainWindow.Visibility = Visibility.Visible;
                else
                    mainWindow.Activate();
            }
        }

        private string PostRequest(string url, NameValueCollection postParams, out Exception exception)
        {
            string result = string.Empty;
            using (var client = new WebClient())
            {
                try
                {
                    var response = client.UploadValues(url, postParams);
                    result = Encoding.Default.GetString(response);
                    exception = null;
                }
                catch (Exception e)
                {
                    exception = e;
                    result = e.Message;
                }
            }
            return result;
        }

        private bool ConnectToServer(string url)
        {
            var postParams = new NameValueCollection();
            postParams["username"] = Username;
            postParams["password"] = Password;

            Exception exception = null;
            var success = false;
            var result = PostRequest(Server + url, postParams, out exception);
            if (exception != null)
            {
                if (exception is WebException && (exception as WebException).Response is HttpWebResponse)
                {
                    HttpWebResponse response = (HttpWebResponse)(exception as WebException).Response;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            // wrong username
                            State = AppState.INVALID_USERNAME;
                            break;
                        case HttpStatusCode.Unauthorized:
                            // wrong password
                            State = AppState.INVALID_PASSWORD;
                            break;
                        case HttpStatusCode.Conflict:
                            // already logged in / logged out
                            string dateString = response.Headers["Last-Modified"];
                            _lastLoggedDate = Convert.ToDateTime(dateString);
                            State = url.Equals(AppURL.LOGIN) ? AppState.LOGGED_IN : AppState.LOGGED_OUT;
                            break;
                        case HttpStatusCode.Forbidden:
                            // trying to logout without logging in
                            State = AppState.NEED_LOGIN;
                            break;
                        default:
                            // unknown error
                            State = AppState.CANT_CONNECT_SERVER;
                            break;
                    }
                }
                else
                {
                    Console.WriteLine(exception.Message);
                    State = AppState.CANT_CONNECT_SERVER;
                }
            }
            else
            {
                _lastLoggedDate = Convert.ToDateTime(result);
                State = url.Equals(AppURL.LOGIN) ? AppState.LOGGED_IN : AppState.LOGGED_OUT;
                success = true;
            }
            return success;
        }

        private void PopupMessage(AppState state)
        {
            switch (state)
            {
                case AppState.INVALID_USERNAME:
                    trayMenu.ShowBalloonTip("Unknown User", 
                        "Please register this user account to the server.", System.Windows.Forms.ToolTipIcon.Error);
                    break;
                case AppState.INVALID_PASSWORD:
                    trayMenu.ShowBalloonTip("Invalid Password", 
                        "Update your Windows password now and try again.", System.Windows.Forms.ToolTipIcon.Error);
                    break;
                case AppState.CANT_CONNECT_SERVER:
                    trayMenu.ShowBalloonTip("Server Unreachable", 
                        "Make sure you have configured the right server URL.", System.Windows.Forms.ToolTipIcon.Error);
                    break;
                case AppState.LOGGED_IN:
                    trayMenu.ShowBalloonTip("Logged In", 
                        "Checked in at " + LastLoggedDate.ToShortTimeString(), System.Windows.Forms.ToolTipIcon.Info);
                    break;
                case AppState.LOGGED_OUT:
                    trayMenu.ShowBalloonTip("Logged Out",
                        "Checked out at " + LastLoggedDate.ToShortTimeString(), System.Windows.Forms.ToolTipIcon.Info);
                    break;
            }
        }

        public void Login()
        {
            bool success = ConnectToServer(AppURL.LOGIN);
            PopupMessage(State);
        }

        public void Logout()
        {
            MessageBoxResult result = MessageBox.Show(
                "Want to call it a day?", "Logout Confirmation", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = ConnectToServer(AppURL.LOGOUT);
                PopupMessage(State);
            }
        }

        #region EventHandlers

        void mainWindow_Closed(object sender, EventArgs e)
        {
            mainWindow = null;
        }

        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (State == AppState.LOGGED_IN)
                Logout();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            settings.Save();
        }

        #endregion


    }
}
