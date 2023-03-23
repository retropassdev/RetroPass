using RetroPass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LaunchPass
{
    /// <summary>
    /// Theme Settings Page: Purpose, Store, Fetch & Set all user customizable options of the App 
    /// </summary>
    public sealed partial class ThemeSettingsPage : Page
    {
        private StorageFile settingsXMLFile;

        public ThemeSettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += ThemeSettingsPage_Loaded;
        }

        private void ThemeSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayer.MediaPath = ((App)Application.Current).CurrentThemeSettings.GetMediaPath("CustomizePage");

            LoadBackgroundsAndFonts();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;

            base.OnNavigatedFrom(e);
        }

        //on windows, windows key + backspace
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }
        /// <summary>
        /// Variables & Methods for  fetching, caching file extensions  then populating the customize combo-boxes of the themes settings page.
        /// </summary>
        // Cache for storing file extensions for different file types
        private Dictionary<string, List<string>> fileExtensionCache = new Dictionary<string, List<string>>();

        // Function to get file extensions based on the input type (background or font)
        private List<string> GetFileExtensions(string type)
        {
            // Check if the extensions are already in the cache
            if (fileExtensionCache.ContainsKey(type))
            {
                return fileExtensionCache[type];
            }

            List<string> fileExtensions = new List<string>();

            // Define file extensions for different types
            switch (type)
            {
                case "background":
                    fileExtensions = new List<string>() { ".png", ".jpg", ".jxr", ".dds", ".jpeg", ".webp", ".webm", ".mkv", ".mp4", ".mov", ".wdp" };
                    break;
                case "font":
                    fileExtensions = new List<string>() { ".ttf", ".otf" };
                    break;
            }

            // Store the extensions in the cache for future use
            fileExtensionCache[type] = fileExtensions;
            return fileExtensions;
        }

        // Async function to load backgrounds and fonts
        private async Task LoadBackgroundsAndFonts()
        {
            try
            {
                // Get removable devices
                var removableDevices = KnownFolders.RemovableDevices;
                var folders = await removableDevices.GetFoldersAsync();

                StorageFolder launchPassFolderCurrent = null;

                // Iterate through the devices to find LaunchBox and LaunchPass folders
                foreach (StorageFolder rootFolder in folders)
                {
                    StorageFolder launchBoxFolder = await rootFolder.TryGetItemAsync("LaunchBox") as StorageFolder;

                    if (launchBoxFolder != null)
                    {
                        launchPassFolderCurrent = await rootFolder.TryGetItemAsync("LaunchPass") as StorageFolder;
                        break;
                    }
                }

                if (launchPassFolderCurrent != null)
                {
                    // Load the theme settings XML file
                    settingsXMLFile = await GetLaunchPassThemeSettingsFile(launchPassFolderCurrent);

                    // Get the supported file extensions for background and font files
                    var backgroundFileExtensions = GetFileExtensions("background");
                    var fontFileExtensions = GetFileExtensions("font");

                    // Retrieve background and font files
                    var backgroundFiles = await GetFilesAsync(launchPassFolderCurrent, "Backgrounds", backgroundFileExtensions);
                    var fontFiles = await GetFilesAsync(launchPassFolderCurrent, "Fonts", fontFileExtensions);

                    // Create lists of file names for backgrounds and fonts
                    List<String> backgroundsFilesNameList = backgroundFiles.Select(s => s.Name).ToList();
                    List<String> fontFilesNameList = fontFiles.Select(s => s.Name).ToList();

                    // Populate ComboBoxes with the file names
                    MainPageCB.ItemsSource = backgroundsFilesNameList;
                    GamePageCB.ItemsSource = backgroundsFilesNameList;
                    DetailsPageCB.ItemsSource = backgroundsFilesNameList;
                    SearchPageCB.ItemsSource = backgroundsFilesNameList;
                    CustomizePageCB.ItemsSource = backgroundsFilesNameList;
                    SettingsPageCB.ItemsSource = backgroundsFilesNameList;
                    FontsCB.ItemsSource = fontFilesNameList;
                }

                // Check if the current theme settings are available
                if (((App)Application.Current).CurrentThemeSettings != null)
                {
                    // Set the selected font in the ComboBox
                    FontsCB.SelectedItem = ((App)Application.Current).CurrentThemeSettings.Font;

                    // Get the list of background settings for each page
                    List<Background> backgroundList = ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background;

                    // If the background list exists and is not empty, set the selected background for each page
                    if (backgroundList != null && backgroundList.Count() != 0)
                    {
                        MainPageCB.SelectedItem = backgroundList.Where(s => s.Page == "MainPage").Select(s => s.File).FirstOrDefault();
                        GamePageCB.SelectedItem = backgroundList.Where(s => s.Page == "GamePage").Select(s => s.File).FirstOrDefault();
                        DetailsPageCB.SelectedItem = backgroundList.Where(s => s.Page == "DetailsPage").Select(s => s.File).FirstOrDefault();
                        SearchPageCB.SelectedItem = backgroundList.Where(s => s.Page == "SearchPage").Select(s => s.File).FirstOrDefault();
                        CustomizePageCB.SelectedItem = backgroundList.Where(s => s.Page == "CustomizePage").Select(s => s.File).FirstOrDefault();
                        SettingsPageCB.SelectedItem = backgroundList.Where(s => s.Page == "SettingsPage").Select(s => s.File).FirstOrDefault();
                    }

                    // Get the box art type from the theme settings
                    string boxArtType = ((App)Application.Current).CurrentThemeSettings.BoxArtType;

                    // If the box art type exists, set the corresponding CheckBox to checked
                    if (!string.IsNullOrEmpty(boxArtType))
                    {
                        if (Convert.ToString(toggleBoxFront.Content) == boxArtType)
                        {
                            toggleBoxFront.IsChecked = true;
                        }
                        else if (Convert.ToString(toggleBox3d.Content) == boxArtType)
                        {
                            toggleBox3d.IsChecked = true;
                        }
                        else if (Convert.ToString(ToggleCartFront.Content) == boxArtType)
                        {
                            ToggleCartFront.IsChecked = true;
                        }
                        else if (Convert.ToString(toggleClearLogo.Content) == boxArtType)
                        {
                            toggleClearLogo.IsChecked = true;
                        }
                        else if (Convert.ToString(ToggleFanartBackground.Content) == boxArtType)
                        {
                            ToggleFanartBackground.IsChecked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur while loading background and font files
                Debug.WriteLine($"Error loading backgrounds and fonts: {ex.Message}");
            }
        }

        private async Task<List<StorageFile>> GetFilesAsync(StorageFolder rootFolder, string subFolderName, List<string> allowedExtensions)
        {
            var files = new List<StorageFile>();
            try
            {
                var folderNameStorageFolder = await rootFolder.TryGetItemAsync(subFolderName) as StorageFolder;
                if (folderNameStorageFolder != null)
                {
                    files.AddRange(await GetFilesInFolderAsync(folderNameStorageFolder, allowedExtensions));
                    // Check sub-folders for files
                    foreach (var subfolder in await folderNameStorageFolder.GetFoldersAsync())
                    {
                        files.AddRange(await GetFilesInFolderAsync(subfolder, allowedExtensions));
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

        private async Task<StorageFile> GetLaunchPassThemeSettingsFile(StorageFolder folder)
        {
            StorageFile file = null;

            if (folder != null)
            {
                //check if user settings config file exists
                var item = await folder.TryGetItemAsync("LaunchPassUserSettings.xml");

                if (item != null)
                {
                    file = item as StorageFile;
                }
            }

            return file;
        }

        private async void btnApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((App)Application.Current).CurrentThemeSettings != null)
                {
                    bool isFontChanged = ((App)Application.Current).CurrentThemeSettings.Font != Convert.ToString(FontsCB.SelectedItem);

                    ((App)Application.Current).CurrentThemeSettings.Font = Convert.ToString(FontsCB.SelectedItem);

                    if (((App)Application.Current).CurrentThemeSettings.Backgrounds.Background != null && ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.Count() != 0)
                    {
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "MainPage").File = Convert.ToString(MainPageCB.SelectedItem);
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "GamePage").File = Convert.ToString(GamePageCB.SelectedItem);
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "DetailsPage").File = Convert.ToString(DetailsPageCB.SelectedItem);
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "SearchPage").File = Convert.ToString(SearchPageCB.SelectedItem);
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "CustomizePage").File = Convert.ToString(CustomizePageCB.SelectedItem);
                        ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "SettingsPage").File = Convert.ToString(SettingsPageCB.SelectedItem);
                    }

                    string boxArtType = String.Empty;
                    if (toggleBoxFront.IsChecked == true)
                    {
                        boxArtType = Convert.ToString(toggleBoxFront.Content);
                    }
                    else if (toggleBox3d.IsChecked == true)
                    {
                        boxArtType = Convert.ToString(toggleBox3d.Content);
                    }
                    else if (ToggleCartFront.IsChecked == true)
                    {
                        boxArtType = Convert.ToString(ToggleCartFront.Content);
                    }
                    else if (toggleClearLogo.IsChecked == true)
                    {
                        boxArtType = Convert.ToString(toggleClearLogo.Content);
                    }
                    else if (ToggleFanartBackground.IsChecked == true)
                    {
                        boxArtType = Convert.ToString(ToggleFanartBackground.Content);
                    }

                    XmlSerializer x = new XmlSerializer(typeof(LaunchPassThemeSettings));
                    using (TextWriter writer = new StringWriter())
                    {
                        x.Serialize(writer, ((App)Application.Current).CurrentThemeSettings);
                        await FileIO.WriteTextAsync(settingsXMLFile, writer.ToString());
                    }

                    var folderPath = await (await settingsXMLFile.GetParentAsync() as StorageFolder).GetFolderAsync("Backgrounds");
                    var file = await folderPath.GetFileAsync(((App)Application.Current).CurrentThemeSettings.Backgrounds.Background.FirstOrDefault(s => s.Page == "CustomizePage").File);

                    mediaPlayer.MediaPath = file.Path;

                    ((App)Application.Current).CurrentThemeSettings.BoxArtType = boxArtType;
                    // Show the user that fonts were changed and application will be restarted
                    if (isFontChanged)
                    {
                        var msgBox = new MessageDialog("You changed fonts. Application will be restarted!", "LaunchPass");
                        await msgBox.ShowAsync();
                        await CoreApplication.RequestRestartAsync("Application Restart Programmatically.");
                    }
                }
            }
            // Show user that something has gone wrong while applying the settings
            catch (Exception ex)
            {
                var restart_required_msgBox = new MessageDialog("Something went wrong! Could not save the Settings." + Environment.NewLine + ex.Message, "LaunchPass");
                await restart_required_msgBox.ShowAsync();
            }

            // Show user that the Settings have been applied successfully
            var settings_applied_msgBox = new MessageDialog("Your settings have been applied!", "Success");
            await settings_applied_msgBox.ShowAsync();
        }

        private void toggleBoxFront_Checked(object sender, RoutedEventArgs e)
        {
            toggleBox3d.IsChecked = false;
            ToggleCartFront.IsChecked = false;
            toggleClearLogo.IsChecked = false;
            ToggleFanartBackground.IsChecked = false;
        }

        private void toggleBox3d_Checked(object sender, RoutedEventArgs e)
        {
            toggleBoxFront.IsChecked = false;
            ToggleCartFront.IsChecked = false;
            toggleClearLogo.IsChecked = false;
            ToggleFanartBackground.IsChecked = false;
        }

        private void ToggleCartFront_Checked(object sender, RoutedEventArgs e)
        {
            toggleBoxFront.IsChecked = false;
            toggleBox3d.IsChecked = false;
            toggleClearLogo.IsChecked = false;
            ToggleFanartBackground.IsChecked = false;
        }

        private void toggleClearLogo_Checked(object sender, RoutedEventArgs e)
        {
            toggleBoxFront.IsChecked = false;
            toggleBox3d.IsChecked = false;
            ToggleCartFront.IsChecked = false;
            ToggleFanartBackground.IsChecked = false;
        }

        private void ToggleFanartBackground_Checked(object sender, RoutedEventArgs e)
        {
            toggleBoxFront.IsChecked = false;
            toggleBox3d.IsChecked = false;
            ToggleCartFront.IsChecked = false;
            toggleClearLogo.IsChecked = false;
        }
    }
}