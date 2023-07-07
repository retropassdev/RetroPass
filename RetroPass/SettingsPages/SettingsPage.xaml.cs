using RetroPass.SettingsPages;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace RetroPass
{
	public sealed partial class SettingsPage : Page
	{
		DataSourceManager dataSourceManager;
		string AppVersion { get; set; }

		public SettingsPage()
		{
			var version = Package.Current.Id.Version;

			AppVersion = string.Format("v{0}.{1}.{2}", version.Major, version.Minor, version.Build);
			InitializeComponent();
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
			base.OnNavigatedTo(e);
			//select sources as default
			NavigationViewSettings.SelectedItem = NavigationViewSettings.MenuItems[0];		
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
			base.OnNavigatedFrom(e);
		}		
		
        private void NavigationViewSettings_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
			var item = args.SelectedItem as NavigationViewItem;
			string navTo = item.Tag.ToString();

			if (navTo != null)
			{
				switch (navTo)
				{
					case "SettingsPageDataSource":
						ContentFrame.Navigate(typeof(SettingsDataSourcePage), dataSourceManager);
						break;

					case "SettingsPagePersonalization":
						ContentFrame.Navigate(typeof(SettingsPersonalizationPage));
						break;

					case "SettingsPageLogging":
						ContentFrame.Navigate(typeof(SettingsLogPage));
						break;
				}
			}
		}
	}
}
