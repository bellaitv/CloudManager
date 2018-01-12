using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManagerCommons
{
    public delegate int DownloadProgress();

    public interface ICloudWorker
    {
        IDictionary<String, String> GetFiles();

        IDictionary<String, String> GetDirectories();

        IDictionary<String, String> GetChildOfRootDir();

        IDictionary<String, String> GetChildElements(String root);

        IDictionary<String, String> Back(ref String id);
        bool IsFile(String rootID);

        System.IO.MemoryStream DownloadFile(String id, DownloadProgress progresss, ref String fileName, ref String type);

        bool UploadFile(string _uploadFile, string _parent);
        bool UploadDirectory(string _title, string _description, string _parent, string path);

        void DeleteFile(String id);

        void DeleteFolder(String id);
    }
}
