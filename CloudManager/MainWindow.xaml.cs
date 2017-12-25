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
        private CloudWorker actualWorker;
        private IDictionary<Button, CloudWorker> workerDict;
        private Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            settings = new Settings();
            workerDict = new Dictionary<Button, CloudWorker>();
            InitializeClouds();
        }

        private void InitializeClouds()
        {
            bool first = true;
            foreach (KeyValuePair<String, String> setting in settings.dictionary)
            {
                CloudWorker cloudWorker = new CloudWorker(this, setting.Value);
                //todo work in the style of the labelbutton
                Button button = new Button() { Content = setting.Key };
                button.Click += click_button_label;
                workerDict.Add(button, cloudWorker);
                if (first)
                {
                    cloudWorker.Initialize();
                    actualWorker = cloudWorker;
                }
                first = false;
            }
        }

        public void click_button_label(object sender, RoutedEventArgs e)
        {
            Button actualButton = sender as Button;
            if (actualButton == null)
                return;
            workerDict.TryGetValue(actualButton, out actualWorker);
            actualWorker.Initialize();
        }

        public void click_back(object sender, RoutedEventArgs e)
        {
            actualWorker.click_back();
        }
    }
}