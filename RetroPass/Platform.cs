using System.Xml.Serialization;
using Windows.Storage;

namespace RetroPass
{
	public class Platform
	{
		public enum EEmulatorType
		{
			retroarch,
			rgx,
			xbsx2,
		}
		public string Name { get; set; }
		public string SourceName { get; set; }
		public EEmulatorType EmulatorType { get; set; }
		public string BoxFrontPath { get; set; }
		public string ScreenshotGameTitlePath { get; set; }
		public string ScreenshotGameplayPath { get; set; }
		public string ScreenshotGameSelectPath { get; set; }
		public string VideoPath { get; set; }

		[XmlIgnoreAttribute]
		public StorageFolder BoxFrontFolder { get; set; }

		public void SetEmulatorType(string emulatorPath)
		{
			/*if((string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("retroarch", System.StringComparison.InvariantCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.retroarch;
			}
			else */
			if (string.IsNullOrEmpty(emulatorPath) == false && 
					(emulatorPath.Contains("pcsx2", System.StringComparison.CurrentCultureIgnoreCase) || 
					emulatorPath.Contains("xbsx2", System.StringComparison.CurrentCultureIgnoreCase))
				)
			{
				EmulatorType = EEmulatorType.xbsx2;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("retrix", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.rgx;
			}
			else
			{
				//let it just be default retroarch
				EmulatorType = EEmulatorType.retroarch;
			}
		}

		public Platform Copy()
		{
			return (Platform)this.MemberwiseClone();
		}
	}
}
