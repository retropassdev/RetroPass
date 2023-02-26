# RetroPass Ultimate
RetroPass Ultimate is a fork of RetroPass frontend for RetroArch and RetriX Gold, Xenia Cannary UWP, Dolphin UWP on Xbox One & Xbox Series S|X. 

![Video](https://github.com/Misunderstood-Wookiee/RetroPassUltimate/blob/cfb5c229a25da69934f0fc5180301f544f6cb592/Docs/retropassultimate.gif)
![Video](/Docs/collection.gif)

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
	- The 3 appx dependencies in Dependencies.zip or Dependencies.7z.
9. At this point Installation is finished. Proceed to and Follow [Setup](https://github.com/Misunderstood-Wookiee/RetroPassUltimate/wiki/Basic-Usage) Guide!


## Build from source

1. Install Visual Studio 2019
2. Get the latest source code from Main/Dev branch or [release](../../releases/)
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

