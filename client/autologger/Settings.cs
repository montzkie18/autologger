using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;
using System.IO;

namespace autologger
{
    class SettingsData
    {
        public string server = AppURL.BASE_URL;
        public Dictionary<string, string> accounts = new Dictionary<string, string>();
    }

    class Settings
    {
        private SettingsData data;
        private SimpleAES encryptor;

        private const string foldername = "AttendanceLogger";
        private const string filename = "settings.conf";

        public SettingsData Data
        {
            get { return data; }
        }

        public string FullPath
        {
            get { return Path.Combine(DirectoryPath, filename); }
        }

        public string DirectoryPath
        {
            get { return Path.Combine(Path.GetTempPath(), foldername); }
        }

        public Settings() 
        {
            data = new SettingsData();
            encryptor = new SimpleAES();
        }

        public void Load()
        {
            if (File.Exists(FullPath))
            {
                StreamReader reader = new StreamReader(FullPath);
                string decodedString = encryptor.Decrypt(reader.ReadToEnd());
                data = JsonMapper.ToObject<SettingsData>(decodedString);
                reader.Close();
            }
        }

        public void Save()
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            string plainJSON = JsonMapper.ToJson(data);
            string encrypted = encryptor.Encrypt(plainJSON);
            StreamWriter writer = new StreamWriter(FullPath);
            writer.Write(encrypted);
            writer.Close();
        }
    }
}
