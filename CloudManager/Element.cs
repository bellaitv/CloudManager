using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudManagerCommons;

namespace CloudManager
{
    public abstract class Element
    {
        public String ID { get; set; }
        public String Name { get; set; }
        public ICloudWorker CloudWorker { get; set; }
        public DownloadProgress downloadProgress { private set; get; }

        public abstract void Download(String dirPath);
        public abstract void Uploaad();
        public abstract void Remove();
        public abstract void Delete();
        public abstract void Move();
    }
}
