# Setup with LaunchBox

1. Install LaunchBox directly to external storage.
2. Setup and configure LaunchBox to work with any of:
    - RetroArch
    - [RetriX Gold](/Docs/SetupRetriXGold.md)
    - [Dolphin](/Docs/SetupDolphin.md)
    - [XBSX2](/Docs/SetupXBSX2.md)
    - [Xenia Canary](/Docs/SetupXeniaCanary.md)
3. **IMPORTANT!!!** When you import your content, it will ask you to select which media type to download. If you are using LaunchBox just to configure RetroPass, there is no need to download all the media types because RetroPass needs only a subset. It will speed up your setup process and it will make less burden on LaunchBox database. You only need to check these media types:
  - "Box - Front" 
  - "Screenshot - Game Title"
  - "Screenshot - Gameplay"
  - "Screenshot - Game Select"
  - "Video"
  
  	![](/Docs/media_types.png)

4. Make sure that **\<CommandLine>** property for each emulator is properly configured in **LaunchBox/Data/Emulators.xml**. The path is not important, as long as the core name is properly specified. RetroPass ignores the path part and gets only the name of the core. It knows how to properly pass it to RetroArch or RetriX Gold.

	```XML
	<CommandLine>-L "cores\<core_name>.dll"</CommandLine>
	```
	Where **\<core_name>** is the name of the core you wish to use for particular emulator.

5. Optionally, if you need to set up a different core for a game, go to Launchbox, open "Edit Metadata/Media" and modify command line under Emulation.

 	![](/Docs/core_per_game.png)

6. If you want to specify the order of your playlists and platforms, you can do so by checking the [LaunchBox playlist and platform sorting](/Docs/SetupLaunchBoxSorting.md) section.

7. [Download](/Docs/v1.5/RetroPass.xml) Retropass configuration file and copy it to the root of external storage.

8. Edit **RetroPass.xml** configuration file. 
	
	**\<relativePath>** points to LaunchBox directory on the external storage. Do not put absolute path like "E:\LaunchBox", because when external storage is plugged into Xbox, it might be recognized under a different letter. For example, if LaunchBox folder is in the root of external storage, then it should be configured like this:

	```XML
	<?xml version="1.0"?>
	<dataSource>
		<type>LaunchBox</type>
		<relativePath>./LaunchBox</relativePath>
	</dataSource>
	```  
9. At this point setup is finished. Connect external storage to Xbox and start RetroPass. Follow [First Run and Settings](/README.md#first-run-and-settings) section.
  
 

