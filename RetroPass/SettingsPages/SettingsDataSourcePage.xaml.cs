using System;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;

namespace RetroPass.SettingsPages
{
	public sealed partial class SettingsDataSourcePage : Page
	{
		DataSourceManager dataSourceManager;

		public SettingsDataSourcePage()
		{
			this.InitializeComponent();
			Loaded += SettingsPage_Loaded;
		}

		private void RefreshDataSourceUI()
		{
			if (dataSourceManager.HasDataSources())
			{
				foreach (var item in ListDataSources.Items)
				{
					ListViewItem listViewItem = ListDataSources.ContainerFromItem(item) as ListViewItem;
					if (listViewItem != null)
					{
						DataSource dataSource = item as DataSource;
						if (dataSource != null)
						{
							ToggleButton button = Utils.FindChild<ToggleButton>(listViewItem, "ButtonActivateRemovableStorage");
							if (button != null)
							{
								switch (dataSource.status)
								{
									case DataSource.Status.Active:
										button.Content = "Deactivate";
										button.IsChecked = true;
										break;
									case DataSource.Status.Inactive:
										button.Content = "Activate";
										button.IsChecked = false;
										break;
									case DataSource.Status.Unavailable:
										button.Content = "Unavailable";
										button.IsChecked = true;
										button.IsEnabled = false;
										break;
								}
							}
						}
					}
				}

				ListDataSources.Visibility = Visibility.Visible;
				ButtonClearRemovableCache.Visibility = Visibility.Visible;
			}
			else
			{
				ListDataSources.Visibility = Visibility.Collapsed;
				ButtonClearRemovableCache.Visibility = Visibility.Collapsed;
			}
		}

		private void RefreshSettingsUI()
		{
			if (dataSourceManager.HasDataSources() == false)
			{
				Hyperlink hyperlink = new Hyperlink();
				hyperlink.NavigateUri = new Uri("https://github.com/retropassdev/RetroPass#Setup");
				hyperlink.Inlines.Add(new Run() { Text = "github.com/retropassdev/RetroPass" });

				Run run1 = new Run() { Text = "RetroPass must be configured first. Follow the link for the setup guide:\n" };

				TextStatus.Inlines.Add(run1);
				TextStatus.Inlines.Add(hyperlink);
			}

			RefreshDataSourceUI();
		}

		private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
		{
			await dataSourceManager.ScanDataSources();
			RefreshSettingsUI();
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
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;

			base.OnNavigatedFrom(e);
		}

		private void ButtonActivateRemovableStorage_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton button = sender as ToggleButton;
			DataSource dataSourceItem = button.DataContext as DataSource;

			if ((bool)button.IsChecked == true)
			{
				dataSourceManager.UpdateDataSourceStatus(dataSourceItem.retroPassConfig.name, DataSource.Status.Active);
			}
			else
			{
				dataSourceManager.UpdateDataSourceStatus(dataSourceItem.retroPassConfig.name, DataSource.Status.Inactive);
			}

			RefreshDataSourceUI();
		}

		private async void ButtonClearRemovableCache_Click(object sender, RoutedEventArgs e)
		{
			dataSourceManager.DeleteUnavailableDataSources();
			await ThumbnailCache.Instance.Delete();
			RefreshSettingsUI();
		}

		private async void ButtonAddSource_Click(object sender, RoutedEventArgs e)
		{
			var folderPicker = new Windows.Storage.Pickers.FolderPicker();
			folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
			folderPicker.FileTypeFilter.Add("*");

			StorageFolder folder = await folderPicker.PickSingleFolderAsync();
			if (folder != null)
			{				
				SettingsAddDataSourcePage popup = new SettingsAddDataSourcePage(dataSourceManager,folder.Path);
				await popup.ShowAsync();
				
				if (popup.result == ContentDialogResult.Primary)
				{
					await dataSourceManager.ScanDataSources();
					RefreshSettingsUI();
				}
			}
		}
	}
}
