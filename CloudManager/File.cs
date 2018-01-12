using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManager
{
    class File : Element
    {
        

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override void Download(String dirPath)
        {
            String originalFileName = null;
            String type = null;
            MemoryStream stream = CloudWorker.DownloadFile(ID, downloadProgress, ref originalFileName, ref type);
            FileStream fileStream = null;

            try
            {
                string fileName = String.Format("{0}{1}{2}", dirPath, System.IO.Path.DirectorySeparatorChar, originalFileName);
                fileStream = new FileStream(fileName, FileMode.Create);
                stream.WriteTo(fileStream);
                stream.Close();
                fileStream.Close();
            }

            catch (Exception ex)
            {
                //todo show message to user
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
                if (stream != null)
                    stream.Close();
            }
        }

        public override void Move()
        {
            throw new NotImplementedException();
        }

        public override void Remove()
        {
            throw new NotImplementedException();
        }

        public override void Uploaad()
        {
            throw new NotImplementedException();
        }
    }
}
