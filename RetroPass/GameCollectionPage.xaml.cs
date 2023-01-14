using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

        public GameCollectionPage()
        {
            this.InitializeComponent();
            this.Loaded += GameCollectionPage_Loaded;
        }

        private void GameCollectionPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (PlatformGridView.ItemsSource == null || PlatformGridView.ItemsSource != playlist.PlaylistItems)
            {
                PlatformGridView.ItemsSource = playlist.PlaylistItems;
                NameCollection.Text = playlist.Name;
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
                args.Handled = true;
            }

            if (args.Phase == 0)
            {
                SetItemText(args.ItemContainer.ContentTemplateRoot);

                var templateRoot = args.ItemContainer.ContentTemplateRoot as Button;
                var image = (Image)templateRoot.FindName("ItemImage");
                args.RegisterUpdateCallback(ShowImage);
                args.Handled = true;
            }
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

                args.Handled = true;
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;

                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }

                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void Grid_GotFocus(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as DependencyObject;
            var childListView = FindChild<UIElement>(item, "GameNameDisplay");
            childListView.Visibility = Visibility.Visible;

            PlaylistItem playlistItem = PlatformGridView.SelectedItem as PlaylistItem;
            PlayLaterControl.UpdatePlayLaterControl(playlistItem, playlistPlayLater);
        }

        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as DependencyObject;
            var childListView = FindChild<UIElement>(item, "GameNameDisplay");
            childListView.Visibility = Visibility.Collapsed;
        }

        private async void PlatformGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as PlaylistItem;
            GameDetailsPage popup = new GameDetailsPage();
            popup.OnNavigatedTo(item);
            ContentDialogResult result = await popup.ShowAsync();
            popup.OnNavigatedFrom();
        }
    }
}
