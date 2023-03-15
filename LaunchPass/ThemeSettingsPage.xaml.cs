using RetroPass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
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
        StorageFile settingsXMLFile;

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

        private async void LoadBackgroundsAndFonts()
        {
            try
            {
                var removableDevices = KnownFolders.RemovableDevices;
                var folders = await removableDevices.GetFoldersAsync();

                StorageFolder retroPassUltimateFolderCurrent = null;

                foreach (StorageFolder rootFolder in folders)
                {
                    //FIND LAUNCHBOX FOLDER TO RELATED RETROPASS FOLDER ON THE SAME REMOVABLE DEVICE
                    StorageFolder launchBoxFolder = await rootFolder.TryGetItemAsync("LaunchBox") as StorageFolder;

                    if (launchBoxFolder != null)
                    {
                        // Check removable devices for LaunchPass Folder.
                        retroPassUltimateFolderCurrent = await rootFolder.TryGetItemAsync("LaunchPass") as StorageFolder;

                        settingsXMLFile = await GetRPUThemeSettingsFile(retroPassUltimateFolderCurrent);

                        var backgroundFiles = await GetFilesAsync(retroPassUltimateFolderCurrent, "Backgrounds", new List<string>() { ".png", ".jpg", ".mp4", ".mpg", ".MOV", ".avif", ".webp" });
                        List<String> backgroundsFilesNameList = backgroundFiles.Select(s => s.Name).ToList();

                        MainPageCB.ItemsSource = backgroundsFilesNameList;
                        GamePageCB.ItemsSource = backgroundsFilesNameList;
                        DetailsPageCB.ItemsSource = backgroundsFilesNameList;
                        SearchPageCB.ItemsSource = backgroundsFilesNameList;
                        CustomizePageCB.ItemsSource = backgroundsFilesNameList;
                        SettingsPageCB.ItemsSource = backgroundsFilesNameList;

                        var fontFiles = await GetFilesAsync(retroPassUltimateFolderCurrent, "Fonts", new List<string>() { ".ttf", ".otf" });
                        FontsCB.ItemsSource = fontFiles.Select(s => s.Name).ToList();

                        break;
                    }
                }

                if (((App)Application.Current).CurrentThemeSettings != null)
                {
                    FontsCB.SelectedItem = ((App)Application.Current).CurrentThemeSettings.Font;

                    List<Background> backgroundList = ((App)Application.Current).CurrentThemeSettings.Backgrounds.Background;
                    if (backgroundList != null && backgroundList.Count() != 0)
                    {
                        MainPageCB.SelectedItem = backgroundList.Where(s => s.Page == "MainPage").Select(s => s.File).FirstOrDefault();
                        GamePageCB.SelectedItem = backgroundList.Where(s => s.Page == "GamePage").Select(s => s.File).FirstOrDefault();
                        DetailsPageCB.SelectedItem = backgroundList.Where(s => s.Page == "DetailsPage").Select(s => s.File).FirstOrDefault();
                        SearchPageCB.SelectedItem = backgroundList.Where(s => s.Page == "SearchPage").Select(s => s.File).FirstOrDefault();
                        CustomizePageCB.SelectedItem = backgroundList.Where(s => s.Page == "CustomizePage").Select(s => s.File).FirstOrDefault();
                        SettingsPageCB.SelectedItem = backgroundList.Where(s => s.Page == "SettingsPage").Select(s => s.File).FirstOrDefault();
                    }

                    string boxArtType = ((App)Application.Current).CurrentThemeSettings.BoxArtType;

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
            catch (Exception)
            {
                //throw;
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

        private async Task<StorageFile> GetRPUThemeSettingsFile(StorageFolder folder)
        {
            StorageFile file = null;

            if (folder != null)
            {
                //check if the location exists
                var item = await folder.TryGetItemAsync("LaunchPass.xml");

                if (item != null)
                {
                    file = item as StorageFile;
                }
            }

            return file;
        }

        async private void btnApplyChanges_Click(object sender, RoutedEventArgs e)
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

                    if (isFontChanged)
                    {
                        var msgBox = new MessageDialog("You changed fonts. Application will be restarted!", "RetroPass Utlimate");
                        await msgBox.ShowAsync();
                        await CoreApplication.RequestRestartAsync("Application Restart Programmatically.");
                    }

                }
            }
            catch (Exception ex)
            {
                var msgBox = new MessageDialog("Something went wrong! Could not save the Settings." + Environment.NewLine + ex.Message, "RetroPass Utlimate");
                await msgBox.ShowAsync();
            }
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



