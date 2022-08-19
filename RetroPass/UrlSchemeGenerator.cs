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
			args += " \"" + game.ApplicationPathFull + "\"";
			args += "&launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
		}

		private static string GetUrlRetrix(Game game)
		{
			//retrix uses the same uri scheme syntax as retroarch
			string args = "cmd=" + "retroarch";
			args += " -L";
			args += " cores\\" + game.CoreName;
			args += " \"" + game.ApplicationPathFull + "\"";
			args += " &launchOnExit=" + "retropass:";
			return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
		}
	}
}
