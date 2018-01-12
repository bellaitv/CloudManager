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
using System.Windows.Input;

namespace CloudManager
{
    //todo rename
    public class CloudsWorker
    {
        public const int MAX_ROW_COUNT = 5;
        public String DOWNLOAD_DIRECTORY = @"C:\TMP";
        public DownloadProgress downloadProgress { private set; get; }

        private MainWindow mainWindow;
        private Utility utility;
        //private IDictionary<Button, String> buttonDict;
        private IList<Element> elements;
        private ICloudWorker cloudWorker;
        private String backID;
        private String assemblyPath;
        private PopupWorker popupWorker;

        public CloudsWorker(MainWindow mainWindow, String assemblyPath)
        {
            this.mainWindow = mainWindow;
            utility = new Utility();
            popupWorker = new PopupWorker() {CloudsWorker = this};
            popupWorker.Initialize();
            this.assemblyPath = assemblyPath;
            //buttonDict = new Dictionary<Button, String>();
            elements = new List<Element>();
        }

        public void Initialize()
        {
            Type type = InitializeCloudWorker();
            cloudWorker = (ICloudWorker)Activator.CreateInstance(type);
            if (cloudWorker == null)
                //todo show msgbox insted of this
                throw new CloudManagerException("The {0} assembly does not contain the implementation of ICloudWorker.", ExceptionType.InitializeType);
            IDictionary<String, String> dirs = cloudWorker.GetChildOfRootDir();
            FillInTheCloudContentGrid(dirs);
            downloadProgress = DownloadProgressMethod;
            popupWorker.CloudWorker = cloudWorker;
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
            foreach (KeyValuePair<String, String> keys in dirs)
            {
                if (x % 5 == 0)
                {
                    mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Add(new RowDefinition());
                    x = 0;
                }
                String content = null;
                if (keys.Value.Length < 8)
                    content = keys.Value;
                else
                    content = String.Format("{0}...", keys.Value.Substring(0, 8));
                Button actual = new Button() { Content = content , Tag = keys.Value};
                Element element = null;
                element = !cloudWorker.IsFile(keys.Key) ? element = new Folder() { ID = keys.Key, CloudWorker = this.cloudWorker, Name = keys.Value} :
                    element = new File() { ID = keys.Key, CloudWorker = cloudWorker, Name = keys.Value};
                actual.Tag = element;
                actual.MouseRightButtonDown += ((Object sender, MouseButtonEventArgs e) =>
                {
                    Button actualButton = sender as Button;
                    if (actualButton == null)
                        return;
                    popupWorker.popupName.IsOpen = false;
                    popupWorker.ShowPopupRightClick(actualButton);
                });
                actual.MouseLeave += ((Object sender, MouseEventArgs e) =>
                {
                    popupWorker.popupName.IsOpen = false;
                });
                actual.MouseEnter += ((Object sender, MouseEventArgs e) =>
                {
                    Button actualButton = sender as Button;
                    if (actualButton == null)
                        return;
                    String text = actualButton.Content as String;
                    if (text.Length > 8)
                        popupWorker.ShowPopupName(actualButton.Tag as String);
                });
                actual.Click += click_content;
                mainWindow.GRID_CLOUD_CONTENT.Children.Add(actual);
                Grid.SetRow(actual, mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Count - 1);
                Grid.SetColumn(actual, x);
                x++;
                //buttonDict.Add(actual, keys.Key);
                elements.Add(element);
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

        private void click_content(object sender, RoutedEventArgs e)
        {
            mainWindow.Popup1.IsOpen = false;
            utility.DeleteDirectoryContent(DOWNLOAD_DIRECTORY);
            Button actual = sender as Button;
            if (actual == null)
                return;
            String rootID = GetId(actual);
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

        public String GetId(Button button) {
            Element tagElement = button.Tag as Element;
            if (tagElement == null)
                return null;
            String result = null;
            //todo linq
            foreach (Element element in elements)
                if (element.Name.Equals(tagElement.Name))
                    result = element.ID;
            return result;
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
            String[] lines = System.IO.File.ReadAllLines(fileName);
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
    }
}