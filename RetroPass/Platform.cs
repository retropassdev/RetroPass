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
			citrara,
            xbsx2,
			dolphin,
			xenia,
			xeniacanary,
			ppsspp,
			duckstation,
			flycast,
			edge,
            gamepass,
            armsx,
			panda,
            supermodel,
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
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("citrara", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.citrara;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("dolphin", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.dolphin;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false &&
				(emulatorPath.Contains("xenia-canary", System.StringComparison.CurrentCultureIgnoreCase) ||
				emulatorPath.Contains("xeniacanary", System.StringComparison.CurrentCultureIgnoreCase))
				)
			{
				EmulatorType = EEmulatorType.xeniacanary;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("xenia", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.xenia;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("ppsspp", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.ppsspp;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("duckstation", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.duckstation;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("flycast", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.flycast;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("edge", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.edge;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("gamepass", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.gamepass;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("armsx", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.armsx;
			}
			else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("panda", System.StringComparison.CurrentCultureIgnoreCase))
			{
				EmulatorType = EEmulatorType.panda;
			}
            else if (string.IsNullOrEmpty(emulatorPath) == false && emulatorPath.Contains("supermodel", System.StringComparison.CurrentCultureIgnoreCase))
            {
                EmulatorType = EEmulatorType.supermodel;
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
