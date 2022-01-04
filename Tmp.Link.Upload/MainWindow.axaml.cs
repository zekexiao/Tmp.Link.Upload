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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tmp.Link.Upload.Utils;

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
            DropBorder.AddHandler(DragDrop.DragLeaveEvent, (_, _) => { DropBorder.Background = Brushes.Transparent; });
        }

        private async void OnDropBorderDrop(object? s, DragEventArgs e)
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

            InfoBlock.Text = $"Total {_selectedFiles.Count} Files";

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

        #region Services
        private async Task UploadAll()
        {
            // max upload 3 files one time, in case service 503
            var fileInfos = new List<FileInfo>();
            foreach (var fullFilePath in _selectedFiles)
            {
                if (TryGetFileInfo(fullFilePath, out var info))
                {
                    fileInfos.Add(info);
                }
            }

            var chunks = fileInfos.Chunk(3);

            var uploadingBlock = this.Get<TextBlock>("UploadingBlock");

            foreach (var chunk in chunks)
            {
                uploadingBlock.Text = $"Uploading: {string.Join(", ", chunk.Select(info => info.Name))}";
                await Task.WhenAll(chunk.Select(Upload).ToArray());
            }

            var sumBytes = fileInfos.Select(info => info.Length).Sum();
            uploadingBlock.Text = $"Uploaded: {fileInfos.Count} files, {CommonUtils.ByteStrToSizeStr(sumBytes)}";
        }

        private bool TryGetFileInfo(string fileName, out FileInfo info)
        {
            info = new FileInfo(fileName);
            DirectoryInfo dirInfo = new DirectoryInfo(fileName);
            if (dirInfo.Exists)
            {
                Log($"Upload: {fileName} Failed, It's a Directory\n");
                return false;
            }
            
            if (!info.Exists)
            {
                Log($"Upload: {fileName} Failed, File Not Exists\n");
                return false;
            }

            return true;
        }
        private async Task Upload(FileInfo fileInfo)
        {
            await using var file = fileInfo.OpenRead();
            using var resp = await FormBuilder.Init(_httpClient)
                .AddForm("token", App.Settings.Token)
                .AddForm("model", App.Settings.ExpiresToUploadModel())
                .AddForm("file", fileInfo.Name, file)
                .PostAsync(new Uri("https://connect.tmp.link/api_v2/cli_uploader"));
            
            var result = await resp.Content.ReadAsStringAsync();
            Log($"Upload File: {fileInfo.FullName}\n{result}");
            if (resp.IsSuccessStatusCode)
            {
                var httpStartIndex = result.IndexOf("http", StringComparison.OrdinalIgnoreCase);
                if (httpStartIndex != -1)
                {
                    var downloadUrl = result.Substring(httpStartIndex,
                        result.IndexOf("\n", httpStartIndex, StringComparison.OrdinalIgnoreCase) - httpStartIndex);
                    // TODO, add downloadUrl
                }
            }
        }

        class TotalDataModel
        {
            [JsonProperty("nums")]
            public string Nums { get; set; }
            [JsonProperty("size")]
            public string Size { get; set; }
        }

        class TotalModel
        {
            [JsonProperty("data")]
            public TotalDataModel Data { get; set; }
            [JsonProperty("debug")]
            public object Debug { get; set; }
            [JsonProperty("status")]
            public string Status { get; set; }
        }

        private async Task<TotalModel> GetInfo()
        {
            using var resp = await FormBuilder.Init(_httpClient)
                                 .AddForm("token", App.Settings.Token)
                                 .AddForm("action", "total")
                                 .PostAsync(new Uri("https://tun.tmp.link/api_v2/file")); 
            var result = await resp.Content.ReadAsStringAsync();
            var total = JsonConvert.DeserializeObject<TotalModel>(result);

            return total;
        }

        #endregion

        private void Log(string str)
        {
            var logBox = this.Get<TextBox>("LogBox");
            if (logBox != null)
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

        private async void OnGetInfoBtnClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var info = await GetInfo();
                var nums = info.Data.Nums;
                var size = CommonUtils.ByteStrToSizeStr(info.Data.Size);
                btn.Content = $"{nums} files, {size}";
            }
        }
    }
}