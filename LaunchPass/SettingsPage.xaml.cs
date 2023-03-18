using LaunchPass;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RetroPass
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        DataSourceManager dataSourceManager;
        Brush defaultForeground;

        public SettingsPage()
        {
            //Instance = this;
            InitializeComponent();
            Loaded += SettingsPage_Loaded;

            defaultForeground = ButtonActivateLocalStorage.Foreground;
        }

        protected override async void OnKeyDown(KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.GamepadView:
                case VirtualKey.Q:
                    if (LogPage.Instance.IsOpened == false)
                    {
                        LogPage.Instance.OnNavigatedTo();
                        await LogPage.Instance.ShowAsync();
                        LogPage.Instance.OnNavigatedFrom();
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        private void RefreshDataSourceUI()
        {
            StackPanelRemovableStorage.Visibility = Visibility.Collapsed;
            StackPanelLocalStorage.Visibility = Visibility.Collapsed;

            bool hasLocalDataSource = dataSourceManager.HasDataSource(DataSourceManager.DataSourceLocation.Local);
            bool hasRemovableDataSource = dataSourceManager.HasDataSource(DataSourceManager.DataSourceLocation.Removable);

            if (hasRemovableDataSource)
            {
                StackPanelRemovableStorage.Visibility = Visibility.Visible;
                ButtonActivateRemovableStorage.Visibility = Visibility.Visible;
                ButtonClearRemovableCache.Visibility = Visibility.Visible;
                //ButtonImport.Visibility = Visibility.Visible;

                if (dataSourceManager.IsImportInProgress())
                {
                    //ButtonImport.Visibility = Visibility.Collapsed;
                }
            }

            if (hasLocalDataSource || dataSourceManager.IsImportInProgress())
            {
                StackPanelLocalStorage.Visibility = Visibility.Visible;
                ButtonActivateLocalStorage.Visibility = Visibility.Visible;
                ButtonDeleteLocalStorage.Visibility = Visibility.Visible;
                StackPanelLocalXboxProgress.Visibility = Visibility.Collapsed;

                if (dataSourceManager.IsImportInProgress())
                {
                    ButtonActivateLocalStorage.Visibility = Visibility.Collapsed;
                    ButtonDeleteLocalStorage.Visibility = Visibility.Collapsed;
                    StackPanelLocalXboxProgress.Visibility = Visibility.Visible;
                }
                else if (dataSourceManager.ImportFinished == false)
                {
                    ButtonActivateLocalStorage.Visibility = Visibility.Collapsed;
                }
            }

            switch (dataSourceManager.ActiveDataSourceLocation)
            {
                case DataSourceManager.DataSourceLocation.None:
                    ButtonActivateLocalStorage.Content = "Activate";
                    ButtonActivateLocalStorage.Foreground = defaultForeground;
                    ButtonActivateRemovableStorage.Content = "Activate";
                    ButtonActivateRemovableStorage.Foreground = defaultForeground;
                    ButtonActivateRemovableStorage.Focus(FocusState.Keyboard);
                    break;

                case DataSourceManager.DataSourceLocation.Local:
                    ButtonActivateLocalStorage.Content = "Active";
                    ButtonActivateLocalStorage.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
                    ButtonActivateRemovableStorage.Content = "Activate";
                    ButtonActivateRemovableStorage.Foreground = defaultForeground;
                    ButtonActivateLocalStorage.Focus(FocusState.Keyboard);
                    break;

                case DataSourceManager.DataSourceLocation.Removable:
                    ButtonActivateLocalStorage.Content = "Activate";
                    ButtonActivateLocalStorage.Foreground = defaultForeground;
                    ButtonActivateRemovableStorage.Content = "Active";
                    ButtonActivateRemovableStorage.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
                    ButtonActivateRemovableStorage.Focus(FocusState.Keyboard);
                    break;

                default:
                    break;
            }
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayer.MediaPath = ((App)Application.Current).CurrentThemeSettings.GetMediaPath("SettingsPage");

            await dataSourceManager.ScanDataSource();

            bool hasLocalDataSource = dataSourceManager.HasDataSource(DataSourceManager.DataSourceLocation.Local);
            bool hasRemovableDataSource = dataSourceManager.HasDataSource(DataSourceManager.DataSourceLocation.Removable);

            if (hasLocalDataSource == false && hasRemovableDataSource == false)
            {
                TextStatus.Text = "Couldn't locate RetroPass Ultimate configuration file.\nMake sure a valid LaunchPass.xml is in the root of your removable storage!";
            }

            RefreshDataSourceUI();
        }

        //on windows, windows key + backspace
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            dataSourceManager = e.Parameter as DataSourceManager;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            dataSourceManager.OnImportStarted += OnImportStarted;
            dataSourceManager.OnImportUpdateProgress += OnImportUpdateProgress;
            dataSourceManager.OnImportFinished += OnImportFinished;
            dataSourceManager.OnImportError += OnImportError;

            AutoPlayVideoCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo];
            EnableLoggingCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled];

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;

            dataSourceManager.OnImportUpdateProgress -= OnImportUpdateProgress;
            dataSourceManager.OnImportFinished -= OnImportFinished;
            dataSourceManager.OnImportError -= OnImportError;

            base.OnNavigatedFrom(e);
        }

        private void ButtonActivateRemovableStorage_Click(object sender, RoutedEventArgs e)
        {
            dataSourceManager.ActivateDataSource(DataSourceManager.DataSourceLocation.Removable);
            RefreshDataSourceUI();
        }

        private void OnImportStarted()
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressSync.Value = 0;
                ProgressSyncText.Text = "0%";
                RefreshDataSourceUI();
            });
        }

        public void OnImportUpdateProgress(float progress)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressSync.Value = progress;
                ProgressSyncText.Text = ((int)progress).ToString() + "%";
            });
        }

        public void OnImportFinished(bool finished)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RefreshDataSourceUI();
            });
        }

        public void OnImportError()
        {
        }

        private async void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            //await dataSourceManager.CopyToLocalFolder();
        }

        private void ButtonImportCancel_Click(object sender, RoutedEventArgs e)
        {
            //dataSourceManager.CancelImport();
        }

        private void ButtonActivateLocalStorage_Click(object sender, RoutedEventArgs e)
        {
            dataSourceManager.ActivateDataSource(DataSourceManager.DataSourceLocation.Local);
            RefreshDataSourceUI();
        }

        private async void ButtonDeleteLocalStorage_Click(object sender, RoutedEventArgs e)
        {
            await dataSourceManager.DeleteLocalDataSource();
            StackPanelLocalStorage.Visibility = Visibility.Collapsed;
        }

        private async void ButtonClearRemovableCache_Click(object sender, RoutedEventArgs e)
        {
            await ThumbnailCache.Instance.Delete(DataSourceManager.DataSourceLocation.Removable);
        }

        private void AutoPlayVideoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo] = true;
        }

        private void AutoPlayVideoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo] = false;
        }

        private void EnableLoggingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] = true;
            LogPage.SetLogging();
        }

        private void EnableLoggingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] = false;
            LogPage.SetLogging();
        }

        private async void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uri = new Uri("microsoft-edge:https://github.com/Misunderstood-Wookiee/LaunchPass/wiki");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                var dialog = new MessageDialog("Sorry, something went wrong! Check your connection and make sure you have MS-Edge browser.");
                await dialog.ShowAsync();
            }
        }

        private void ButtonCustomize_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ThemeSettingsPage));
        }

        private async void RetroAchievements_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uri = new Uri("microsoft-edge:https://retroachievements.org/");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                var dialog = new MessageDialog("Sorry, something went wrong! Check your connection and make sure you have MS-Edge browser.");
                await dialog.ShowAsync();
            }
        }

        private async void Xhb_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uri = new Uri("https://discord.com/servers/xbox-emulation-hub-1007582798598647889");
            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                var dialog = new MessageDialog("Sorry, something went wrong! Check your connection and make sure you have MS-Edge browser.");
                await dialog.ShowAsync();
            }
        }

        private async void SM_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uri = new Uri("microsoft-edge:https://github.com/Misunderstood-Wookiee/LaunchPass");
            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                var dialog = new MessageDialog("Sorry, something went wrong! Check your connection and make sure you have MS-Edge browser.");
                await dialog.ShowAsync();
            }
        }

        private async void Our_Apps_Click(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            var uri = new Uri("microsoft-edge:https://github.com/Misunderstood-Wookiee/LaunchPass/wiki");
            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
                var dialog = new MessageDialog("Sorry, something went wrong! Check your connection and make sure you have MS-Edge browser.");
                await dialog.ShowAsync();
            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
        }
    }
}