using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetroPass
{
	public sealed partial class PlayLaterControl : UserControl
	{
		public PlayLaterControl()
		{
			this.InitializeComponent();
		}

		public void UpdatePlayLaterControl(PlaylistItem playlistItem, PlaylistPlayLater playlistPlayLater)
		{
			if (playlistPlayLater != null && playlistItem != null)
			{
				OverlayPlayLater.Visibility = Visibility.Visible;

				if (playlistPlayLater.GameExists(playlistItem))
				{
					StatusText.Text = "Remove from Play later";
				}
				else
				{
					StatusText.Text = "Add to Play later";
				}
			}
			else
			{
				OverlayPlayLater.Visibility = Visibility.Collapsed;
			}
		}
	}
}
