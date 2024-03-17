using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RetroPass
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GameCollectionPage : Page
	{
		Playlist playlist;

		public class GameCollectionPageNavigateParams
		{
			public GameCollectionPageNavigateParams(PlaylistItem p1, Playlist p2, PlaylistPlayLater p3)
			{
				playlistItem = p1;
				playlist = p2;
				playlistPlayLater = p3;
			}

			public PlaylistItem playlistItem;
			public Playlist playlist;
			public PlaylistPlayLater playlistPlayLater;
		}

		PlaylistPlayLater playlistPlayLater;
		string currentLayoutMode = null;
		string currentImageStretch = null;

		public GameCollectionPage()
		{
			this.InitializeComponent();
			this.Loaded += GameCollectionPage_Loaded;
		}

		//This layout is default. Fixed dimensions width and height. Looks uniform but higher aspect ratio box art doesn't looks that good.
		private void SetFixedItemWidth(BitmapImage image)
		{
			this.Resources["PlaylistItemWidth"] = (int)this.Resources["InitialPlaylistItemWidth"];
			this.Resources["PlaylistItemMargin"] = (Thickness)this.Resources["InitialPlaylistItemMargin"];
		}

		//This layout puts images into categories of predefined widths, depending on their aspect ratio and fixed spacing.
		//Widths are predefined so that the number of images in one row perfectly fits available grid area while preserving
		//exact fixed space between rows and columns.
		//For example, images that fall between 0.99 and 1.33 aspect ratio, there will be 5 images in a row.
		private void SetApproximateAspectFixedSpacingItemWidth(BitmapImage image)
		{
			if (image != null)
			{
				float aspectRatio = (float)image.PixelWidth / image.PixelHeight;
				int height = (int)this.Resources["PlaylistItemHeight"];
				int width = (int)(height * aspectRatio);
				float finalAspectRatio = 136.0f / 210; //0.64, 6 images in a row

				//3 images in a row
				if (aspectRatio >= 280.0f / 210) //1.33
				{
					finalAspectRatio = 280.0f / 210;
				}
				//4 images in a row
				else if (aspectRatio >= 208.0f / 210) //0.99
				{
					finalAspectRatio = 208.0f / 210;
				}
				//5 images in a row
				else if (aspectRatio >= 164.0f / 210) //0.78
				{
					finalAspectRatio = 164.0f / 210;
				}

				width = (int)(finalAspectRatio * height);

				this.Resources["PlaylistItemWidth"] = width;
				this.Resources["PlaylistItemMargin"] = (Thickness)this.Resources["InitialPlaylistItemMargin"];
			}
		}

		//This layout calculates how many box art fits into one row and adjusts spacing accordingly.
		//It is similar to Predefined layout. Small difference is that predefined layout defines exact image dimensions
		//depending on the aspect ratio and has fixed space
		//For example: if the box art has aspect ratio of 0.90, in Predefined layout width would be shrinked to that of 
		//aspect ratio 0.78 and would result in having exactly 5 images in a row
		//Adaptive layout would display images at ratio 0.90, but since images would be wider, it would probably show only
		//4 images in a row.
		private void SetOriginalAspectFlexibleSpacingItemWidth(BitmapImage image)
		{
			if (image != null)
			{
				float aspectRatio = (float)image.PixelWidth / image.PixelHeight;

				int gridWidth = (int)this.Resources["GridWidth"];
				Thickness minMarginThickness = (Thickness)this.Resources["InitialPlaylistItemMargin"];
				int minMargin = (int)minMarginThickness.Left;
				int height = (int)this.Resources["PlaylistItemHeight"];
				int width = (int)(height * aspectRatio);

				//fit as much as possible into width
				float countF = gridWidth / (float)(width + minMargin * 2);
				int count = (int)countF;
				double variance = countF - Math.Truncate((double)countF);

				//this means the current width is very close to adding one more item into the row at the expense of smaller width
				if (variance >= 0.9)
				{
					count++;
					width = gridWidth / count - minMargin * 2;
				}

				//if aspect ratio is wider than the whole row, shrink width to the max row size
				if (count == 0)
				{
					count = 1;
					width = gridWidth - minMargin * 2;
				}

				//check what's left of available space
				int freeWidth = gridWidth - count * (width + minMargin * 2);
				//add free space to margins
				int moreMargin = freeWidth / (count * 2);

				Thickness marginSet = new Thickness(minMargin + moreMargin, 4, minMargin + moreMargin, 4); // Replace with the desired margins
				this.Resources["PlaylistItemWidth"] = width;
				this.Resources["PlaylistItemMargin"] = marginSet;
			}
		}

		private async void GameCollectionPage_Loaded(object sender, RoutedEventArgs e)
		{
			var game = playlist.PlaylistItems[0].game;

			BitmapImage thumb = await game.GetImageThumbnailAsync();
			string layoutMode = ApplicationData.Current.LocalSettings.Values[App.SettingsCollectionPageLayout] as string;
			string imageStretch = ApplicationData.Current.LocalSettings.Values[App.SettingsImageStretch] as string;

			if (layoutMode == App.SettingsCollectionPageLayoutType.ApproximateAspect.ToString())
			{
				SetApproximateAspectFixedSpacingItemWidth(thumb);
			}
			else if(layoutMode == App.SettingsCollectionPageLayoutType.OriginalAspect.ToString())
			{
				SetOriginalAspectFlexibleSpacingItemWidth(thumb);
			}
			else
			{
				SetFixedItemWidth(thumb);
			}

			this.Resources["PlaylistItemImageStretch"] = imageStretch;

			//reset layout if needed
			if (currentLayoutMode != layoutMode || currentImageStretch != imageStretch)
			{
				currentLayoutMode = layoutMode;
				currentImageStretch = imageStretch;
				PlatformGridView.ItemsSource = null;
			}

			if (PlatformGridView.ItemsSource == null || PlatformGridView.ItemsSource != playlist.PlaylistItems)
			{
				PlatformGridView.ItemsSource = playlist.PlaylistItems;
				NameCollection.Text = playlist.Name;
				//fix a subtle bug, change SelectedIndex to force selecting first button
				//seting only to 0, sometimes doesn't select item
				PlatformGridView.SelectedIndex = -1;
				await System.Threading.Tasks.Task.Delay(1);
				PlatformGridView.SelectedIndex = 0;
			}

			lock (imageloading)
			{
				tasksImage.Clear();
			}
		}

		protected async override void OnKeyDown(KeyRoutedEventArgs e)
		{
			switch (e.Key)
			{
				case VirtualKey.GamepadX:
				case VirtualKey.X:
					// Gamepad X button was pressed
					if (PlatformGridView.SelectedItem != null)
					{
						int lastIndex = PlatformGridView.SelectedIndex;
						PlaylistItem pl = PlatformGridView.SelectedItem as PlaylistItem;
						playlistPlayLater.UpdatePlaylistPlayLater(pl);
						PlayLaterControl.UpdatePlayLaterControl(pl, playlistPlayLater);
						//if this is item from playlistPlayLater, then currentListView is playLate list view
						//so set proper selected item in case of deletion
						if (pl.playlist == playlistPlayLater)
						{
							UpdateNumGamesText(playlistPlayLater.PlaylistItems.Count);
							PlatformGridView.SelectedIndex = Math.Min(lastIndex, playlistPlayLater.PlaylistItems.Count - 1);
						}
					}
					break;
				case VirtualKey.GamepadY:
				case VirtualKey.Y:
					var pl1 = new List<Playlist>();
					pl1.Add(playlist);
					SearchPage.Instance.OnNavigatedTo(pl1);
					await SearchPage.Instance.ShowAsync();
					SearchPage.Instance.OnNavigatedFrom();

					if (SearchPage.Instance.selectedPlaylistItem != null)
					{
						GameDetailsPage popupDetails = new GameDetailsPage();
						popupDetails.OnNavigatedTo(SearchPage.Instance.selectedPlaylistItem);
						await popupDetails.ShowAsync();
						popupDetails.OnNavigatedFrom();
					}
					break;
				case VirtualKey.GamepadMenu:
					if (PlatformGridView.SelectedItem != null)
					{
						int lastIndex = PlatformGridView.SelectedIndex;
						PlaylistItem pl = PlatformGridView.SelectedItem as PlaylistItem;
						GameDetailsPage.StartContent(pl);
					}
					break;
			}
			base.OnKeyDown(e);
		}

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
			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
			var param = e.Parameter as GameCollectionPageNavigateParams;

			if (param.playlist != null)
			{
				playlist = param.playlist;
			}

			if (PlatformGridView.ItemsSource == null || PlatformGridView.ItemsSource != playlist.PlaylistItems)
			{
				PlatformGridView.ItemsSource = null;
				PlatformGridView.SelectedIndex = -1;
			}

			UpdateNumGamesText(playlist.PlaylistItems.Count);
			playlistPlayLater = param.playlistPlayLater;

			base.OnNavigatedTo(e);
		}

		private void UpdateNumGamesText(int count)
		{
			NumGamesCollectionText.Text = count + " games in collection";
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			//CoreWindow.GetForCurrentThread().KeyDown -= OnKeyDown;
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
			base.OnNavigatedFrom(e);
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


		private void ItemsWrapGrid_BringIntoViewRequested(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			ScrollToCenter(sender, args);
		}

		public static object imageloading = new object();
		public static Dictionary<int, string> tasksImage = new Dictionary<int, string>();

		private void SetItemRecycled(UIElement element)
		{
			var templateRoot = element as Button;

			var image = (Image)templateRoot.FindName("ItemImage");
			image.Source = null;
			image.Opacity = 0;

			var GameNameDisplayEmpty = (UIElement)templateRoot.FindName("GameNameDisplayEmpty");
			GameNameDisplayEmpty.Visibility = Visibility.Collapsed;
			var GameNameDisplay = (UIElement)templateRoot.FindName("GameNameDisplay");
			GameNameDisplay.Visibility = Visibility.Collapsed;
		}

		private void SetItemText(UIElement element)
		{
			var templateRoot = element as Button;

			var image = (Image)templateRoot.FindName("ItemImage");
			image.Source = null;
			image.Opacity = 0;

			var GameNameDisplayEmpty = (UIElement)templateRoot.FindName("GameNameDisplayEmpty");
			GameNameDisplayEmpty.Visibility = Visibility.Visible;
			var GameNameDisplay = (UIElement)templateRoot.FindName("GameNameDisplay");
			GameNameDisplay.Visibility = Visibility.Collapsed;
		}

		private void SetItemImage(UIElement element)
		{
			var templateRoot = element as Button;

			var image = (Image)templateRoot.FindName("ItemImage");
			var GameNameDisplayEmpty = (UIElement)templateRoot.FindName("GameNameDisplayEmpty");
			GameNameDisplayEmpty.Visibility = Visibility.Collapsed;
			var GameNameDisplay = (UIElement)templateRoot.FindName("GameNameDisplay");
			GameNameDisplay.Visibility = Visibility.Collapsed;
			image.Opacity = 100;
		}

		private void GamesListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.InRecycleQueue)
			{
				SetItemRecycled(args.ItemContainer.ContentTemplateRoot);
				ShowItemOverlay(args.ItemContainer.ContentTemplateRoot, 1.0f);
				args.Handled = true;
			}

			if (args.Phase == 0)
			{
				SetItemText(args.ItemContainer.ContentTemplateRoot);
				ShowItemOverlay(args.ItemContainer.ContentTemplateRoot, 1.0f);
				args.RegisterUpdateCallback(ShowImage);
				args.Handled = true;
			}
		}

		private void ShowItemOverlay(UIElement item, float opacity)
		{
			var button = item as Button;
			var Overlay = (Border)button.FindName("GameOverlay");
			Overlay.Opacity = opacity;
		}

		private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.Phase == 1)
			{
				// It's phase 1, so show this item's image.
				var templateRoot = args.ItemContainer.ContentTemplateRoot as Button;
				var image = (Image)templateRoot.FindName("ItemImage");
				var item = args.Item as PlaylistItem;
				BitmapImage imageTmp = await item.game.GetImageThumbnailAsync();
				////Trace.TraceInformation("Loaded " + hash + " Image Name:" + imageName);

				//image.Opacity = 0;
				image.Source = imageTmp;
				if (image.Source != null)
				{
					SetItemImage(args.ItemContainer.ContentTemplateRoot);
				}
				else
				{
					SetItemText(args.ItemContainer.ContentTemplateRoot);
				}

				ShowItemOverlay(args.ItemContainer.ContentTemplateRoot, 0.0f);
				args.Handled = true;
			}
		}	

		private void Grid_GotFocus(object sender, RoutedEventArgs e)
		{
			var item = e.OriginalSource as DependencyObject;
			var childListView = Utils.FindChild<UIElement>(item, "GameNameDisplay");
			childListView.Visibility = Visibility.Visible;

			PlaylistItem playlistItem = PlatformGridView.SelectedItem as PlaylistItem;
			PlayLaterControl.UpdatePlayLaterControl(playlistItem, playlistPlayLater);
		}

		private void Grid_LostFocus(object sender, RoutedEventArgs e)
		{
			var item = e.OriginalSource as DependencyObject;
			var childListView = Utils.FindChild<UIElement>(item, "GameNameDisplay");
			if (childListView != null)
			{
				childListView.Visibility = Visibility.Collapsed;
			}
		}

		private void PlatformGridView_ItemClick(object sender, ItemClickEventArgs e)
		{
		}

		private async void GameButton_Click(object sender, RoutedEventArgs e)
		{
			var item = (sender as FrameworkElement)?.DataContext as PlaylistItem;
			GameDetailsPage popup = new GameDetailsPage();
			popup.OnNavigatedTo(item);
			popup.playlist = playlist;
			ContentDialogResult result = await popup.ShowAsync();
			popup.OnNavigatedFrom();
			//popup can change current item selection
			PlatformGridView.SelectedItem = popup.playlistItem;
		}
	}
}
