using System;

namespace RetroPass
{
	class UrlSchemeGenerator
	{
		public static string GetUrl(Game game)
		{
			string url = "";

			switch (game.GamePlatform.EmulatorType)
			{
				case Platform.EEmulatorType.retroarch:
					url = GetUrlRetroarch(game);
					break;
				case Platform.EEmulatorType.rgx:
					url = GetUrlRetrix(game);
					break;
				case Platform.EEmulatorType.xbsx2:
					url = GetUrlXBSX2(game);
					break;
				case Platform.EEmulatorType.dolphin:
					url = GetUrlDolphin(game);
					break;
                case Platform.EEmulatorType.ppsspp:
                    url = GetUrlPpsspp(game);
                    break;
                case Platform.EEmulatorType.duckstation:
                    url = GetUrlDuckstation(game);
                    break;
                case Platform.EEmulatorType.flycast:
                    url = GetUrlFlycast(game);
                    break;
                default:
					break;
			}

			return url;
		}

		private static string GetUrlRetroarch(Game game)
		{
			string args = "cmd=" + "retroarch";
			args += " -L";
			args += " cores\\" + game.CoreName;
			args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
			args += "&launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
		}

		private static string GetUrlRetrix(Game game)
		{
			//retrix uses the same uri scheme syntax as retroarch
			string args = "cmd=" + "retroarch";
			args += " -L";
			args += " cores\\" + game.CoreName;
			args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
			args += " &launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
		}

		private static string GetUrlXBSX2(Game game)
		{
			string args = "cmd=" + "pcsx2.exe";
			args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
			args += "&launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
		}

		private static string GetUrlDolphin(Game game)
		{
			string args = "cmd=" + "dolphin.exe";
			args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
			args += "&launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        private static string GetUrlPpsspp(Game game)
        {
            string args = "cmd=" + "ppsspp.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "retropass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        private static string GetUrlDuckstation(Game game)
        {
            string args = "cmd=" + "duckstation.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "retropass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        private static string GetUrlFlycast(Game game)
        {
            string args = "cmd=" + "flycast.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "retropass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }
    }
}
