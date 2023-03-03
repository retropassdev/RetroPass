using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RetroPass
{
    public sealed partial class ThemeSettingsPage : Page
    {
        private DataSourceManager dataSourceManager;
        private Brush defaultForeground;

        public ThemeSettingsPage()
        {
            //Instance = this;
            InitializeComponent();
            Loaded += ThemeSettingsPage_Loaded;
        }

        private void ThemeSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}