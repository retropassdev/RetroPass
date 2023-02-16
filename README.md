# RetroPass Ultimate
RetroPass Ultimate is a fork of RetroPass frontend for RetroArch and RetriX Gold, Xenia Cannary UWP, Dolphin UWP on Xbox One & Xbox Series S|X. 

![Video](/Docs/retropassultimate.gif)
![Video](/Docs/menu.gif)

This is made specifically for Xbox console and hopefully, should feel familiar to Xbox users right from the start with its classic xbox theme meets Game Pass Ultimate vibe.

## Limitations

 - Xbox Only
 - Optimized for Gamepad Only
 - No Custom Themes *(yet)*
 - Zipped content supported only if RetroArch or RetriX Gold and other supported cores/emulators support that.
 - No Automatic Scrapper, you must use Launchbox or EmulationStation (more info below)
  
## Prerequisites

- Xbox developer account and console configured for Developer Mode...
- If using RetroArch - version **v1.10.1.** or **higher**
- If using RetriX Gold - version **3.0.19.** or **higher**
- If using Dolphin UWP - version **v1.1.0.** or **higher**
- If using Xenia Cannary UWP - version **v1.0.1.** or **higher**
- For seamless experience, it is recommended to setup a hotkey for quitting RetroArch. When content is started from RetroPass, once user exits RetroArch either with the hotkey or through the menu, it will immediately return to RetroPass.
- External storage of any type used for setting up content library
- PC/Laptop for setting up content library

## Installation
 1. [Download](../../releases/) latest RetroPass binaries
 2. Connect to Xbox through Xbox Device portal and install:
	- RetroPass_x.y.z.0_x64.msix
	- 3 appx dependencies in /Dependencies/x64/ folder. 



 ## Setup
 
RetroPass can't be configured directly from Xbox because it doesn't have a built in scraper and doing all the configuration directly on Xbox would be fairly difficult. Instead, RetroPass is made compatible with:

1. LaunchBox (Recommended)
2. Emulation Station


## Setup with LaunchBox

LaunchBox is a preferred option, because it gives the best results when properly set up. Mainly:
 - It has support for multiple title and gameplay screenshots. 
 - Setup specific core for a game, if needed.

