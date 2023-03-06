using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RetroPass_Ultimate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ThemeSettingsPage : Page
    {
        public ThemeSettingsPage()
        {
            this.InitializeComponent();
            LoadBackgroundsAndFonts();

        }

        private async void LoadBackgroundsAndFonts()
        {
            try
            {
                var backgroundFiles = await GetFilesAsync("Backgrounds", new List<string>() { ".png", ".jpg", ".mp4", ".mpg" });
                List<String> backgroundsFilesNameList = backgroundFiles.Select(s => s.DisplayName).ToList();

                MainPageCB.ItemsSource = backgroundsFilesNameList;
                GamePageCB.ItemsSource = backgroundsFilesNameList;
                DetailsPageCB.ItemsSource = backgroundsFilesNameList;
                SearchPageCB.ItemsSource = backgroundsFilesNameList;
                CustomizePageCB.ItemsSource = backgroundsFilesNameList;
                SettingsPageCB.ItemsSource = backgroundsFilesNameList;

                var fontFiles = await GetFilesAsync("Fonts", new List<string>() { ".ttf" });
                FontsCB.ItemsSource = fontFiles.Select(s => s.DisplayName).ToList();
            }
            catch (Exception)
            {
                //throw;
            }
        }


        private async Task<List<StorageFile>> GetFilesAsync(string folderName, List<string> allowedExtensions)
        {
            var files = new List<StorageFile>();
            try
            {
                var removableDevices = KnownFolders.RemovableDevices;
                var folders = await removableDevices.GetFoldersAsync();
                foreach (StorageFolder rootFolder in folders)
                {
                    //FIND LAUNCHBOX FOLDER TO RELATED RETROPASS FOLDER ON THE SAME REMOVABLE DEVICE
                    StorageFolder launchBoxFolder = await rootFolder.TryGetItemAsync("LaunchBox") as StorageFolder;

                    if (launchBoxFolder != null)
                    {
                        // Check removable devices for RetroPassUltimate Folder.
                        StorageFolder retroPassUltimateFolder = await rootFolder.TryGetItemAsync("RetroPassUltimate") as StorageFolder;
                        if (retroPassUltimateFolder != null)
                        {
                            // If exists find folderName
                            var folderNameStorageFolder = await retroPassUltimateFolder.TryGetItemAsync(folderName) as StorageFolder;
                            if (folderNameStorageFolder != null)
                            {
                                files.AddRange(await GetFilesInFolderAsync(folderNameStorageFolder, allowedExtensions));
                                // Check sub-folders for files
                                foreach (var subfolder in await folderNameStorageFolder.GetFoldersAsync())
                                {
                                    files.AddRange(await GetFilesInFolderAsync(subfolder, allowedExtensions));
                                }
                            }
                            else
                            {
                                await retroPassUltimateFolder.CreateFolderAsync(folderName);
                            }
                        }
                        else
                        {
                            StorageFolder newRetroPassUltimateFolder = await rootFolder.CreateFolderAsync("RetroPassUltimate") as StorageFolder;
                            if (newRetroPassUltimateFolder != null)
                            {
                                await newRetroPassUltimateFolder.CreateFolderAsync(folderName);
                            }
                        }
                    }



                }
            }
            catch (Exception ex)
            {
                //throw
            }

            return files;
        }

        // Filter only allowed extension files 
        private async Task<List<StorageFile>> GetFilesInFolderAsync(StorageFolder folder, List<string> allowedExtensions)
        {
            List<StorageFile> allFiles = new List<StorageFile>();
            var files = await folder.GetFilesAsync();

            foreach (string fileType in allowedExtensions)
            {
                allFiles.AddRange(files.Where(x => x.FileType == fileType).ToList());
            }

            return allFiles;
        }


    }
}



