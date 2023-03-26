// This code defines a UrlSchemeGenerator class.
// The class generates URLs for launching games in different emulators based on the game's emulator type.
// It supports various emulator types, including Retroarch, Retrix, XBSX2, Dolphin, PPSSPP, Duckstation, Flycast, Xenia, and Xenia Canary.

using System;

namespace RetroPass
{
    // Defines an internal class UrlSchemeGenerator.
    internal class UrlSchemeGenerator
    {
        // Public method GetUrl takes a Game object and returns a URL for launching the game in the appropriate emulator.
        public static string GetUrl(Game game)
        {
            string url = "";

            // Switch case to determine the emulator type for the game.
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

                case Platform.EEmulatorType.xenia:
                    url = GetUrlXenia(game);
                    break;

                case Platform.EEmulatorType.xeniacanary:
                    url = GetUrlXeniaCanary(game);
                    break;

                default:
                    break;
            }

            return url;
        }

        // Private methods for generating URLs for different emulator types.

        // GetUrlRetroarch generates a URL for launching a game in the Retroarch emulator.
        private static string GetUrlRetroarch(Game game)
        {
            string args = "cmd=" + "retroarch";
            args += " -L";
            args += " cores\\" + game.CoreName;
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlRetrix generates a URL for launching a game in the Retrix emulator.
        private static string GetUrlRetrix(Game game)
        {
            // Retrix uses the same URI scheme syntax as Retroarch.
            string args = "cmd=" + "retroarch";
            args += " -L";
            args += " cores\\" + game.CoreName;
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += " &launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlXBSX2 generates a URL for launching a game in the XBSX2 (PCSX2) emulator.
        private static string GetUrlXBSX2(Game game)
        {
            string args = "cmd=" + "pcsx2.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlDolphin generates a URL for launching a game in the Dolphin emulator.
        private static string GetUrlDolphin(Game game)
        {
            string args = "cmd=" + "dolphin.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlPpsspp generates a URL for launching a game in the PPSSPP emulator.
        private static string GetUrlPpsspp(Game game)
        {
            string args = "cmd=" + "ppsspp.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlDuckstation generates a URL for launching a game in the Duckstation emulator.
        private static string GetUrlDuckstation(Game game)
        {
            string args = "cmd=" + "duckstation.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlFlycast generates a URL for launching a game in the Flycast emulator.
        private static string GetUrlFlycast(Game game)
        {
            string args = "cmd=" + "flycast.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlXenia generates a URL for launching a game in the Xenia emulator.
        private static string GetUrlXenia(Game game)
        {
            string args = "cmd=" + "xenia.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }

        // GetUrlXeniaCanary generates a URL for launching a game in the Xenia Canary emulator.
        private static string GetUrlXeniaCanary(Game game)
        {
            string args = "cmd=" + "xeniacanary.exe";
            args += " \"" + Uri.EscapeDataString(game.ApplicationPathFull) + "\"";
            args += "&launchOnExit=" + "LaunchPass:";
            return game.GamePlatform.EmulatorType.ToString() + ":?" + args;
        }
    }
}