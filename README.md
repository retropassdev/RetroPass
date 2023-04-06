# LaunchPass
LaunchPass is a modified fork of [RetroPass](https://github.com/retropassdev/RetroPass) without this project would not exist & is a themeable frontend for Xbox/Xbox Series Retro Gaming Emulators!
![Video](https://github.com/Misunderstood-Wookiee/LaunchPass/blob/e55c515e3d5e9385093a306142e1ab50302d9f97/Docs/LaunchPass.webp)
![Video](/Docs/collection.gif)


This is made specifically for Xbox console and hopefully, should feel familiar to Xbox users right from the start with its Game Pass Ultimate vibe.
## Usage
[Check out our Wiki for setup and usage intructions](https://github.com/Misunderstood-Wookiee/LaunchPass/wiki)


## Limitations

 - Xbox Only
 - Optimized for Gamepad Only
 - Zipped content supported only if RetroArch, RetriX or other supported cores/emulators support reading the file extentsion.
 - No Automatic Scrapper, you must use Launchbox (More Info Below)
 - EmulationStation support is discontinued sorry!
  
## Installation
 1. [Download](../../releases/) latest LaunchPass.
 2. Connect to Xbox through Xbox Device portal and install:
	- LaunchPass_x.y.z.0_x64.appxbundle
	- The 3 appx dependencies in Dependencies.zip or Dependencies.7z.
9. At this point Installation is finished. Proceed to and Follow [Setup](https://github.com/Misunderstood-Wookiee/LaunchPass/wiki/Basic-Usage) Guide!


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

Feel free to fork the repository and further develop the app to your liking, but you must keep branding & give credit where due.

