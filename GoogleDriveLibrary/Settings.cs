using System;
using System.IO;
using System.Xml.Linq;

namespace GoogleDriveLibrary
{
    public class Settings
    {
        private const String SETTINGS_FILE_NAME = "Settings.xml";

        public String ClientID { get; private set; }
        public String ClientSecret { get; private set; }
        public String User { get; private set; }

        public Settings()
        {
            String settingsFilePath = GetSettingsFile();
            foreach (XElement level1Element in XElement.Load(settingsFilePath).Elements("Property"))
            {
                string id = (level1Element.Attribute("name").Value);

                switch (id)
                {
                    case "client_id":
                        {
                            foreach (XElement level2Element in level1Element.Elements("product"))
                                ClientID = level2Element.Attribute("value").Value;

                            break;
                        }
                    case "client_secret": {
                            foreach (XElement level2Element in level1Element.Elements("product"))
                                ClientSecret = level2Element.Attribute("value").Value;
                            break;
                        }
                    case "user":
                        {
                            foreach (XElement level2Element in level1Element.Elements("product"))
                                User = level2Element.Attribute("value").Value;
                            break;
                        }
                    default:
                        throw new CloudManagerCommons.CloudManagerException();
                }
            }
            //ClientID = ConfigurationManager.AppSettings["client_id"];
            //ClientSecret = ConfigurationManager.AppSettings["client_secret"];
            //user = ConfigurationManager.AppSettings["user"];
        }

        private String GetSettingsFile()
        {
            String result = null;
            if (File.Exists(SETTINGS_FILE_NAME))
                return SETTINGS_FILE_NAME;
            string path = Environment.GetEnvironmentVariable("path");
            //todo get filepath of the file
            return result;
        }
    }
}
