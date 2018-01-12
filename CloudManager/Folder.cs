using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManager
{
    class Folder : Element
    {
        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override void Download(String dirPath)
        {
            String downloadFolder = String.Format("{0}//{1}", dirPath, Name);
            if (!Directory.Exists(downloadFolder))
                Directory.CreateDirectory(downloadFolder);
            IDictionary<String, String> elements = CloudWorker.GetChildElements(ID);
            foreach (KeyValuePair<String, String> keys in elements)
            {
                if (CloudWorker.IsFile(keys.Key))
                {
                    File file = new File() { ID = keys.Key, CloudWorker = CloudWorker, Name = keys.Value };
                    file.Download(downloadFolder);
                }
                //DownloadFile(keys.Key, downloadFolder);
                else
                {
                    Folder folder = new Folder() { ID = keys.Key, CloudWorker = CloudWorker, Name = keys.Value };
                    folder.Download(downloadFolder);
                    //DownloadFolder(keys.Key, downloadFolder, keys.Value);
                }
            }
        }

        public override void Move()
        {
            throw new NotImplementedException();
        }

        public override void Remove()
        {
            CloudWorker.DeleteFolder(ID);
        }

        public override void Upload(String parentID)
        {
            CloudWorker.UploadDirectory(Name, Name, parentID, Name);
        }
    }
}