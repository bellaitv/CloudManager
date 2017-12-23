using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveBrowser
{
    public interface ICloudWorker
    {
        bool UploadFile(string _uploadFile, string _parent);
        bool UploadDirectory(string _title, string _description, string _parent, string path);
    }
}
