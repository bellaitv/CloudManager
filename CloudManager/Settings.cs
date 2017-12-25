using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace CloudManager
{
    public class Settings
    {
        public IDictionary<String, String> dictionary;

        public Settings() {
            dictionary = new Dictionary<String, String>();
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings.Count == 0)
            {
                Console.WriteLine("AppSettings is empty.");
            }
            else
            {
                foreach (var key in appSettings.AllKeys)
                {
                    String value = null;
                    value = appSettings[key] as String;
                    dictionary.Add(key, value);
                }
            }
        }
    }
}
