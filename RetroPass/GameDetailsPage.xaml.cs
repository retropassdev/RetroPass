using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Image = Windows.UI.Xaml.Controls.Image;

namespace RetroPass
{
	public class DetailImage
	{
		public BitmapImage image { get; set; }
		public string path { get; set; }
	}

	public sealed partial class GameDetailsPage : ContentDialog
	{
		public PlaylistItem playlistItem;
		private bool detailsPopupActive = false;
		private bool descriptionPopupActive = false;
		public string Subtitle { get; set; }
		private MediaSource mediaSource;

		public GameDetailsPage()
		{
			this.InitializeComponent();
			mediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
			RequestedTheme = ThemeManager.Instance.CurrentMode;
		}

		protected override void OnPreviewKeyDown(KeyRoutedEventArgs e)
		{
			if (ImageOverlay.Visibility == Visibility.Visible)
			{
				switch (e.Key)
				{
					case VirtualKey.GamepadLeftThumbstickLeft:
					case VirtualKey.GamepadDPadLeft:
					case VirtualKey.Left:
						
						//navigate to previous image
						int index = IndexOfImageInGridView(OverlayImage);

						if (index != -1)
						{
							int newIndex = Math.Max(index - 1, 0);
							RefreshImage(newIndex);
							e.Handled = true;
						}
						break;
					case VirtualKey.GamepadLeftThumbstickRight:
					case VirtualKey.GamepadDPadRight:
					case VirtualKey.Right:

						//navigate to next image
						index = IndexOfImageInGridView(OverlayImage);

						if (index != -1)
						{
							int newIndex = Math.Min(index + 1, GameDetailsGridView.Items.Count - 1);
							RefreshImage(newIndex);
							e.Handled = true;
						}
						break;
				}
			}

			base.OnPreviewKeyDown(e);
		}

		private void RefreshImage(int index)
		{
			var imageList = GameDetailsGridView.ItemsSource as ObservableCollection<DetailImage>;
			OverlayImageLeft.Visibility = index == 0 ?
									OverlayImageLeft.Visibility = Visibility.Collapsed :
									OverlayImageLeft.Visibility = Visibility.Visible;
			OverlayImageRight.Visibility = index == imageList.Count - 1 ?
									OverlayImageRight.Visibility = Visibility.Collapsed :
									OverlayImageRight.Visibility = Visibility.Visible;
			OverlayImage.Source = imageList[index].image;
		}

		private void GameDetailsPage_LosingFocus(UIElement sender, LosingFocusEventArgs args)
		{
			if (ImageOverlay.Visibility == Visibility.Visible ||
				DescriptionOverlay.Visibility == Visibility.Visible)
			{
				args.Cancel = true;
			}
		}

		public void OnNavigatedTo(PlaylistItem playlistItem)
		{
			this.Closing += OnGameDetailsClosing;
			this.LosingFocus += GameDetailsPage_LosingFocus;

			this.playlistItem = playlistItem;
			Game game = playlistItem.game;
			GetDetailsImages();

			DateTime dt;
			string date = "";
			if (DateTime.TryParse(game.ReleaseDate, out dt))
			{
				date = dt.Year.ToString();
			}
			string[] arr = { game.Developer, game.Publisher, date, game.Genre };
			arr = Array.FindAll(arr, t => string.IsNullOrEmpty(t) == false);
			Subtitle = string.Join(" · ", arr);
		}

		public void OnNavigatedFrom()
		{
			this.LosingFocus -= GameDetailsPage_LosingFocus;
			this.Closing -= OnGameDetailsClosing;
		}

		private void OnGameDetailsClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
		{
			if (detailsPopupActive == true)
			{
				detailsPopupActive = false;
				ImageOverlay.Visibility = Visibility.Collapsed;
				args.Cancel = true;
			}
			else if (descriptionPopupActive == true)
			{
				descriptionPopupActive = false;
				DescriptionOverlay.Visibility = Visibility.Collapsed;
				args.Cancel = true;
			}
			else if (mediaSource != null)
			{
				mediaPlayerElement.MediaPlayer.Pause();
				mediaPlayerElement.Source = null;
				mediaSource.Dispose();
				mediaSource = null;
			}

			//ButtonDescription.Visibility = Visibility.Collapsed;
		}

