# Setup with Emulation Station

If you do not wish to use LaunchBox, it is also possible to create Emulation Station compatible metadata source.

1. Setup and configure Emulation Station to work with RetroArch, preferably directly on external storage.

2. Download all images, descriptions and videos. There are various scrapers that can download assets from various databases and output to Emulation Station gamelist.xml files

3. [Download](/Docs/v1.6/RetroPass.xml) Retropass configuration file and copy it to the root of external storage.

4. Edit **RetroPass.xml** configuration file. 
	
	**\<relativePath>** points to Emulation Station directory on the external storage. Do not put absolute path like "E:\EmulationStation", because when external storage is plugged into Xbox, it might be recognized under a different letter. For example, if EmulationStation folder is in the root of external storage, then it should be configured like this:

	```XML
	<retropass version="1.6">
		<dataSources>
			<dataSource>
				<name>Games</name>
				<type>EmulationStation</type>
				<relativePath>./EmulationStation</relativePath>    
			</dataSource>
		</dataSources>
	</retropass>
	```

5. Check that **es_systems.cfg** file exists somewhere in the Emulation Station directory and that it has valid systems defined. i.e.
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
6. Setup **gamelist.xml** for every system:
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
7. At this point setup is finished. Connect external storage to Xbox and start RetroPass. Follow [Add DataSource](/Docs/SettingsDataSources.md) section.