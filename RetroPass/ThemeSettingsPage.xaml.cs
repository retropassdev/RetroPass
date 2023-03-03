using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RetroPass
{
    public sealed partial class ThemeSettingsPage : Page
    {
        private DataSourceManager dataSourceManager;
        private Brush defaultForeground;

        public ThemeSettingsPage()
        {
            Loaded += ThemeSettingsPage_Loaded;
        }

        private void ThemeSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static bool IsBackOrEscapeKey(Windows.System.VirtualKey key)
        {
            return key == VirtualKey.Back || key == VirtualKey.Escape || key == VirtualKey.GamepadB;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            // Method intentionally left empty.
        }

        private async Task<List<StorageFile>> GetFilesAsync()
        {
            var files = new List<StorageFile>();
            try
            {
                var removableDevices = KnownFolders.RemovableDevices;
                var folders = await removableDevices.GetFoldersAsync();
                foreach (var folder in folders)
                {
                    // Check removable devices for RetroPassUltimate Folder.
                    var retroPassUltimateFolder = await folder.TryGetItemAsync("RetroPassUltimate") as StorageFolder;
                    if (retroPassUltimateFolder != null)
                    {
                        // If exists find Themes folder.
                        var themesFolder = await retroPassUltimateFolder.TryGetItemAsync("Themes") as StorageFolder;
                        if (themesFolder != null)
                        {
                            files.AddRange(await GetFilesInFolderAsync(themesFolder));
                            // Check sub-folders of Themes.
                            foreach (var subfolder in await themesFolder.GetFoldersAsync())
                            {
                                files.AddRange(await GetFilesInFolderAsync(subfolder));
                            }
                        }
                    }

                    // Check removable devices for LaunchBox Folder.
                    var launchBoxFolder = await folder.TryGetItemAsync("LaunchBox") as StorageFolder;
                    if (launchBoxFolder != null)
                    {
                        // If exists find Themes folder.
                        var themesFolder = await launchBoxFolder.TryGetItemAsync("Themes") as StorageFolder;
                        if (themesFolder != null)
                        {
                            files.AddRange(await GetFilesInFolderAsync(themesFolder));
                            // Check sub-folders of Themes.
                            foreach (var subfolder in await themesFolder.GetFoldersAsync())
                            {
                                files.AddRange(await GetFilesInFolderAsync(subfolder));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (this will do for now)
                throw new NotImplementedException();
            }

            return files;
        }

        // Filter only files used using method (List<StorageFile> files = await GetFilesAsync();)
        private async Task<List<StorageFile>> GetFilesInFolderAsync(StorageFolder folder)
        {
            List<string> fileTypes = new List<string> { ".tiff", ".png", ".jpg", ".mp4" };
            List<StorageFile> allFiles = new List<StorageFile>();
            var files = await folder.GetFilesAsync();

            foreach (string fileType in fileTypes)
            {
                allFiles.AddRange(files.Where(x => x.FileType == fileType).ToList());
            }

            return allFiles;
        }
    }
}