using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

	public sealed partial class LogPage : Page
	{

		public LogPage()
		{
			this.InitializeComponent();
		}

		private static LogPage instance = null;
		private static Stream logStream = null;
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
		public static async Task SetLogging()
		{			
			if ((bool)ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] == true)
			{
				if (logStream == null)
				{
					var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("RetroPass.log", CreationCollisionOption.ReplaceExisting);
					logStream = await file.OpenStreamForWriteAsync();
					var logFileTraceListener = new TextWriterTraceListener(logStream, "logFileTraceListener");
					Trace.Listeners.Add(logFileTraceListener);
				}
			}
			else
			{
				if (logStream != null)
				{
					logStream.Dispose();
					logStream = null;
				}
				Trace.Listeners.Clear();
			}
			//Trace.AutoFlush = true;
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			//LogListView.ItemsSource = logEntries;
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

			base.OnNavigatedTo(e);
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

		private void LogListView_GotFocus(object sender, RoutedEventArgs e)
		{
			var fc = LogListView.FindDescendant<FocusControl>();
			fc.Enabled = true;
		}

		private void LogListView_LostFocus(object sender, RoutedEventArgs e)
		{
			var fc = LogListView.FindDescendant<FocusControl>();
			fc.Enabled = false;
		}
	}
}
