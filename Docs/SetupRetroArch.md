# RetroArch

## Setup with LaunchBox

 1. Open LaunchBox
 2. Go to **Tools -> Manage... -> Emulators**
 3. Click **Add** to add an emulator.
 4. In **Emulator Name** dropdown choose **Retroarch**.
 5. **Application Path** doesn't have to point to real application. It's enough to just input **retroarch**.
 
 	![](/Docs/setup_retroarch_edit_emulator.png)
 
 6. Click **Associated Platforms**, and make sure "Default Emulator" checkbox is checked for platforms which should be started with Retroarch.
 
 	![](/Docs/setup_retroarch_associated_platforms.png)
    
 7. Optionally, if you need to set up a different core for a game, go to Launchbox, select the game, open "Edit Metadata/Media" and modify command line under Emulation.

 	![](/Docs/core_per_game.png)
	
 8. For seamless experience, it is recommended to setup a hotkey for quitting RetroArch. When content is started from RetroPass, once user exits RetroArch either with the hotkey or through the menu, it will immediately return to RetroPass.

	![](/Docs/setup_retroarch_quit_hotkey.png)