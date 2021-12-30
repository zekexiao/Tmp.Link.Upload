using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Tmp.Link.Upload
{
    public partial class MainWindow : Window
    {
        private readonly List<string> _selectedFiles = new List<string>();

        public TextBlock InfoBlock;
        public Border DropBorder;
        
        private readonly HttpClient _httpClient = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            DropBorder = this.Get<Border>("DropBorder");
            InfoBlock = this.Get<TextBlock>("InfoBlock");
            DropBorder.AddHandler(DragDrop.DropEvent, OnDropBorderDrop);
            DropBorder.AddHandler(DragDrop.DragEnterEvent, (s, e) =>
            {
                var fileNames = e.Data.GetFileNames() ?? new List<string>();
                if (fileNames.Any())
                {
                    DropBorder.Background = new SolidColorBrush(Color.FromRgb(0xe2, 0xe2, 0xe2));
                }
            });
            DropBorder.AddHandler(DragDrop.DragLeaveEvent, (s, e) =>
            {
                DropBorder.Background = Brushes.Transparent;
            });
        }

        private async void OnDropBorderDrop(object s, DragEventArgs e)
        {
            _selectedFiles.Clear();
            var fileNamesNullable = e.Data.GetFileNames();
            if (fileNamesNullable is { } fileNames)
            {
                foreach (var file in fileNames)
                {
                    _selectedFiles.Add(file);
                }
            }

            if (App.Settings.AutoUpload)
            {
                await UploadAll();
            }

            DropBorder.Background = Brushes.Transparent;
        }

        /// <summary>
        /// curl -k -F "file=@ your file path (etc.. @/root/test.bin)"
        /// -F "token=yourtoken" -F "model=99"  -X POST "https://connect.tmp.link/api_v2/cli_uploader"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartBtnClick(object? sender, RoutedEventArgs e)
        {
            UploadAll().ConfigureAwait(false);
        }

        private async Task UploadAll()
        {
            // max upload 3 files one time, in case service 503
            var chunks = _selectedFiles.Chunk(3);

            foreach (var chunk in chunks)
            {
                await Task.WhenAll(chunk.Select(Upload).ToArray());
            }
        }

        private async Task Upload(string fileName)
        {
            FileInfo info = new FileInfo(fileName);
            if (!info.Exists)
            {
                Log($"Upload File: {fileName} Failed, File Not Exists\n");
                return;
            }
            
            await using var file = info.OpenRead();
            MultipartFormDataContent i = new MultipartFormDataContent();
            i.Add(new StreamContent(file),  "file", info.Name);
            i.Add(new StringContent(App.Settings.Token), "token");
            i.Add(new StringContent($"{App.Settings.ExpiresToUploadModel()}"), "model");
            using var resp = await _httpClient.PostAsync(new Uri("https://connect.tmp.link/api_v2/cli_uploader"), i);
            var result = await resp.Content.ReadAsStringAsync();
            Log($"Upload File: {fileName}\n{result}");
            if (resp.IsSuccessStatusCode)
            {
                var httpStartIndex = result.IndexOf("http", StringComparison.OrdinalIgnoreCase);
                if (httpStartIndex != -1)
                {
                    var downloadUrl = result.Substring(httpStartIndex, result.IndexOf("\n", httpStartIndex, StringComparison.OrdinalIgnoreCase) - httpStartIndex);
                    // TODO, add downloadUrl
                }
            }
            
        }
        private void Log(string str)
        {
            var logBox = this.Get<TextBox>("LogBox");
            if(logBox != null)
                logBox.Text += str + Environment.NewLine;
        }

        private void OnSettingClick(object? sender, RoutedEventArgs e)
        {
            var settingWindow = new SettingsWindow();
            settingWindow.ShowDialog(this);
        }

        private void OnExitClick(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            App.SaveSettings(App.Settings);
            base.OnClosed(e);
        }
    }
}
