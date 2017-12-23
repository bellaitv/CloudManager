using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v2.Data;

namespace GoogleDriveBrowser
{
    public interface IGoogleDriveWorker : ICloudWorker
    {
        IDictionary<String, String> GetFiles();

        IDictionary<String, String> GetDirectories();

        IDictionary<String, String> GetChildOfRootDir();

        IDictionary<String, String> GetChildElements(String root);

        IDictionary<String, String> Back(ref String id);
        bool IsFile(String rootID);

        System.IO.MemoryStream DownloadFile(String id, DownloadProgress progresss, ref String fileName, ref String type);
    }
}
