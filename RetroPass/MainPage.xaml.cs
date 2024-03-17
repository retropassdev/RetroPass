using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace RetroPass
{
	[Serializable, XmlRoot("dataSource")]
	public class RetroPassConfig
	{
		public enum DataSourceType
		{
			LaunchBox,
			EmulationStation,
		}

		public class Retroarch
		{
			public string romPath;
			[XmlArrayItemAttribute("dir", IsNullable = false)]
			public RetroarchDirMap[] dirMappings;
		}

		[SerializableAttribute()]
		public partial class RetroarchDirMap
		{
			[XmlAttributeAttribute()]
			public string src;
			[XmlAttributeAttribute()]
			public string dst;
		}

		public string name;
		public DataSourceType type;
		public string relativePath;
		public Retroarch retroarch;
	}

	[Serializable, XmlRoot("retropass")]
	public class RetroPassConfig_V1_6
	{
		[XmlAttribute("version")]
		public string Version = "1.6";

		[XmlArrayItem("dataSource", typeof(RetroPassConfig))]
		public List<RetroPassConfig> dataSources;
	}

	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		List<DataSource> loadedActiveDataSources = new List<DataSource>();
		PlaylistPlayLater playlistPlayLater = new PlaylistPlayLater();
		ListView currentListView;
		Button currentButton;
		PlaylistItem currentPlaylistItem;
		DataSourceManager dataSourceManager;
		StackPanel stackPanelPlayLater;
		bool activeDataSourcesChanged = false;
		string currentLayoutMode = null;

		protected async override void OnKeyDown(KeyRoutedEventArgs e)
		{
			switch (e.Key)
			{
				case VirtualKey.GamepadX:
				case VirtualKey.X:
					// Gamepad X button was pressed
					if (currentPlaylistItem != null)
					{
						ListViewItem listViewItem = (ListViewItem)currentListView.ContainerFromItem(currentPlaylistItem);
						int lastIndex = currentListView.IndexFromContainer(listViewItem);
						playlistPlayLater.UpdatePlaylistPlayLater(currentPlaylistItem);
						PlayLaterControl.UpdatePlayLaterControl(currentPlaylistItem, playlistPlayLater);
						//if this is item from playlistPlayLater, then currentListView is playLater list view
						//so set proper selected item in case of deletion
						if (currentPlaylistItem.playlist == playlistPlayLater)
						{
							currentListView.SelectedIndex = Math.Min(lastIndex, playlistPlayLater.PlaylistItemsLandingPage.Count - 1);

							if (playlistPlayLater.PlaylistItems.Count > 0)
							{
								currentPlaylistItem = playlistPlayLater.PlaylistItemsLandingPage[currentListView.SelectedIndex];
							}
							else
							{
								currentPlaylistItem = null;
							}
						}

						if (playlistPlayLater.PlaylistItems.Count == 0)
						{
							stackPanelPlayLater.Visibility = Visibility.Collapsed;
						}
						else
						{
							stackPanelPlayLater.Visibility = Visibility.Visible;
						}
					}
					break;

				case VirtualKey.GamepadY:
				case VirtualKey.Y:
					await ShowSearch();
					break;
				case VirtualKey.GamepadMenu:
					if (currentPlaylistItem != null)
					{
						GameDetailsPage.StartContent(currentPlaylistItem);
					}
					break;
			}
			base.OnKeyDown(e);
		}

		public MainPage()
		{
			this.InitializeComponent();
			this.Loaded += MainPage_Loaded;
			dataSourceManager = new DataSourceManager();
			dataSourceManager.ActiveDataSourcesChanged += ActiveDataSourceChanged;
			LogPage.SetLogging();
		}

		private void ActiveDataSourceChanged()
		{
			activeDataSourcesChanged = true;
		}

		private void ClearAll()
		{
			//clear all 
			foreach (var dataSource in loadedActiveDataSources)
			{
				dataSource.PlatformImported -= OnPlatformImported;
				dataSource.PlaylistImported -= OnPlaylistImported;
				dataSource.Platforms.Clear();
				dataSource.Playlists.Clear();
			}

			ClearMainPanel();
			loadedActiveDataSources.Clear();
			playlistPlayLater = new PlaylistPlayLater();
		}

		private async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			//SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
			//			   AppViewBackButtonVisibility.Collapsed;
			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

			var dataSourceItems = await dataSourceManager.GetPresentActiveDataSources();

			if (dataSourceItems.Count == 0)
			{
				if (activeDataSourcesChanged)
				{
					activeDataSourcesChanged = false;
					ClearAll();
				}

				Frame.Navigate(typeof(SettingsPage), dataSourceManager);
				return;
			}

			string layoutMode = ApplicationData.Current.LocalSettings.Values[App.SettingsMainPageLayout] as string;

			//load data source only the first time or when it's changed, or if layout is changed
			if (activeDataSourcesChanged || currentLayoutMode != layoutMode)
			{
				currentLayoutMode = layoutMode;
				activeDataSourcesChanged = false;

				ClearAll();

				await playlistPlayLater.Load(dataSourceItems);
				playlistPlayLater.UpdateGamesLandingPage();
				OnPlaylistImported(playlistPlayLater);

				foreach (var dataSource in dataSourceItems)
				{
					loadedActiveDataSources.Add(dataSource);
					dataSource.PlatformImported += OnPlatformImported;
					dataSource.PlaylistImported += OnPlaylistImported;
					await dataSource.Load();
				}
			}

			//focus on button only if getting from another page
			//this is to prevent setting focus after delayed load while search page is visible
			if (currentButton != null && SearchPage.Instance != null && SearchPage.Instance.IsOpened == false)
			{
				currentButton.Focus(FocusState.Programmatic);
			}

			if (playlistPlayLater.PlaylistItems.Count == 0)
			{
				stackPanelPlayLater.Visibility = Visibility.Collapsed;
			}
			else
			{
				stackPanelPlayLater.Visibility = Visibility.Visible;
			}
		}

		public static bool IsBackOrEscapeKey(Windows.System.VirtualKey key)
		{
			return key == VirtualKey.Back || key == VirtualKey.Escape || key == VirtualKey.GamepadB;
		}

		private void OnBackRequested(object sender, BackRequestedEventArgs e)
		{
			//int i = 5;
			//e.Handled = true;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
			base.OnNavigatedFrom(e);
		}

		private void ClearMainPanel()
		{
			var playlists = StackPanelMain.FindChildren().Where(t => t is ScrollViewer).ToList();

			for (int i = playlists.Count() - 1; i >= 0; i--)
			{
				StackPanelMain.Children.Remove(playlists[i]);
			}

			stackPanelPlayLater = null;
		}

		private void AddList(Playlist playlist, bool prepend)
		{
			string mainPageLayoutMode = ApplicationData.Current.LocalSettings.Values[App.SettingsMainPageLayout] as string;

			ListView listView = new ListView();
			listView.ItemTemplate = (DataTemplate)Resources["PlaylistItemTemplate"];
			listView.ItemsSource = playlist.PlaylistItemsLandingPage;

			if (mainPageLayoutMode == App.SettingsMainPageLayoutType.OriginalAspect.ToString())
			{
				listView.ItemsPanel = (ItemsPanelTemplate)Resources["OriginalAspectGamesListViewPanelTemplate"];
				listView.ItemContainerStyle = (Style)Resources["OriginalAspectListViewPlatformItemContainerTemplateStyle"];
			}
			else
			{
				listView.ItemsPanel = (ItemsPanelTemplate)Resources["GamesListViewPanelTemplate"];
				listView.ItemContainerStyle = (Style)Resources["ListViewPlatformItemContainerTemplateStyle"];
			}

			listView.ContainerContentChanging += GamesListView_ContainerContentChanging;
			listView.SelectionMode = ListViewSelectionMode.Single;
			listView.SingleSelectionFollowsFocus = false;
			listView.GotFocus += ListView_GotFocus;
			listView.IsItemClickEnabled = true;
			listView.ItemClick += OnItemClick;
			listView.SelectedIndex = 0;

			StackPanel stackPanel = new StackPanel();
			stackPanel.Style = (Style)Resources["StackPanelPlatformTemplateStyle"];
			Button button = new Button();
			button.DataContext = playlist;
			button.BringIntoViewRequested += Grid_BringIntoViewRequested;
			button.Style = (Style)Resources["PlatformFirstItemButtonStyle"];
			button.Click += Button_Click;
			button.GotFocus += Button_GotFocus;
			//First button doesn't have popup reveal, to prevent page transition glitch where focus is immediately
			//visible. But then elevate the index to appaear on top of other buttons 
			Canvas.SetZIndex(button, 10);

			stackPanel.Orientation = Orientation.Horizontal;
			stackPanel.Children.Add(button);
			stackPanel.Children.Add(listView);

			ScrollViewer scrollViewer = new ScrollViewer();
			scrollViewer.Content = stackPanel;
			//scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
			scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

			if (mainPageLayoutMode == App.SettingsMainPageLayoutType.OriginalAspect.ToString())
			{
				scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
			}
			else
			{
				scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
			}

			//remember playlater stack panel so it can be hidden when there is nothing in the list
			if (playlist is PlaylistPlayLater)
			{
				stackPanelPlayLater = stackPanel;
				//stackPanelPlayLater.Visibility = Visibility.Collapsed;

				if (playlist.PlaylistItems.Count == 0)
				{
					stackPanelPlayLater.Visibility = Visibility.Collapsed;
				}
				else
				{
					stackPanelPlayLater.Visibility = Visibility.Visible;
				}
			}

			if (prepend)
			{
				StackPanelMain.Children.Insert(0, scrollViewer);
			}
			else
			{
				StackPanelMain.Children.Add(scrollViewer);
			}
		}

		private void ScrollPlatformListViewToLeft(ListView listView)
		{
			if (listView != null && listView.Items.Count > 0)
			{
				var scrollViewer = currentListView.FindAscendant<ScrollViewer>();
				if (scrollViewer != null)
				{
					scrollViewer.ChangeView(0, null, null);
				}
			}
		}

		private void Button_GotFocus(object sender, RoutedEventArgs e)
		{
			PlayLaterControl.UpdatePlayLaterControl(null, null);
			//scroll back current list view to start before setting new list view
			ScrollPlatformListViewToLeft(currentListView);

			currentButton = sender as Button;
			currentPlaylistItem = null;
			currentListView = null;
		}

		private void ListView_GotFocus(object sender, RoutedEventArgs e)
		{
			ListView lv = sender as ListView;
			currentButton = null;

			if (currentListView != lv)
			{
				//scroll back current list view to start before setting new list view
				ScrollPlatformListViewToLeft(currentListView);
				currentListView = lv;
			}

			//PlaylistItem playlistItem = current as PlaylistItem;
			PlayLaterControl.UpdatePlayLaterControl(currentPlaylistItem, playlistPlayLater);
		}

		private void PlaylistItem_GotFocus(object sender, RoutedEventArgs e)
		{
			var focusedButton = sender as Button;
			currentPlaylistItem = focusedButton.DataContext as PlaylistItem;
		}

		private void OnPlaylistImported(Playlist playlist)
		{
			AddList(playlist, false);
		}

		private void OnPlatformImported(Platform platform)
		{
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			//remember currently focused element
			FocusManager.GetFocusedElement();

			Button b = sender as Button;
			//currentButton = b;
			Playlist p = b.DataContext as Playlist;

			await Task.Delay(10);
			this.Frame.Navigate(typeof(GameCollectionPage), new GameCollectionPage.GameCollectionPageNavigateParams(null, p, playlistPlayLater));
		}

		private void ScrollToCenter(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			if (args.VerticalAlignmentRatio != 0.5)  // Guard against our own request
			{
				args.Handled = true;
				// Swallow this request and restart it with a request to center the item.  We could instead have chosen
				// to adjust the TargetRect’s Y and Height values to add a specific amount of padding as it bubbles up, 
				// but if we just want to center it then this is easier.

				// (Optional) Account for sticky headers if they exist
				var headerOffset = 0.0;

				// Issue a new request
				args.TargetElement.StartBringIntoView(new BringIntoViewOptions()
				{
					AnimationDesired = true,
					VerticalAlignmentRatio = 0.5, // a normalized alignment position (0 for the top, 1 for the bottom)
					VerticalOffset = headerOffset, // applied after meeting the alignment ratio request
				});
			}
		}

		private void Grid_BringIntoViewRequested(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			ScrollToCenter(sender, args);
		}

		// The BringIntoViewRequested event is raised by the framework when items receive keyboard (or Narrator) focus or 
		// someone triggers it with a call to UIElement.StartBringIntoView.

		private void PlatformItemsWrapGrid_BringIntoViewRequested(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			//currentButton = args.OriginalSource as FrameworkElement;
			ScrollToCenter(sender, args);
		}

		private async void OnItemClick(object sender, ItemClickEventArgs e)
		{
			/*var item = e.ClickedItem as PlaylistItem;
			GameDetailsPage popup = new GameDetailsPage();
			popup.OnNavigatedTo(item);
			await popup.ShowAsync();
			popup.OnNavigatedFrom();*/
		}

		private async void PlaylistItem_Click(object sender, RoutedEventArgs e)
		{
			var item = (sender as FrameworkElement)?.DataContext as PlaylistItem;
			GameDetailsPage popup = new GameDetailsPage();
			popup.OnNavigatedTo(item);
			popup.playlist = item.playlist;
			await popup.ShowAsync();
			popup.OnNavigatedFrom();
		}

		private void GamesListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.InRecycleQueue)
			{
				var templateRoot = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
				if (templateRoot != null)
				{
					var image = (Image)templateRoot.FindName("ItemImage");

					image.Source = null;
				}
				args.Handled = true;
			}

			if (args.Phase == 0)
			{
				args.RegisterUpdateCallback(ShowImage);
				args.Handled = true;
			}
		}

		private async Task ShowSearch()
		{
			//search only platform playlists and not custom playlists
			//List<Playlist> searchPlaylists = dataSource.Playlists.FindAll(t => dataSource.Platforms.Exists(p => p.Name == t.Name));

			/*List<Playlist> searchPlaylists = new List<Playlist>();

			foreach (DataSource dataSource in loadedDataSources)
			{
				searchPlaylists.AddRange(dataSource.Playlists.FindAll(t => dataSource.Platforms.Any(p => p.Name == t.Name)));
			}*/

			List<Playlist> searchPlaylists = loadedActiveDataSources.SelectMany(ds => ds.Playlists.Where(pl => ds.Platforms.Any(p => p.Name == pl.Name))).ToList();
			SearchPage.Instance.OnNavigatedTo(searchPlaylists);
			await SearchPage.Instance.ShowAsync();
			SearchPage.Instance.OnNavigatedFrom();

			if (SearchPage.Instance.selectedPlaylistItem != null)
			{
				GameDetailsPage popupDetails = new GameDetailsPage();
				popupDetails.OnNavigatedTo(SearchPage.Instance.selectedPlaylistItem);
				await popupDetails.ShowAsync();
				popupDetails.OnNavigatedFrom();
			}
		}

		private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.Phase == 1)
			{
				// It's phase 1, so show this item's image.
				var templateRoot = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
				var image = (Image)templateRoot.FindName("ItemImage");
				image.Opacity = 100;

				var item = args.Item as PlaylistItem;

				image.Source = await item.game.GetImageThumbnailAsync();

				if (image.Source == null)
				{
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.UriSource = new Uri(image.BaseUri, "Assets/empty.png");
					image.Source = bitmapImage;
				}
			}
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(SettingsPage), dataSourceManager);
		}

		private async void Search_Click(object sender, RoutedEventArgs e)
		{
			await ShowSearch();
		}
	}
}
