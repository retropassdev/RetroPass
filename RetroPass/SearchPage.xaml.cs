using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace RetroPass
{
	public sealed partial class SearchPage : ContentDialog
	{
		//need search functionality in several different pages, but also want to keep search results accross pages for quicker navigation
		//so implement this as a singleton
		private static SearchPage instance = null;

		public static SearchPage Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new SearchPage();
				}
				return instance;
			}
		}

		public bool IsOpened { get; private set; }
		public PlaylistItem selectedPlaylistItem;
		private List<Playlist> playlists;
		private ObservableCollection<PlaylistItem> searchResultList = new ObservableCollection<PlaylistItem>();
		private bool firstFocus;

		public SearchPage()
		{
			this.InitializeComponent();
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			//set focus on text box when search box is opened
			if (firstFocus)
			{
				SearchText.Focus(FocusState.Programmatic);
				firstFocus = false;
			}
			base.OnGotFocus(e);
		}

		public void OnNavigatedTo(List<Playlist> playlists)
		{
			firstFocus = true;
			IsOpened = true;
			SearchGridView.ItemsSource = searchResultList;

			//check if we need to update search results
			if (this.playlists != null)
			{
				var firstNotSecond = this.playlists.Except(playlists).ToList();
				var secondNotFirst = playlists.Except(this.playlists).ToList();

				if (firstNotSecond.Count > 0 || secondNotFirst.Count > 0)
				{
					this.playlists = playlists;
					selectedPlaylistItem = null;
					UpdateSearchResults(SearchText.Text);
				}
			}
			else
			{
				this.playlists = playlists;
				selectedPlaylistItem = null;
			}

			this.Closing += OnClosing;
		}

		public void OnNavigatedFrom()
		{
			this.Closing -= OnClosing;
		}

		private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
		{
			firstFocus = false;
			IsOpened = false;
			//args.Cancel = true;
		}

		private void UpdateSearchResults(string searchText)
		{
			if (searchText.Length > 2)
			{
				string searchCriteria = SearchCriteria.SelectedValue as string;
				switch (searchCriteria)
				{
					case "Title":
						var playlistItems = playlists.SelectMany(p => p.PlaylistItems).Where(t => t.game.Title != null && t.game.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
						searchResultList.Clear();
						foreach (PlaylistItem i in playlistItems)
						{
							searchResultList.Add(i);
						}
						break;
					case "Developer":
						playlistItems = playlists.SelectMany(p => p.PlaylistItems).Where(t => t.game.Developer != null && t.game.Developer.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
						searchResultList.Clear();
						foreach (PlaylistItem i in playlistItems)
						{
							searchResultList.Add(i);
						}
						break;
					case "Year":
						playlistItems = playlists.SelectMany(p => p.PlaylistItems).Where(t => t.game.ReleaseDate != null && t.game.ReleaseDate.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
						searchResultList.Clear();
						foreach (PlaylistItem i in playlistItems)
						{
							searchResultList.Add(i);
						}
						break;
					default:
						break;
				}
			}
			else
			{
				searchResultList.Clear();
			}
		}

		private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//checks if typing in progress
			async Task<bool> TypingInProgress()
			{
				string txt = SearchText.Text;
				await Task.Delay(500);
				return txt != SearchText.Text;
			}
			if (await TypingInProgress()) return;

			//stopped typing
			UpdateSearchResults(SearchText.Text);
		}

		private void SearchGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.InRecycleQueue)
			{
				var templateRoot = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
				var image = (Image)templateRoot.FindName("ItemImage");

				var button = (Button)templateRoot.FindName("GameButton");
				button.DataContext = null;

				image.Source = null;
				image.Opacity = 0;
				args.Handled = true;
			}

			if (args.Phase == 0)
			{
				var templateRoot = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
				var image = (Image)templateRoot.FindName("ItemImage");
				image.Source = null;
				image.Opacity = 0;

				var button = (Button)templateRoot.FindName("GameButton");
				button.DataContext = args.Item as PlaylistItem;
				args.RegisterUpdateCallback(ShowImage);
				args.Handled = true;
			}
		}

		private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.Phase == 1)
			{
				var item = args.Item as PlaylistItem;
				var bitmapImage = await item.game.GetImageThumbnailAsync();

				//Trace.TraceInformation("ShowImage " + item.game.BoxFrontFileName);

				// It's phase 1, so show this item's image.				
				var templateRoot = args.ItemContainer.ContentTemplateRoot as FrameworkElement;
				var image = (Image)templateRoot.FindName("ItemImage");

				image.Opacity = 0;
				image.Source = bitmapImage;
				if (image.Source == null)
				{
					bitmapImage = new BitmapImage();
					bitmapImage.UriSource = new Uri(image.BaseUri, "Assets/empty.png");
					image.Source = bitmapImage;
				}
				image.Opacity = 100;
				args.Handled = true;
			}
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

		private void GameButton_Click(object sender, RoutedEventArgs e)
		{
			Button b = sender as Button;
			selectedPlaylistItem = b.DataContext as PlaylistItem;
			Hide();
		}

		private void DummyReturnFocusTextBox_GettingFocus(UIElement sender, Windows.UI.Xaml.Input.GettingFocusEventArgs args)
		{
			//When there are a lot of search items in SearchGridView, navigating to the items in the last row will move focus to
			//underlying controls in the MainPage or GameCollectionPage.
			//To prevent that, a dummy text box exists below SearchGridView to catch focus and cancel it.
			args.TryCancel();
		}

        private void SearchGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
