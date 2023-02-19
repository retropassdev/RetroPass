# RetroPass Ultimate
RetroPass Ultimate is a fork of RetroPass frontend for RetroArch and RetriX Gold, Xenia Cannary UWP, Dolphin UWP on Xbox One & Xbox Series S|X. 

![Video](/Docs/retropassultimate.gif)
![Video](/Docs/menu.gif)

This is made specifically for Xbox console and hopefully, should feel familiar to Xbox users right from the start with its classic xbox theme meets Game Pass Ultimate vibe.
## Usage
[Check out our Wiki for setup and usage intructions](https://github.com/Misunderstood-Wookiee/RetroPassUltimate/wiki)


## Limitations

 - Xbox Only
 - Optimized for Gamepad Only
 - No Custom Themes *(yet)*
 - Zipped content supported only if RetroArch or RetriX Gold and other supported cores/emulators support that.
 - No Automatic Scrapper, you must use Launchbox or EmulationStation (more info below)
  
## Installation
 1. [Download](../../releases/) latest RetroPass Ultimate.
 2. Connect to Xbox through Xbox Device portal and install:
	- RetroPass_Ultimate_x.y.z.0_x64.appxbundle
	- The 3 appx dependencies in Dependencies.zip (if needed). 

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