1. Install LaunchBox directly to external storage
2. Setup and configure LaunchBox to work with RetroArch or [RetriX Gold](/Docs/SetupRetriXGold.md), [XBSX2](https://github.com/TheRhysWyrill/XBSX2), [Xenia](https://github.com/SirMangler/xenia), [Xenia Canary](https://github.com/SirMangler/xenia/tree/canary_experimental), or [Dolphin](https://github.com/SirMangler/dolphin/tree/uwp)  
3. **IMPORTANT!!!** When you import your content, it will ask you to select which media type to download. If you are using LaunchBox just to configure RetroPass, there is no need to download all the media types because RetroPass needs only a subset. (It will speed up your setup process and it will make less burden on LaunchBox database)

You only need to check these media types:
  - "Box - Front" 
  - "Screenshot - Game Title"
  - "Screenshot - Gameplay"
  - "Screenshot - Game Select"
  - "Video"

  	![](/Docs/media_types.png)


4. Make sure that **\<CommandLine>** property for each emulator is properly configured in **LaunchBox/Data/Emulators.xml**. The **path is not important, as long as the core name is properly specified**. RetroPass ignores the path part and gets only the name of the core. It knows how to properly pass it to RetroArch or RetriX Gold, XBSX2, Dolphin Stand-alone Xbox UWP.

	```xml
	<CommandLine>-L "cores\<core_name>.dll"</CommandLine>
	```
	Where **\<core_name>** is the name of the core you wish to use for particular emulator.
5. Optional, if you need to set up a different core or emulator for a game, Run Launchbox and right-click a game, choose "Edit Metadata/Media" and modify command line under Emulation.
 	![](/Docs/core_per_game.png)
6.Optional, if you wish to add/setup other supported emulator types, Open LaunchBox and click ![](/Docs/lbaeg04.png) > Tools > Manage > Emulators
** Adding new Emulators
![](/Docs/lbaeg01.png)
![](/Docs/lbaeg02.png)
**Example of pre configuration for *some* supported emulators**
![](/Docs/lbaeg03.png)

7. [Download](/Docs/RetroPass.xml) Retropass configuration file and copy it to the root of external storage.
8. Edit **RetroPass.xml** configuration file. 
	
	**\<relativePath>** points to LaunchBox directory on the external storage. You may change location by removing **./** 
Example: **RetroPass/LaunchBox**, however, ***do NOT put absolute path like "E:\LaunchBox"***, because when external storage is plugged into Xbox, it might be recognized under a different letter!

For example, if LaunchBox folder is in the root of external storage, then it should be configured like this:

	```xml
	<?xml version="1.0"?>
	<dataSource>
		<type>LaunchBox</type>
		<relativePath>./LaunchBox</relativePath>
	</dataSource>
	```
9. At this point setup is finished. Connect external storage to Xbox and start RetroPass. Follow [First Run and Settings](#first-run-and-settings) section!



## Setup with Emulation Station

If you do not wish to use LaunchBox, it is also possible to create Emulation Station compatible metadata source.

1. Setup and configure Emulation Station to work with RetroArch on PC, preferably directly on external storage.
2. Download all images, descriptions and videos. There are various scrapers that can download assets from various databases and output to Emulation Station gamelist.xml files
3. Edit **RetroPass.xml** configuration file. 
	
	**\<relativePath>** points to Emulation Station directory on the external storage. Do not put absolute path like "E:\EmulationStation", because when external storage is plugged into Xbox, it might be recognized under a different letter. For example, if EmulationStation folder is in the root of external storage, then it should be configured like this:

	```XML
	<?xml version="1.0"?>
	<dataSource>
		<type>EmulationStation</type>
		<relativePath>./EmulationStation</relativePath>
	</dataSource>
	```
4. Check that **es_systems.cfg** file exists somewhere in the Emulation Station directory and that it has valid systems defined. i.e.
	- Ignore **\<path>** property, it doesn't have to be properly set.
	- Make sure that **\<command>** property for each system is properly configured. Paths are not important, as long as the core name is properly specified. RetroPass ignores everything in the command line except the core name. It knows how to properly pass it to RetroArch.
	- **\<fullname>** is what is actually displayed in RetroPass as the name of the system.
	- In the example below, only relevant properties are shown:

	```XML
	<system>
		<name>nes</name>
		<fullname>Nintendo Entertainment System</fullname>	
		<command>-L %HOME%\\systems\nestopia_libretro.dll</command>
		<platform>nes</platform>
	</system>
	```
5. Setup **gamelist.xml** for every system:
	- **gamelist.xml** must be in its own directory which equals system's **\<name>** defined in **es_systems.cfg**. For example, based on the example in step 3, it is expected that for NES, it should be **/nes/gamelist.xml**.
	- All paths defined in **\<path>**, **\<thumbnail>**, **\<image>**, **\<video>**  must be a relative path to **\<relativePath>** defined in step 3. 
	- For example, if in **Retropass.xml** relative path is 	
	```XML
		<relativePath>./EmulationStation</relativePath>
	```
	- And 
	```XML
		<game>
			<path>./contents/nes/Elite.nes</path>
			<name>Elite</name>
			<desc>The player...</desc>
			<thumbnail>./downloaded_images/nes/covers/Elite-thumb.jpg</thumbnail>
			<image>./downloaded_images/nes/Elite-image.jpg</image>
			<video>./videos/nes/Elite.mp4</video>
			<releasedate>19910101T000000</releasedate>
			<developer>David Braben, Ian Bell</developer>
			<publisher>Imagineer Co., Ltd.</publisher>
			<genre>Action, Shooter</genre>
		</game>
	```
	- Then the thumbnail full path would be
	```
		./EmulationStation/downloaded_images/nes/covers/Elite-thumb.jpg
	```
6. At this point setup is finished. Connect external storage to Xbox and start RetroPass. Follow [First Run and Settings](#first-run-and-settings) section.




## First Run and Settings

1. If RetroPass configuration file is found and properly configured, Settings screen is shown:

	![](/Docs/first_settings.png)
2. Click **Activate** button and then Back.
3. You should see a list of platforms and content.

- **Delete Cache** Deletes all cached thumbnails, but also **Play Later** playlist.
- **Auto Play Video** automatically plays a video when content is selected.
- **Enable Logging** enables logging. See [Troubleshooting](#troubleshooting) section for more info.

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
5. Log file is also generated in the local RetroPass folder on Xbox.

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

