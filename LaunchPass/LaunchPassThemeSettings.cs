using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Windows.UI.Xaml;

/// <summary>
/// The RetroPass name-space contains classes for handling theme settings in this UWP application.
/// </summary>
namespace RetroPass
{
    /// Represents a single background image configuration for a specific page.
    [XmlRoot(ElementName = "Background")]
    public class Background
    {
        /// Gets or sets the page associated with the background image.
        [XmlElement(ElementName = "Page")]
        public string Page { get; set; }

        /// Gets or sets the file path of the background image.
        [XmlElement(ElementName = "File")]
        public string File { get; set; }
    }

    /// Represents a collection of background image configurations.
    [XmlRoot(ElementName = "Backgrounds")]
    public class Backgrounds
    {
        /// Gets or sets the list of background image configurations.
        [XmlElement(ElementName = "Background")]
        public List<Background> Background { get; set; }
    }

    /// Represents the theme settings for the LaunchPass application.
    [XmlRoot(ElementName = "LaunchPass")]
    public class LaunchPassThemeSettings
    {
        /// Gets or sets the background image configurations.
        [XmlElement(ElementName = "Backgrounds")]
        public Backgrounds Backgrounds { get; set; }

        /// Gets or sets the font used in the theme.
        [XmlElement(ElementName = "Font")]
        public string Font { get; set; }

        /// Gets or sets the box art type for the theme.
        [XmlElement(ElementName = "BoxArtType")]
        public string BoxArtType { get; set; }

        /// Retrieves the media path for the specified page name.
        /// name="PageName" The name of the page for which to retrieve the media path.</param>
        /// <returns>The media path as a string.</returns>
        public string GetMediaPath(string PageName)
        {
            string path = string.Empty;

            if (!string.IsNullOrEmpty(PageName))
            {
                path = Path.Combine(((App)Application.Current).LaunchPassRootPath, "Backgrounds",
                    ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == PageName).File);
            }
            return path;
        }

        /// Retrieves the font file path for the current theme settings.
        /// <returns>The font file path as a string.</returns>
        public string GetFontFilePath()
        {
            return Path.Combine(((App)Application.Current).LaunchPassRootPath, "Fonts", ((App)Application.Current).CurrentThemeSettings.Font);
        }
    }
}