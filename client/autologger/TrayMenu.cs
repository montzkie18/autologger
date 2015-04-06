using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace autologger
{

    class TrayMenu
    {
        private App app;

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private MenuItem itemLoggedIn;
        private MenuItem itemStartup;

        public void Initialize()
        {
            app = (App)System.Windows.Application.Current;
            app.StateChanged += new EventHandler<AppChangeStateArgs>(app_StateChanged);
            app.PropertyChanged += new EventHandler(app_PropertyChanged);

            itemLoggedIn = new MenuItem();
            itemLoggedIn.Text = "Logged at 10:10";
            itemLoggedIn.Click += new EventHandler(itemLoggedIn_Click);

            itemStartup = new MenuItem();
            itemStartup.Text = "Launch on startup";
            itemStartup.Checked = app.LaunchOnStartup;
            itemStartup.Click += new EventHandler(itemStartup_Click);

            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(itemLoggedIn);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add(itemStartup);
            trayMenu.MenuItems.Add("Settings", new EventHandler(menuItem_ShowMainWindow));
            trayMenu.MenuItems.Add("Exit", new EventHandler(menuItem_Exit));

            trayIcon = new NotifyIcon();
            trayIcon.Text = "AttendanceLogger";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.DoubleClick += new EventHandler(menuItem_ShowMainWindow);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            trayIcon.BalloonTipClicked += new EventHandler(trayIcon_BalloonTipClicked);

            UpdateState(app.State);
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon)
        {
            trayIcon.ShowBalloonTip(3000, title, message, icon);
        }

        void UpdateState(AppState state)
        {
            switch (state)
            {
                case AppState.LOGGED_IN:
                    itemLoggedIn.Text = "Logged in at " + app.LastLoggedDate.ToShortTimeString();
                    itemLoggedIn.Checked = true;
                    break;
                case AppState.LOGGED_OUT:
                    itemLoggedIn.Text = "Logged out at " + app.LastLoggedDate.ToShortTimeString();
                    itemLoggedIn.Checked = true;
                    break;
                default:
                    itemLoggedIn.Text = "Not logged in";
                    itemLoggedIn.Checked = false;
                    break;
            }
        }

        void app_PropertyChanged(object sender, EventArgs e)
        {
            itemStartup.Checked = app.LaunchOnStartup;
        }

        void app_StateChanged(object sender, AppChangeStateArgs e)
        {
            UpdateState(e.State);
        }

        void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            app.ActivateMainWindow();
        }

        void itemLoggedIn_Click(object sender, EventArgs e)
        {
            if (app.State != AppState.LOGGED_IN)
            {
                if (app.State != AppState.LOGGED_OUT)
                    app.Login();
            }
            else
            {
                app.Logout();
            }
        }

        void itemStartup_Click(object sender, EventArgs e)
        {
            itemStartup.Checked = !itemStartup.Checked;
            if(app.LaunchOnStartup != itemStartup.Checked)
                app.LaunchOnStartup = itemStartup.Checked;
        }

        private void menuItem_ShowMainWindow(object sender, EventArgs e)
        {
            app.ActivateMainWindow();
        }

        private void menuItem_Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            app.Shutdown();
        }
    }
}
