using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

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
		private List<string> searchGenreList = new List<string>();

		private static string genreAll = "All";
		private static string genreNone = "None";

		public SearchPage()
		{
			this.InitializeComponent();
			RequestedTheme = ThemeManager.Instance.CurrentMode;
		}

		protected override void OnPreviewKeyDown(KeyRoutedEventArgs e)
		{
			switch (e.Key)
			{
				case VirtualKey.GamepadB:
				case VirtualKey.Escape:
					selectedPlaylistItem = null;
					break;
			}
			base.OnPreviewKeyDown(e);
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
			if (RequestedTheme != ThemeManager.Instance.CurrentMode)
			{
				RequestedTheme = ThemeManager.Instance.CurrentMode;
			}

			if (SearchCriteria.SelectedItem == null)
			{
				SearchCriteria.SelectedItem = SearchCriteria.MenuItems[0];
			}
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
			if (playlists == null)
			{
				return;
			}

			string searchCriteria = (SearchCriteria.SelectedItem as NavigationViewItem).Tag.ToString();

			if (searchCriteria == "Genre")
			{
				SearchText.Visibility = Visibility.Collapsed;
				SearchGenre.Visibility = Visibility.Visible;

				var genres = playlists.SelectMany(p => p.PlaylistItems).SelectMany(i => i.game.Genre.Split(',')).Select(g => g.Split('/')[0].Trim()).Where(g => !string.IsNullOrEmpty(g)).Distinct().OrderBy(g => g).ToList();
				//searchGenreList contains previously selected genre. It's useful to keep those checked between searches.
				//keep any genre that exists in currently selected list
				searchGenreList.RemoveAll(t => genres.Contains(t) == false);

				StackPanelGenre.Children.Clear();

				ToggleButton toggleButton = new ToggleButton();
				toggleButton.Content = genreAll;
				toggleButton.Tag = genreAll;
				toggleButton.Checked += ToggleButtonGenre_Checked;
				toggleButton.Unchecked += ToggleButtonGenre_Unchecked;
				toggleButton.XYFocusDown = SearchCriteria;
				StackPanelGenre.Children.Add(toggleButton);


				toggleButton = new ToggleButton();
				toggleButton.Content = genreNone;
				toggleButton.Tag = genreNone;
				toggleButton.Checked += ToggleButtonGenre_Checked;
				toggleButton.Unchecked += ToggleButtonGenre_Unchecked;
				toggleButton.XYFocusDown = SearchCriteria;
				StackPanelGenre.Children.Add(toggleButton);

				foreach (var genre in genres)
				{
					toggleButton = new ToggleButton();
					toggleButton.Content = genre;
					toggleButton.Tag = genre;
					toggleButton.IsChecked = searchGenreList.Contains(genre);
					toggleButton.Checked += ToggleButtonGenre_Checked;
					toggleButton.Unchecked += ToggleButtonGenre_Unchecked;
					toggleButton.XYFocusDown = SearchCriteria;
					StackPanelGenre.Children.Add(toggleButton);
				}

				ToggleButton first = StackPanelGenre.Children.First() as ToggleButton;
				ToggleButton last = StackPanelGenre.Children.Last() as ToggleButton;

				first.XYFocusLeftNavigationStrategy = XYFocusNavigationStrategy.RectilinearDistance;
				last.XYFocusRightNavigationStrategy = XYFocusNavigationStrategy.RectilinearDistance;
				first.XYFocusLeft = last;
				last.XYFocusRight = first;

				RefreshGenreSearch();

			}
			else if (searchText.Length > 2)
			{
				SearchText.Visibility = Visibility.Visible;
				SearchGenre.Visibility = Visibility.Collapsed;

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
				SearchText.Visibility = Visibility.Visible;
				SearchGenre.Visibility = Visibility.Collapsed;

				searchResultList.Clear();
			}
		}

		private void ToggleButtonGenre_Unchecked(object sender, RoutedEventArgs e)
		{
			string genre = ((ToggleButton)sender).Tag.ToString();

			if (genre == genreAll || genre == genreNone)
			{
				//ignore
			}
			else if (searchGenreList.Contains(genre) == true)
			{
				searchGenreList.Remove(genre);
				RefreshGenreSearch();
			}
		}

		private void ToggleButtonGenre_Checked(object sender, RoutedEventArgs e)
		{
			string genre = ((ToggleButton)sender).Tag.ToString();

			if (genre == genreAll)
			{
				searchGenreList.Clear();

				foreach (ToggleButton toggleButton in StackPanelGenre.Children)
				{
					if (toggleButton.Tag.ToString() == genreNone || toggleButton.Tag.ToString() == genreAll)
					{
						toggleButton.IsChecked = false;
					}
					else
					{
						toggleButton.IsChecked = true;
						searchGenreList.Add(toggleButton.Tag.ToString());
					}
				}
			}
			else if (genre == genreNone)
			{
				searchGenreList.Clear();

				foreach (ToggleButton toggleButton in StackPanelGenre.Children)
				{
					toggleButton.IsChecked = false;
				}
			}
			else if (searchGenreList.Contains(genre) == false)
			{
				searchGenreList.Add(genre);
			}

			RefreshGenreSearch();
		}

		private void RefreshGenreSearch()
		{
			var playlistItems = playlists.SelectMany(p => p.PlaylistItems).Where(t => t.game.Genre != null && searchGenreList.Any(g => t.game.Genre.Contains(g, StringComparison.InvariantCultureIgnoreCase))).ToList();
			searchResultList.Clear();
			foreach (PlaylistItem i in playlistItems)
			{
				searchResultList.Add(i);
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

		private void SearchCriteria_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			UpdateSearchResults(SearchText.Text);
		}
	}
}
