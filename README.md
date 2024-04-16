# RetroPass
Retro Pass is a simple frontend for emulators running on Xbox.

![Video](/Docs/menu.gif)

This frontend is made specifically for Xbox console and hopefully, should feel familiar to Xbox users right from the start.

## Supported emulators
- RetroArch - version **v1.10.1.** or **higher**
- RetriX Gold - version **3.0.19.** or **higher**
- Dolphin - version **1.0.1.** or **higher**
- XBSX2 - version **19-09-2022.** or **higher**
- Xenia Canary - version **1.1.0** or **higher**
- PPSSPP - version **1.14.22** or **higher** available here:
  <br> https://github.com/basharast/PPSSPP-UWP-ARM/tree/main/x64

## Limitations
- Xbox only
- optimized for gamepad only
- no custom themes, except light and dark main theme
- zipped content supported only if specific emulator supports it
- no automatic box art scrapper
  
## Prerequisites

- Xbox developer account and console configured for Developer Mode
- Emulators from a list of supported emulators
- External storage of any type used for setting up content library, preferably formatted to NTFS
- Additional computer for setting up content library with either
	* for LaunchBox: Windows, macOS or Linux with Windows virtual machine
	* for Emulation Station: Windows, macOS or Linux


## Installation

 1. [Download](../../releases/) latest RetroPass binaries
 2. Connect to Xbox through Xbox Device portal and install:
	- RetroPass_x.y.z.0_x64.msix
	- 3 appx dependencies in /Dependencies/x64/ folder. 

## Setup

RetroPass can't download box art and game screenshots directly from Xbox because it doesn't have a built in scraper and doing all the configuration directly on Xbox would be fairly difficult. Instead, RetroPass is made compatible with:

- [LaunchBox](/Docs/SetupLaunchBox.md)
- [EmulationStation](/Docs/SetupEmulationStation.md)

LaunchBox is a preferred option, because it gives the best results when properly set up. Mainly:
- It has support for multiple title and gameplay screenshots. 
- Setup specific core per game, if needed.
- Customize playlists order

Choose any of the above options and follow the link to set it up. After configuring, connect external storage to Xbox and start RetroPass. Follow [Add DataSource](/Docs/SettingsDataSources.md) section.

## Settings
You can further configure RetroPass in Settings.

1. [Source](/Docs/SettingsDataSources.md) - Add, activate and deactivate your LaunchBox and EmulationStation data sources
2. [Personalization](/Docs/SettingsPersonalization.md) - Set light or dark theme, the way video is played, etc...
3. [Logging](/Docs/SettingsLogging.md) - Turn on/off logger and view log entries.

## Controls
- **Left Stick** - Navigate
- **D-pad** - Navigate
- **Triggers** - Jump vertically for faster navigation through platforms and collections. In game detail view, go to next and previous game in a playlist.
- **Bumpers** - Jump horizontally for faster navigation through platforms
- **Gamepad A** - Confirm
- **Gamepad B** - Back, previous screen, close dialog
- **Gamepad X** - Add/Remove title from/to **Play Later** playlist
- **Gamepad Y** - Show Search screen
- **Gamepad Menu/Start** - immediately start content, works in Main screen and Collection screen

## Troubleshooting

If after installation and setup you don't see your content, inspect [log](/Docs/SettingsLogging.md),


## Thumbnail Caching

- RetroPass uses own thumbnail caching system, so it has an explicit control over cached thumbnails and used storage. Native Windows thumbnail caching is **not** used.
- Cached thumbnails can be deleted by opening **Settings** page and clicking **Delete Cache** button.
- When front box images are to be shown for the first time, they are first cached into a smaller thumbnail for faster loading later. This means that when you access platform collections, image loading will be slower until thumbnails are generated.
- Currently, there is no memory management for shown thumbnails. Loaded thumbnails stay in memory until the app is terminated. This really makes collections fast to show once they are loaded, but it could potentially crash the app if there is a large number of systems with huge collections.


## Build from source

1. Install Visual Studio 2019
2. Get the latest source code from master branch or [release](../../releases/)
3. Open **RetroPass.sln**
4. Under **Package.appxmanifest** -> **Packing**, create and choose a different Certificate if needed.
5. Set **Configuration** to **Release**
6. Set **Platform** to **x64**
7. **Project** -> **Publish** -> **Create App Packages...**
8. Choose **Sideloading**, turn off **Enable automatic updates**
9. **Yes, select a certificate** or **Yes, use the current certificate**
10. Under **Architecture** check only **x64**
11. Package is built and ready to install.
12. Optionally, for smaller package size, it is safe to delete:
	- Add-AppDevPackage.resources
	- Dependencies\arm
	- Dependencies\arm64
	- Dependencies\x86
	- TelemetryDependencies
	- Add-AppDevPackage.ps1
	- Install.ps1

## Roadmap and Contributing

The main goal of RetroPass is to provide a way to launch content in a way that is familiar to Xbox users. We feel RetroPass fulfills this goal. For a roadmap and possible new features check [milestones](../../milestones). We'll keep fixing bugs so feel free to report any [issues](../../issues) you find.

Feel free to fork the repository and further develop the app to your liking.