		private async void GetDetailsImages()
		{
			Game game = playlistItem.game;

			ButtonDescription.Visibility = Visibility.Collapsed;
			ButtonVideo.Visibility = Visibility.Collapsed;

			/////////////////////////////////////search for video/////////////////////////////////
			mediaSource = await game.GetVideo();
			mediaPlayerElement.Source = mediaSource;

			if (mediaPlayerElement.Source != null)
			{
				mediaPlayerElement.MediaPlayer.Play();
				if ((bool)ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo] == false)
				{
					mediaPlayerElement.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(0.7);
					mediaPlayerElement.MediaPlayer.Pause();
				}
				ButtonVideo.Visibility = Visibility.Visible;
			}

			if (string.IsNullOrEmpty(game.Description) == false)
			{
				ButtonDescription.Visibility = Visibility.Visible;
			}

			/////////////////////////////////////get box image/////////////////////////////////
			ItemImage.Source = await game.GetMainImageAsync();

			/////////////////////////////////////get all detail images/////////////////////////////////
			GameDetailsGridView.Visibility = Visibility.Visible;
			GameDetailsGridView.ItemsSource = null;
			ObservableCollection<DetailImage> imageList = new ObservableCollection<DetailImage>();
			GameDetailsGridView.ItemsSource = imageList;
			await game.GetDetailsImages(imageList);
		}

		private void ScrollToCenter(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			if (args.HorizontalAlignmentRatio != 0.5)  // Guard against our own request
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
					HorizontalAlignmentRatio = 0.5, // a normalized alignment position (0 for the top, 1 for the bottom)
					HorizontalOffset = headerOffset, // applied after meeting the alignment ratio request
				});
			}
		}

		private void StackPanel_BringIntoViewRequested(UIElement sender, BringIntoViewRequestedEventArgs args)
		{
			ScrollToCenter(sender, args);
		}

		private int IndexOfImageInGridView(Image image)
		{
			int index = -1;
			var collection = GameDetailsGridView.ItemsSource as ObservableCollection<DetailImage>;
			var imageInList = collection.FirstOrDefault(t => t.image == image.Source);


			if (imageInList != null)
			{
				index = collection.IndexOf(imageInList);
			}

			return index;
		}

		private void ButtonDetail_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			Image image = button.Content as Image;
			int index = IndexOfImageInGridView(image);

			if (index != -1)
			{
				RefreshImage(index);
			}

			detailsPopupActive = true;
			ImageOverlay.Visibility = Visibility.Visible;
		}

		private void ButtonDescription_Click(object sender, RoutedEventArgs e)
		{
			descriptionPopupActive = true;
			DescriptionOverlay.Visibility = Visibility.Visible;
		}

		private void ButtonVideo_Click(object sender, RoutedEventArgs e)
		{
			if (mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
			{
				mediaPlayerElement.MediaPlayer.Pause();
			}
			else
			{
				mediaPlayerElement.MediaPlayer.Play();
			}
		}

		public async static Task StartContent(PlaylistItem playlistItem)
		{
			var game = playlistItem.game;
			playlistItem.playlist.SetLastPlayed(playlistItem);

			//check if it exists
			try
			{
				StorageFile file = await StorageFile.GetFileFromPathAsync(game.ApplicationPathFull);
				string urlScheme = UrlSchemeGenerator.GetUrl(game);
				Uri uri = new Uri(urlScheme);
				Trace.TraceInformation("GameDetailsPage: LaunchUriAsync: {0}", uri.ToString());
				Launcher.LaunchUriAsync(uri);
			}
			catch (Exception)
			{
				Trace.TraceError("GameDetailsPage: Content not found: {0}", game.ApplicationPathFull);
			}
		}

		private async void ButtonPlay_Click(object sender, RoutedEventArgs e)
		{
			//stop playing media
			if (mediaSource != null)
			{
				mediaPlayerElement.MediaPlayer.Pause();
			}

			await StartContent(playlistItem);
			Hide();
		}
	}
}