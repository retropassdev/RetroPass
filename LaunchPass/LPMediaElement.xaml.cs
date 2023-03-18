using RetroPass;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LaunchPass
{
    public sealed partial class LPMediaElement : UserControl
    {
        private static string[] imageExt = new string[] { ".png", "jpg", "jpeg", ".webp" };
        private static string[] videoExt = new string[] { ".mp4", ".MOV", ".webp" };

        public string MediaPath
        {
            get
            {
                return (string)GetValue(MediaPathProperty);
            }
            set
            {
                SetValue(MediaPathProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MediaPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaPathProperty =
            DependencyProperty.Register("MediaPath", typeof(string), typeof(LPMediaElement), new PropertyMetadata(string.Empty, OnMediaPathChangedCallBack));

        private static async void OnMediaPathChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            LPMediaElement element = sender as LPMediaElement;
            string path = Convert.ToString(e.NewValue);

            if (element != null && !string.IsNullOrEmpty(path))
            {
                string extension = Path.GetExtension(path);

                if (imageExt.Contains(extension))
                {
                    element.image1.Source = await ThumbnailCache.Instance.GetImageFromPath(path);
                    element.mediaElement1.Source = null;

                    element.image1.Visibility = Visibility.Visible;
                    element.mediaElement1.Visibility = Visibility.Collapsed;
                }
                else if (videoExt.Contains(extension))
                {
                    element.image1.Source = null;

                    var file = await StorageFile.GetFileFromPathAsync(path);
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    element.mediaElement1.SetSource(stream, file.ContentType);

                    element.image1.Visibility = Visibility.Collapsed;
                    element.mediaElement1.Visibility = Visibility.Visible;
                }
            }
        }

        public LPMediaElement()
        {
            this.InitializeComponent();
        }
    }
}