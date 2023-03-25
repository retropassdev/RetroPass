# Setup Multiple Data Sources

For example, let's setup 3 separate data sources called:
1. **My Games** - all games in your collection, but without PS2 games
2. **My PS2 Games** - a collection of PS2 games
2. **No Gore Games** - games suitable for anyone to play

You also have 2 external storage devices:
1. **USB flash drive** - 32 GB
2. **SSD** - 512 GB

You would like to setup:
1. **My Games** to USB flash drive
1. **My PS2 Games** to SSD
1. **No Gore Games** to USB flash drive

## **My Games** to USB flash drive

1. Install LaunchBox directly to USB flash drive, but **not to a default LaunchBox directory**. Call it anything you like, for example **LaunchBox_My_Games**.
1. [Download](/Docs/RetroPass.xml) Retropass configuration file and copy it to the root of USB flash drive.
1. Edit **RetroPass.xml** configuration file like this: 
	
	```XML
	<?xml version="1.0"?>
	<retropass version="1.6">
		<dataSources>
			<dataSource>
				<name>My Games</name>
				<type>LaunchBox</type>
				<relativePath>./LaunchBox_My_Games</relativePath>    
			</dataSource>  			
		</dataSources>
	</retropass>
	```
1. Follow the [instructions](/README.md#setup-with-launchbox) and set up your LaunchBox configuration. Import games that will be visible under **My Games** data source.

## **My PS2 Games** to SSD

1. Install LaunchBox directly to SSD, but **not to a default LaunchBox directory**. Call it anything you like, for example **LaunchBox_PS2_Games**, but it can't be the same directory name as first data source **LaunchBox_My_Games**.
1. [Download](/Docs/RetroPass.xml) Retropass configuration file and copy it to the root of SSD.
1. Edit **RetroPass.xml** configuration file like this: 

    ```XML
    <?xml version="1.0"?>
    <retropass version="1.6">
        <dataSources>
            <dataSource>
                <name>My PS2 Games</name>
                <type>LaunchBox</type>
                <relativePath>./LaunchBox_PS2_Games</relativePath>    
            </dataSource>
        </dataSources>
    </retropass>
    ```
1. Follow the [instructions](/README.md#setup-with-launchbox) and set up your LaunchBox configuration. Import games that will be visible under **My PS2 Games** data source.

## **No Gore Games** to USB flash drive

1. At this point, you already have a data source called **My Games** installed to **LaunchBox_My_Games** folder on USB flash drive.
1. Install another LaunchBox directly to USB flash drive, but **not to a default LaunchBox directory**. Call it anything you like, for example **LaunchBox_No_Gore_Games**, but it can't be the same as names of other data sources: **LaunchBox_My_Games** and **LaunchBox_PS2_Games**.

	**YES**, you need **TWO** LaunchBox installations on the same USB flash drive, each in its own directory!!!

1. Modify existing **RetroPass.xml** configuration file by adding a new data source **No Gore Games** to a list of data sources:
	
	```XML
	<?xml version="1.0"?>
	<retropass version="1.6">
		<dataSources>
			<dataSource>
				<name>My Games</name>
				<type>LaunchBox</type>
				<relativePath>./LaunchBox_My_Games</relativePath>    
			</dataSource>  
			<dataSource>
				<name>No Gore Games</name>
				<type>LaunchBox</type>
				<relativePath>./LaunchBox_No_Gore_Games</relativePath>    
			</dataSource>  
		</dataSources>
	</retropass>
	```
1. Follow the [instructions](/README.md#setup-with-launchbox) and set up your LaunchBox configuration which is in **LaunchBox_No_Gore_Games** directory.  Import games that will be visible under **No Gore Games** data source.


After all the steps, USB flash drive directory structure should look like this:

![](/Docs/setup_multiple_data_sources_flash_content.png)

In this example, LaunchBox is used to setup all 3 data sources. Emulation Station could also be used instead. LaunchBox and Emulation Station data sources can be mixed.

 
## Settings

When you start RetroPass, go to Settings page. You should see all configured data sources.

![](/Docs/setup_multiple_data_sources_settings_activate.png)

You can activate or deactivate each. Only games from activated data sources are shown on the main page. That way you can quickly switch between different data sources or show them together.

![](/Docs/setup_multiple_data_sources_settings_activated.png)

If data source is **Unavailable**, that means that the source was activated previously, but external storage is not connected to Xbox, so it can't be found. If you connect external storage containing **Unavailable** data source and refresh Settings page, **Unavailable** status changes to **Active**.

![](/Docs/setup_multiple_data_sources_settings_unavailable.png)


This is just an example. You will in **RetroPass.xml** use different names and relativePaths for your data sources but the princple is the same. Just make sure that you never use the same relative path for different data sources. Every relative path should be unique.
