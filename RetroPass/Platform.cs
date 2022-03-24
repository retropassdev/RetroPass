using System.Xml.Serialization;
using Windows.Storage;

namespace RetroPass
{
	public class Platform
	{
		public string Name { get; set; }
		public string SourceName { get; set; }
		public string BoxFrontPath { get; set; }
		public string ScreenshotGameTitlePath { get; set; }
		public string ScreenshotGameplayPath { get; set; }
		public string ScreenshotGameSelectPath { get; set; }
		public string VideoPath { get; set; }

		[XmlIgnoreAttribute]
		public StorageFolder BoxFrontFolder { get; set; }

		public Platform Copy()
		{
			return (Platform)this.MemberwiseClone();
		}
	}
}
