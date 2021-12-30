using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;

namespace Tmp.Link.Upload
{
    public class App : Application
    {
        private static SettingsModel? _settings;
        public static SettingsModel Settings
        {
            get
            {
                _settings ??= LoadSettings();
                return _settings;
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static SettingsModel LoadSettings()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "app.json");
            if (!File.Exists(path))
            {
                using var newFile = File.Create(path);
                return new();
            }

            using var jsonFile = File.OpenText(path);
            var jsonStr = jsonFile.ReadToEnd();
            return JsonConvert.DeserializeObject<SettingsModel>(jsonStr) ?? new();
        }

        public static void SaveSettings(SettingsModel model)
        {
            using StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "app.json"));
            sw.Write(JsonConvert.SerializeObject(model, Formatting.Indented));
        }
    }
}
