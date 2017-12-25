using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudManagerCommons;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Reflection;

namespace CloudManager
{
    public class CloudWorker
    {
        public const int MAX_ROW_COUNT = 5;
        public String DOWNLOAD_DIRECTORY = @"C:\TMP";

        private MainWindow mainWindow;
        private DownloadProgress downloadProgress;
        private Utility utility;
        private IDictionary<Button, String> buttonDict;
        private ICloudWorker cloudWorker;
        private String backID;
        private String assemblyPath;

        public CloudWorker(MainWindow mainWindow, String assemblyPath)
        {
            this.mainWindow = mainWindow;
            utility = new Utility();
            this.assemblyPath = assemblyPath;
            buttonDict = new Dictionary<Button, String>();
        }

        public void Initialize()
        {
            Type type = InitializeCloudWorker();
            cloudWorker = (ICloudWorker)Activator.CreateInstance(type);
            if (cloudWorker == null)
                //todo show msgbox insted of this
                throw new CloudManagerException("The {0} assembly does not contain the implementation of ICloudWorker.");
            IDictionary<String, String> dirs = cloudWorker.GetChildOfRootDir();
            FillInTheCloudContentGrid(dirs);
            downloadProgress = DownloadProgressMethod;
        }

        private Type InitializeCloudWorker()
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                foreach (Type type in assembly.GetTypes())
                    if (type.GetInterfaces().Contains(typeof(ICloudWorker)))
                        return type;
            }
            catch (Exception e)
            {
                //todo show error to user
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public int DownloadProgressMethod() { return 0; }

        private void FillInTheCloudContentGrid(IDictionary<String, String> dirs)
        {
            int x = 0;
            mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Add(new RowDefinition());
            foreach (KeyValuePair<String, String> keys in dirs)
            {
                if (x % 5 == 0)
                    mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Add(new RowDefinition());
                Button actual = new Button() { Content = keys.Value, Margin = new Thickness(0, 0, 5, 1) };
                actual.Click += click;
                mainWindow.GRID_CLOUD_CONTENT.Children.Add(actual);
                Grid.SetRow(actual, mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Count - 1);
                Grid.SetColumn(actual, x);
                if (x > 4)
                    x = 0;
                else
                    x++;
                buttonDict.Add(actual, keys.Key);
            }
        }

        public void click_back()
        {
            if (String.IsNullOrEmpty(backID))
                return;
            IDictionary<String, String> childs = cloudWorker.Back(ref backID);
            if (childs.Count == 0)
                //todo show error to user
                Console.WriteLine("error");
            mainWindow.GRID_CLOUD_CONTENT.Children.Clear();
            mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Clear();
            FillInTheCloudContentGrid(childs);
        }

        private void click(object sender, RoutedEventArgs e)
        {
            mainWindow.Popup1.IsOpen = false;
            utility.DeleteDirectoryContent(DOWNLOAD_DIRECTORY);
            Button actual = sender as Button;
            if (actual == null)
                return;
            String rootID = null;
            buttonDict.TryGetValue(actual, out rootID);
            if (rootID == null)
                return;
            if (!cloudWorker.IsFile(rootID))
            {
                IDictionary<String, String> childs = cloudWorker.GetChildElements(rootID);
                if (childs.Count == 0)
                    //todo show error to user
                    Console.WriteLine("error");
                mainWindow.GRID_CLOUD_CONTENT.Children.Clear();
                mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Clear();
                FillInTheCloudContentGrid(childs);
                backID = rootID;
            }
            else
                WorkWithFile(rootID);
        }

        private void WorkWithFile(String rootID)
        {
            String originalFileName = null;
            String type = null;
            MemoryStream stream = cloudWorker.DownloadFile(rootID, downloadProgress, ref originalFileName, ref type);
            FileStream fileStream = null;

            try
            {
                string fileName = String.Format("{0}{1}{2}", DOWNLOAD_DIRECTORY, System.IO.Path.DirectorySeparatorChar, originalFileName);
                fileStream = new FileStream(fileName, FileMode.Create);
                stream.WriteTo(fileStream);
                stream.Close();
                fileStream.Close();
                if (type.Contains("text"))
                    ShowPopupText(fileName);
                else
                    ShowPopupImage(fileName);
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


        private void ShowPopupText(string fileName)
        {
            String[] lines = File.ReadAllLines(fileName);
            StringBuilder builder = new StringBuilder();
            foreach (String line in lines)
                builder.AppendLine(line);
            TextBox textBox = new TextBox();
            textBox.Text = builder.ToString();
            var dockPanel = new DockPanel();
            mainWindow.Popup1.Child = dockPanel;
            dockPanel.Children.Add(textBox);
            mainWindow.Popup1.IsOpen = true;
        }

        private void ShowPopupImage(String fileName)
        {
            Image myImage = new Image();
            BitmapImage myImageSource = new BitmapImage();
            myImageSource.BeginInit();
            myImageSource.UriSource = new Uri(fileName);
            myImageSource.EndInit();
            myImage.Source = myImageSource;
            //Button actualButton = GetButtonFromID(rootID);
            var dockPanel = new DockPanel();
            mainWindow.Popup1.Child = dockPanel;
            dockPanel.Children.Add(myImage);
            mainWindow.Popup1.IsOpen = true;
        }

        private Button GetButtonFromID(string rootID)
        {
            foreach (KeyValuePair<Button, String> keys in buttonDict)
                if (keys.Value.Equals(rootID))
                    return keys.Key;
            //return new Button(); ??
            return null;
        }
    }
}
