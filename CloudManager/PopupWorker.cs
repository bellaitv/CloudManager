﻿using System;
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
            download.Click += ((Object sender, RoutedEventArgs e) =>
            {
                Button actualButton = sender as Button;
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    String id = CloudsWorker.GetId(lastRightClickedButton);
                    if (!CloudWorker.IsFile(id))
                        DownloadFolder(id, dialog.SelectedPath, lastRightClickedButton.Content as String);
                    else
                        DownloadFile(id, dialog.SelectedPath);

                }

                popup.IsOpen = false;

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

        private void DownloadFolder(string id, string selectedPath, string name)
        {
            String downloadFolder = String.Format("{0}//{1}", selectedPath, name);
            if (!Directory.Exists(downloadFolder))
                Directory.CreateDirectory(downloadFolder);
            IDictionary<String, String> elements = CloudWorker.GetChildElements(id);
            foreach (KeyValuePair<String, String> keys in elements)
            {
                if (CloudWorker.IsFile(keys.Key))
                    DownloadFile(keys.Key, downloadFolder);
                else
                    DownloadFolder(keys.Key, downloadFolder, keys.Value);
            }
        }

        //todo move to another class
        private void DownloadFile(String id, String dirPath)
        {
            String originalFileName = null;
            String type = null;
            MemoryStream stream = CloudWorker.DownloadFile(id, CloudsWorker.downloadProgress, ref originalFileName, ref type);
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