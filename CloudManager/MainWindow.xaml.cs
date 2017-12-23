using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GoogleDriveBrowser;

namespace CloudManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        public const int MAX_ROW_COUNT = 5;
        public String DOWNLOAD_DIRECTORY = @"C:\TMP";

        private IGoogleDriveWorker googleDriverWorker;
        private IDictionary<Button, String> buttonDict;
        private String backID;

        private DownloadProgress downloadProgress;

        public MainWindow()
        {
            InitializeComponent();
            buttonDict = new Dictionary<Button, String>();
            InitializeGoogleDrive();
        }

        private void InitializeGoogleDrive()
        {
            googleDriverWorker = new GoogleDriverWorker();
            IDictionary<String, String> dirs = googleDriverWorker.GetChildOfRootDir();
            FillInTheCloudContentGrid(dirs);
            downloadProgress = DownloadProgressMethod;
        }

        public int DownloadProgressMethod() { return 0; }

        private void FillInTheCloudContentGrid(IDictionary<String, String> dirs)
        {
            int x = 0;
            GRID_CLOUD_CONTENT.RowDefinitions.Add(new RowDefinition());
            foreach (KeyValuePair<String, String> keys in dirs)
            {
                if (x % 5 == 0)
                    GRID_CLOUD_CONTENT.RowDefinitions.Add(new RowDefinition());
                Button actual = new Button() { Content = keys.Value };
                actual.Click += click;
                GRID_CLOUD_CONTENT.Children.Add(actual);
                Grid.SetRow(actual, GRID_CLOUD_CONTENT.RowDefinitions.Count - 1);
                Grid.SetColumn(actual, x);
                if (x > 4)
                    x = 0;
                else
                    x++;
                buttonDict.Add(actual, keys.Key);
            }
        }

        private void click_back(object sender, RoutedEventArgs e)
        {
            IDictionary<String, String> childs = googleDriverWorker.Back(ref backID);
            if (childs.Count == 0)
                //todo show error to user
                Console.WriteLine("error");
            GRID_CLOUD_CONTENT.Children.Clear();
            FillInTheCloudContentGrid(childs);
        }

        private void click(object sender, RoutedEventArgs e)
        {
            Button actual = sender as Button;
            if (actual == null)
                return;
            String rootID = null;
            buttonDict.TryGetValue(actual, out rootID);
            if (rootID == null)
                return;
            if (!googleDriverWorker.IsFile(rootID))
            {
                IDictionary<String, String> childs = googleDriverWorker.GetChildElements(rootID);
                if (childs.Count == 0)
                    //todo show error to user
                    Console.WriteLine("error");
                GRID_CLOUD_CONTENT.Children.Clear();
                FillInTheCloudContentGrid(childs);
                backID = rootID;
            }
            else
            {
                String originalFileName = null;
                MemoryStream stream = googleDriverWorker.DownloadFile(rootID, downloadProgress, ref originalFileName);
                FileStream fileStream = null;

                try
                {
                    string fileName = String.Format("{0}{1}{2}", DOWNLOAD_DIRECTORY, System.IO.Path.DirectorySeparatorChar,  originalFileName);
                    fileStream = new FileStream(fileName, FileMode.Create);
                    stream.WriteTo(fileStream);
                    //fileStream.Flush();
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
        }
    }
}
