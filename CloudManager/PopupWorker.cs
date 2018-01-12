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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CloudManager
{
    public class PopupWorker
    {
        public Popup popupName { get; set; }
        public Popup popupRightClick { get; set; }

        private Button lastRightClickedButton;

        public ICloudWorker CloudWorker { get; set; }

        public CloudsWorker CloudsWorker { get; set; }

        public void Initialize()
        {
            popupName = CreatePopupName();
            popupRightClick = CreateRightClickPopup();
        }

        private Popup CreatePopupName()
        {
            Popup popup = new Popup()
            {
                Margin = new Thickness(10, 0, 0, 13),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50,
                Height = 20,
                IsOpen = false,
                Placement = PlacementMode.Center
            };
            var dockPanel = new DockPanel();
            TextBox textBox = new TextBox();
            popup.Child = dockPanel;
            dockPanel.Children.Add(textBox);
            return popup;
        }

        private Popup CreateRightClickPopup()
        {
            Popup popup = new Popup()
            {
                Margin = new Thickness(10, 0, 0, 13),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 150,
                Height = 150,
                IsOpen = false,
                Placement = PlacementMode.Center
            };
            var point = Mouse.GetPosition(Application.Current.MainWindow);
            popup.HorizontalOffset = point.X;
            popup.VerticalOffset = point.Y;
            Grid grid = new Grid();
            TextBox textBox = new TextBox();
            Button download = new Button() { Content = "Download" };
            //todo after actions update the grid
            download.Click += ((Object sender, RoutedEventArgs e) =>
            {
                popup.IsOpen = false;
                Element element = null;
                Button actualButton = sender as Button;
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    String id = CloudsWorker.GetId(lastRightClickedButton);
                    element = !CloudWorker.IsFile(id) ?
                    element = new Folder() { ID = id, CloudWorker = this.CloudWorker, Name = lastRightClickedButton.Content as String } :
                    element = new File() { ID = id, CloudWorker = this.CloudWorker, Name = lastRightClickedButton.Content as String };
                    element.Download(dialog.SelectedPath);
                }
            });
            Button moveToOtherCloud = new Button() { Content = "Move to another cloud" };
            moveToOtherCloud.Click += ((Object sender, RoutedEventArgs e) =>
            {
                popup.IsOpen = false;
            });
            Button share = new Button() { Content = "Share" };
            share.Click += ((Object sender, RoutedEventArgs e) =>
            {
                popup.IsOpen = false;
            });
            Button rename = new Button() { Content = "Rename" };
            rename.Click += ((Object sender, RoutedEventArgs e) =>
            {
                popup.IsOpen = false;
            });
            Button delete = new Button() { Content = "Delete" };
            delete.Click += ((Object sender, RoutedEventArgs e) =>
            {
                popup.IsOpen = false;
                Element element = null;
                Button actualButton = sender as Button;
                String id = CloudsWorker.GetId(lastRightClickedButton);
                element = !CloudWorker.IsFile(id) ?
                element = new Folder() { ID = id, CloudWorker = this.CloudWorker, Name = lastRightClickedButton.Content as String } :
                element = new File() { ID = id, CloudWorker = this.CloudWorker, Name = lastRightClickedButton.Content as String };
                element.Remove();
            });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.Children.Add(textBox);
            grid.Children.Add(download);
            grid.Children.Add(moveToOtherCloud);
            grid.Children.Add(delete);
            Grid.SetRow(textBox, 0);
            Grid.SetRow(download, 1);//mainWindow.GRID_CLOUD_CONTENT.RowDefinitions.Count - 1
            Grid.SetRow(moveToOtherCloud, 2);
            Grid.SetRow(delete, 3);
            Grid.SetColumn(textBox, 0);
            Grid.SetColumn(download, 0);
            Grid.SetColumn(moveToOtherCloud, 0);
            Grid.SetColumn(delete, 0);
            popup.Child = grid;
            return popup;
        }

        internal void ShowPopupRightClick(Button b)
        {
            lastRightClickedButton = b;
            popupRightClick.IsOpen = true;
            var point = Mouse.GetPosition(Application.Current.MainWindow);
            popupRightClick.HorizontalOffset = point.X;
            popupRightClick.VerticalOffset = point.Y;
            Grid grid = popupRightClick.Child as Grid;
            if (grid == null)
                return;
            TextBox textBox = grid.Children[0] as TextBox;
            textBox.Text = b.Content as String;
        }

        internal void ShowPopupName(String text)
        {
            var point = Mouse.GetPosition(Application.Current.MainWindow);
            popupName.HorizontalOffset = point.X;
            popupName.VerticalOffset = point.Y;
            DockPanel dockPanel = popupName.Child as DockPanel;
            if (dockPanel == null)
                return;
            TextBox textBox = dockPanel.Children[0] as TextBox;
            textBox.Text = text;
            popupName.Width = textBox.Text.Length * 7;
            popupName.IsOpen = true;
        }
    }
}
