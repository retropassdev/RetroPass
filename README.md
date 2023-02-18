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
- If using Dolphin - version **1.0.1.** or **higher**
- If using XBSX2 - version **19-09-2022.** or **higher**
- If using Xenia Canary - version **1.1.0** or **higher**
- For seamless experience, it is recommended to setup a hotkey for quitting RetroArch. When content is started from RetroPass, once user exits RetroArch either with the hotkey or through the menu, it will immediately return to RetroPass.
- External storage of any type used for setting up content library
- PC/Laptop for setting up content library

## Installation
 1. [Download](../../releases/) latest RetroPass binaries
 2. Connect to Xbox through Xbox Device portal and install:
	- RetroPass_x.y.z.0_x64.msix
	- 3 appx dependencies in /Dependencies/x64/ folder. 

	```xml
	<?xml version="1.0"?>
	<dataSource>
		<type>LaunchBox</type>
		<relativePath>./LaunchBox</relativePath>
	</dataSource>
	```
9. At this point setup is finished. Connect external storage to Xbox and start RetroPass. Follow [First Run and Settings](#Basic-Usage#first-run-and-settings) section!


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

Feel free to fork the repository and further develop the app to your liking.

