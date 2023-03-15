using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Xaml;

namespace RetroPass
{

    [XmlRoot(ElementName = "Background")]
    public class Background
    {
        [XmlElement(ElementName = "Page")]
        public string Page { get; set; }

        [XmlElement(ElementName = "File")]
        public string File { get; set; }
    }

    [XmlRoot(ElementName = "Backgrounds")]
    public class Backgrounds
    {
        [XmlElement(ElementName = "Background")]
        public List<Background> Background { get; set; }
    }

    [XmlRoot(ElementName = "LaunchPass")]
    public class LaunchPassThemeSettings
    {
        [XmlElement(ElementName = "Backgrounds")]
        public Backgrounds Backgrounds { get; set; }

        [XmlElement(ElementName = "Font")]
        public string Font { get; set; }

        [XmlElement(ElementName = "BoxArtType")]
        public string BoxArtType { get; set; }


        public string GetMediaPath(string PageName)
        {
            string path = string.Empty;

            if (!string.IsNullOrEmpty(PageName))
            {
                path = Path.Combine(((App)Application.Current).RetroPassRootPath, "Backgrounds",
                    ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == PageName).File);
            }
            return path;
        }

        public string GetFontFilePath()
        {
            return Path.Combine(((App)Application.Current).RetroPassRootPath, "Fonts", ((App)Application.Current).CurrentThemeSettings.Font);
        }
    }
}

