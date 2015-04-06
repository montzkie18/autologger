using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace autologger
{
    static class AppURL
    {
        public const string BASE_URL = "http://localhost";
        private const string APP_URL = "/attendance/";

        public static string LOGIN = APP_URL + "login.php";
        public static string LOGOUT = APP_URL + "logout.php";
    }
}
