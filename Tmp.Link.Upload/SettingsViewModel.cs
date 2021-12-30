using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Tmp.Link.Upload
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly SettingsViewModel _settingsViewModel = new SettingsViewModel();

        public static SettingsViewModel Instance()
        {
            return _settingsViewModel;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Token
        {
            get => App.Settings.Token;
            set
            {
                App.Settings.Token = value;
                OnPropertyChanged(nameof(Token));
            }
        }

        public bool AutoStart
        {
            get => App.Settings.AutoStart;
            set
            {
                App.Settings.AutoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }

        public bool AutoUpload
        {
            get => App.Settings.AutoUpload;
            set
            {
                App.Settings.AutoUpload = value;
                OnPropertyChanged(nameof(AutoUpload));
            }
        }

        public int Expires
        {
            get => App.Settings.Expires;
            set
            {
                App.Settings.Expires = value;
                OnPropertyChanged(nameof(Expires));
            }
        }
    }
}
