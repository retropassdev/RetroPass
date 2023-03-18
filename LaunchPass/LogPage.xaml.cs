using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RetroPass
{
    public class LogItem
    {
        public enum LogLevel
        {
            Information,
            Warning,
            Error
        }

        public LogItem(string text)
        {
            Text = text;

            if (Text.StartsWith("RetroPass Error:"))
            {
                Level = LogLevel.Error;
            }
            else if (Text.StartsWith("RetroPass Warning:"))
            {
                Level = LogLevel.Warning;
            }
            else
            {
                Level = LogLevel.Information;
            }
        }

        public string Text { get; set; }
        public LogLevel Level { get; set; }
    }

    public sealed partial class LogPage : ContentDialog
    {
        public LogPage()
        {
            this.InitializeComponent();
        }

        private static LogPage instance = null;
        public bool IsOpened { get; private set; }

        private ObservableCollection<LogItem> logEntries = new ObservableCollection<LogItem>();

        public static LogPage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogPage();
                }
                return instance;
            }
        }

        public static async void SetLogging()
        {
            if ((bool)ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] == true)
            {
                var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("RetroPass.log", CreationCollisionOption.ReplaceExisting);
                Stream logStream = await file.OpenStreamForWriteAsync();
                var logFileTraceListener = new TextWriterTraceListener(logStream, "logFileTraceListener");
                Trace.Listeners.Add(logFileTraceListener);
            }
            else
            {
                Trace.Listeners.Clear();
            }
            //Trace.AutoFlush = true;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        public async void OnNavigatedTo()
        {
            //LogListView.ItemsSource = logEntries;
            IsOpened = true;
            var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync("RetroPass.log");
            string text = await FileIO.ReadTextAsync(file);

            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    logEntries.Add(new LogItem(line));
                }
            }

            LogListView.SelectedIndex = LogListView.Items.Count - 1;

            this.Closing += OnClosing;
        }

        public void OnNavigatedFrom()
        {
            this.Closing -= OnClosing;
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            IsOpened = false;
        }

        private void LogListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                args.Handled = true;
            }

            if (args.Phase == 0)
            {
                var item = args.Item as LogItem;
                var container = args.ItemContainer.ContentTemplateRoot as TextBlock;

                var whiteBrush = new SolidColorBrush(Windows.UI.Colors.White);
                var yellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
                var redBrush = new SolidColorBrush(Windows.UI.Colors.Red);

                switch (item.Level)
                {
                    case LogItem.LogLevel.Information:
                        container.Foreground = whiteBrush;
                        break;

                    case LogItem.LogLevel.Warning:
                        container.Foreground = yellowBrush;
                        break;

                    case LogItem.LogLevel.Error:
                        container.Foreground = redBrush;
                        break;

                    default:
                        break;
                }

                args.Handled = true;
            }
        }
    }
}