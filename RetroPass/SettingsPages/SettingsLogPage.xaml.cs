using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RetroPass.SettingsPages
{
	public sealed partial class SettingsLogPage : Page
	{
		public SettingsLogPage()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{			
			EnableLoggingCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled];
			base.OnNavigatedTo(e);
		}

		private async void EnableLoggingCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] = true;
			await LogPage.SetLogging();

			LogFrame.Navigate(typeof(LogPage));
		}

		private async void EnableLoggingCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsLoggingEnabled] = false;
			await LogPage.SetLogging();
			LogFrame.Content = null;
		}

	}
}
