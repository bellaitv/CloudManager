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
using CloudManagerCommons;


namespace CloudManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CloudsWorker actualWorker;
        private IDictionary<ListBoxItem, CloudsWorker> workerDict;
        private Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            settings = new Settings();
            workerDict = new Dictionary<ListBoxItem, CloudsWorker>();
            InitializeClouds();
        }

        private void InitializeClouds()
        {
            bool first = true;
            Style buttonStyle = Resources["buttonList"] as Style; 
            foreach (KeyValuePair<String, String> setting in settings.dictionary)
            {
                CloudsWorker cloudWorker = new CloudsWorker(this, setting.Value);
                //todo work in the style of the labelbutton

                //Button button = new Button() { Content = setting.Key, Style = buttonStyle };
                //Label label = new Label();
                //label.PreviewMouseDown += click_button_label;
                //button.Click += click_button_label;
                ListBoxItem item = new ListBoxItem() { Content = setting.Key};
                item.PreviewMouseDown += click_button_label;
                workerDict.Add(item, cloudWorker);
                if (first)
                {
                    cloudWorker.Initialize();
                    actualWorker = cloudWorker;
                    LABEL_NAME.Content = setting.Key;
                }
                CLOUD_LIST.Items.Add(item);
                //GRID_CLOUD_LIST.RowDefinitions.Add(new RowDefinition());
                //GRID_CLOUD_LIST.Children.Add(item);
                //Grid.SetRow(item, GRID_CLOUD_LIST.RowDefinitions.Count - 1);
                //Grid.SetColumn(item, 0);
                first = false;
            }
        }

        public void click_button_label(object sender, RoutedEventArgs e)
        {
            ListBoxItem actualItem = sender as ListBoxItem;
            if (actualItem == null)
                return;
            workerDict.TryGetValue(actualItem, out actualWorker);
            GRID_CLOUD_CONTENT.Children.Clear();
            GRID_CLOUD_CONTENT.RowDefinitions.Clear();
            actualWorker.Initialize();
        }

        public void click_back(object sender, RoutedEventArgs e)
        {
            actualWorker.click_back();
        }
    }
}