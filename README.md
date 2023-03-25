# RetroPass
Retro Pass is a simple frontend for RetroArch, RetriX Gold, Dolphin, XBSX2 and Xenia Canary running on Xbox. 

![Video](/Docs/menu.gif)

This frontend is made specifically for Xbox console and hopefully, should feel familiar to Xbox users right from the start.

## Limitations
- Xbox only
- optimized for gamepad only
- no custom themes
- zipped content supported only if specific emulator supports it
- no scrapper
  
## Prerequisites

- Xbox developer account and console configured for Developer Mode
- If using RetroArch - version **v1.10.1.** or **higher**
- If using RetriX Gold - version **3.0.19.** or **higher**
- If using Dolphin - version **1.0.1.** or **higher**
- If using XBSX2 - version **19-09-2022.** or **higher**
- If using Xenia Canary - version **1.1.0** or **higher**
- For seamless experience, it is recommended to setup a hotkey for quitting RetroArch. When content is started from RetroPass, once user exits RetroArch either with the hotkey or through the menu, it will immediately return to RetroPass.
- External storage of any type used for setting up content library
- Additional computer for setting up content library with either
	* for LaunchBox: Windows, macOS or Linux with Windows virtual machine
	* for Emulation Station: Windows, macOS or Linux


## Installation

 1. [Download](../../releases/) latest RetroPass binaries
 2. Connect to Xbox through Xbox Device portal and install:
	- RetroPass_x.y.z.0_x64.msix
	- 3 appx dependencies in /Dependencies/x64/ folder. 

## Setup

RetroPass can't be configured directly from Xbox because it doesn't have a built in scraper and doing all the configuration directly on Xbox would be fairly difficult. Instead, RetroPass is made compatible with:

- [LaunchBox](/Docs/SetupLaunchBox.md)
- [EmulationStation](/Docs/SetupEmulationStation.md)

LaunchBox is a preferred option, because it gives the best results when properly set up. Mainly:
- It has support for multiple title and gameplay screenshots. 
- Setup specific core per game, if needed.

Choose any of the above options and follow the link to set it up. After configuring, connect external storage to Xbox and start RetroPass. Follow [First Run and Settings](#first-run-and-settings) section.

## First Run and Settings

1. If RetroPass configuration file is found and properly configured, Settings screen is shown:

	![](/Docs/first_settings.png)
1. Click **Activate** button and then Back
1. You should see a list of platforms and content
	- **Delete Cache** deletes all cached thumbnails, but also **Play Later** playlist.
	- **Auto Play Video** automatically plays a video when content is selected.
	- **Enable Logging** enables logging. See [Troubleshooting](#troubleshooting) section for more info.

## Advanced Setup - Multiple Data Sources

RetroPass can be configured so that it recognizes more than one LaunchBox or EmulationStation data source. Here are a few example scenarios where you might find this setup useful:
* You would like to connect two or more external storages. You have one USB stick that doesn't have enough free space and contains only a subset of your games. You also have a larger SSD where you keep the rest of your game collection.
* You have a single SSD where you keep all your games. You would like to separate those into two data sources. One data source contains all your games and another one only games which are appropriate for players of all ages.

For more information go to [Setup multiple data sources](/Docs/SetupMultipleDataSources.md)
 
## Controls
- **Gamepad A** - Confirm
- **Gamepad B** - Back, previous screen, close dialog
- **Gamepad X** - Add/Remove title from/to **Play Later** playlist
- **Gamepad Y** - Show Search screen
- **Gamepad View/Back** - open Log screen, only in Settings screen
- **Gamepad Menu/Start** - immediately start content, works in Main screen and Collection screen

## Troubleshooting

If after installation and setup you don't see your content:
1. Go to Settings and check **Enable Logging**.
2. Quit RetroPass and restart it again.
3. Go to Settings and press View (Back) button on Xbox gamepad.
4. View log and notice yellow warning and red error log entries.
5. Log file is also generated in the local RetroPass folder on Xbox

Make sure to **turn off** logging after troubleshooting, because writing to a log file slows down app performance.

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

The main goal of RetroPass is to provide a way to launch content in a way that is familiar to Xbox users. We feel RetroPass fulfills this goal and is usable at this point. Therefore, any further development of new features is not planned. We'll keep fixing bugs so feel free to report any [issues](../../issues) you find.

Feel free to fork the repository and further develop the app to your liking.

