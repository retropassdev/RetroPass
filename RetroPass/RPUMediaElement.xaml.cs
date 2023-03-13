using Microsoft.Toolkit.Uwp.UI.Controls;
using RetroPass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetroPass_Ultimate
{
    public sealed partial class RPUMediaElement : UserControl
    {
        static string[] imageExt = new string[] { ".png", "jpg", "jpeg", ".webp" };
        static string[] videoExt = new string[] { ".mp4", ".MOV", ".webp" };

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
            DependencyProperty.Register("MediaPath", typeof(string), typeof(RPUMediaElement), new PropertyMetadata(string.Empty, OnMediaPathChangedCallBack));

        async private static void OnMediaPathChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RPUMediaElement element = sender as RPUMediaElement;
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

        public RPUMediaElement()
        {
            this.InitializeComponent();
        }
    }
}
