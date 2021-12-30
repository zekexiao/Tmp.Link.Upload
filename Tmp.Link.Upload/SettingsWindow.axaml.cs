using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tmp.Link.Upload
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = SettingsViewModel.Instance();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
