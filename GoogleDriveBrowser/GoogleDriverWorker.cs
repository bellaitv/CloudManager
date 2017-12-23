using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Logging;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;


namespace GoogleDriveBrowser
{
    public delegate int DownloadProgress();

    public class GoogleDriverWorker : IGoogleDriveWorker
    {
        private const String DIRECTORY_MIME_TYPE = "application/vnd.google-apps.folder";

        private string[] scopes = new String[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
        private string clientID = "753440381473-8vir1veqn1pt4pc0egfu2imimuroq7q2.apps.googleusercontent.com";//"bellaitv"; // configból?
        private string clientSecret = "kPNZIzBvQDu4CXixz0HzlFyl";//"brasilia94;"; //configból?
        private DriveService service;
        //todo karbantartani pl backroundworker
        private IList<File> all = new List<File>();

        public GoogleDriverWorker()
        {
            Connect();
            //todo itt kellene elindítani a backroundworker-t, hogy az 'all' listát karbantartsa
            GetAllFiles();
        }

        //data to config?
        private void Connect()
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientID,
                ClientSecret = clientSecret
            },
                                                                 scopes,
                                                                 "bellaitv",
                                                                 CancellationToken.None,
                                                                 new FileDataStore("Daimto.GoogleDrive.Auth.Store")).Result;

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveBrowser",
            });
        }

        private FileList GetAllFiles()
        {
            FilesResource.ListRequest request = service.Files.List();
            request.MaxResults = 10000000;
            FileList files = request.Execute();
            foreach (var item in files.Items)
            {
                if (!all.Contains(item))
                    all.Add(item);
            }
            return files;
        }

        public IDictionary<String, String> GetFiles()
        {
            IDictionary<String, String> result = new Dictionary<String, String>();
            //linq?
            foreach (var item in all)
                if (!item.MimeType.Equals(DIRECTORY_MIME_TYPE))
                    result.Add(item.Id, item.Title);
            return result;
        }

        public IDictionary<String, String> GetDirectories()
        {
            IDictionary<String, String> result = new Dictionary<String, String>();
            foreach (var item in all)
                if (item.MimeType.Equals(DIRECTORY_MIME_TYPE))
                    result.Add(item.Id, item.Title);
            return result;
        }

        public bool UploadDirectory(string _title, string _description, string _parentID, string path)
        {
            bool result = false;
            string id = UploadEmptyDirectory(_title, _description, _parentID);
            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
                result = UploadFile(file, id);
            return result;
        }

        private string GetDirectoryId(string title)
        {
            IDictionary<String, String> dirs = GetDirectories();
            string result = string.Empty;
            //todo linq

            foreach (KeyValuePair<String, String> keys in dirs)
                if (keys.Value.Equals(title))
                    return keys.Key;

            return result;
        }

        private string UploadEmptyDirectory(string _title, string _description, string _parent)
        {
            File NewDirectory = null;

            // Create metaData for a new Directory
            File body = new File();
            body.Title = _title;
            body.Description = _description;
            body.MimeType = DIRECTORY_MIME_TYPE;
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parent } };

            FilesResource.InsertRequest request = service.Files.Insert(body);
            NewDirectory = request.Execute();
            return NewDirectory.Id;

        }

        public bool UploadFile(string _uploadFile, string _parentID)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Title = System.IO.Path.GetFileName(_uploadFile);
                body.Description = "File uploaded by Diamto Drive Sample";
                body.MimeType = GetMimeType(_uploadFile);
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = _parentID } };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody != null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return false;
            }

        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public IDictionary<String, String> GetChildOfRootDir()
        {
            IDictionary<String, String> result = new Dictionary<String, String>();
            foreach (var item in all)
                if (item.Parents.Count == 0)
                    result.Add(item.Id, item.Title);

            return result;
        }

        public IDictionary<String, String> GetChildElements(String rootID)
        {
            ChildrenResource.ListRequest request = service.Children.List(rootID);
            IDictionary<String, String> result = new Dictionary<String, String>();
            ChildList list = request.Execute();
            foreach (ChildReference child in list.Items)
                foreach (File file in all)
                {
                    if (child.Id.Equals(file.Id))
                        result.Add(file.Id, file.Title);
                }

            return result;
        }

        private IList<ParentReference> GetParents(String id)
        {
            //"0Bw796eEWuOTyYzk0czhvTHU0U2c"
            List<ParentReference> result = new List<ParentReference>();
            ParentsResource.ListRequest request = service.Parents.List(id);
            try
            {
                ParentList parents = request.Execute();
                return parents.Items;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }

        public IDictionary<string, string> Back(ref string id)
        {
            IDictionary<String, String> result = new Dictionary<String, String>();
            IList<ParentReference> parents = GetParents(id);
            foreach (ParentReference actual in parents)
            {
                id = actual.Id;
                return GetChildElements(actual.Id);
            }
            return result;
        }

        public bool IsFile(string rootID)
        {
            FilesResource.ListRequest request = service.Files.List();
            FileList files = request.Execute();
            foreach (var actual in files.Items)
                if (actual.Id.Equals(rootID) && !actual.MimeType.Equals(DIRECTORY_MIME_TYPE))
                    return true;
            return false;
        }

        //todo rename on progresss
        public System.IO.MemoryStream DownloadFile(String id, DownloadProgress progresss, ref String fileName)
        {
            fileName = GetFileNameFromID(id);
            var request = service.Files.Get(id);
            var stream = new System.IO.MemoryStream();
            request.MediaDownloader.ProgressChanged +=
    (IDownloadProgress progress) =>
    {
        switch (progress.Status)
        {
            case DownloadStatus.Downloading:
                {
                    Console.WriteLine(progress.BytesDownloaded);
                    break;
                }
            case DownloadStatus.Completed:
                {
                    Console.WriteLine("Download complete.");
                    break;
                }
            case DownloadStatus.Failed:
                {
                    Console.WriteLine("Download failed.");
                    break;
                }
        }
    };

            request.Download(stream);
            return stream;
        }

        private string GetFileNameFromID(string id)
        {
            var query =
                from x in all
                where x.Id == id
                select x.Title;
            using (var sequenceEnum = query.GetEnumerator())
            {
                while (sequenceEnum.MoveNext())
                {
                    return sequenceEnum.Current;
                }
            }
            return string.Empty;
        }
    }
}