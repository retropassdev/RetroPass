using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace RetroPass
{
    public class PlaylistItem
    {
        public Playlist playlist { get; set; }
        public Game game;
        public BitmapImage bitmapImage { get; set; }
    }
}