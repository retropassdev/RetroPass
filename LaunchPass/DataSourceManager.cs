using LaunchPass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;

namespace RetroPass
{
    public class DataSourceManager
    {
        public enum DataSourceLocation
        {
            None,
            Local,
            Removable
        }

        readonly string ActiveDataSourceLocationKey = "ActiveDataSourceLocationKey";
        readonly string ImportFinishedKey = "ImportFinishedKey";

        public bool ImportFinished
        {
            get
            {
                bool val = true;
                if (ApplicationData.Current.LocalSettings.Values[ImportFinishedKey] != null)
                {
                    val = (bool)ApplicationData.Current.LocalSettings.Values[ImportFinishedKey];
                }
                return val;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[ImportFinishedKey] = value;
            }
        }

        public DataSourceLocation ActiveDataSourceLocation
        {
            get
            {
                DataSourceLocation val = DataSourceLocation.None;
                if (ApplicationData.Current.LocalSettings.Values[ActiveDataSourceLocationKey] != null)
                {
                    val = Enum.Parse<DataSourceLocation>(ApplicationData.Current.LocalSettings.Values[ActiveDataSourceLocationKey] as string);
                }
                return val;
            }
            private set
            {
                ApplicationData.Current.LocalSettings.Values[ActiveDataSourceLocationKey] = value.ToString();
            }
        }

        public Action OnImportStarted;
        public Action<float> OnImportUpdateProgress;
        public Action<bool> OnImportFinished;
        public Action OnImportError;

        private StorageFile removableStorageFile = null;
        private StorageFile localStorageFile = null;
        private DataSource dataSource;

        static CancellationTokenSource tokenSource;

        public DataSourceManager()
        {
            //delete all settings
            //_ = ApplicationData.Current.ClearAsync();
        }

        public async Task ScanDataSource()
        {
            Trace.TraceInformation("DataSourceManager: ScanDataSource");
            localStorageFile = await GetConfigurationFile(DataSourceLocation.Local);
            removableStorageFile = await GetConfigurationFile(DataSourceLocation.Removable);
        }

        private async Task<RetroPassConfig> GetConfiguration(DataSourceLocation location)
        {
            var file = await GetConfigurationFile(location);
            RetroPassConfig configuration = null;

            if (file != null)
            {
                string xmlConfig = await FileIO.ReadTextAsync(file);

                using (TextReader reader = new StringReader(xmlConfig))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RetroPassConfig));
                    // Call the Deserialize method to restore the object's state.
                    configuration = serializer.Deserialize(reader) as RetroPassConfig;
                }
            }
            return configuration;
        }

        private async Task<StorageFile> GetConfigurationFile(DataSourceLocation location)
        {
            StorageFile file = null;

            switch (location)
            {
                case DataSourceLocation.None:
                    break;
                case DataSourceLocation.Local:
                    file = await GetConfigFile(ApplicationData.Current.LocalCacheFolder);
                    break;
                case DataSourceLocation.Removable:
                    // Get the logical root folder for all external storage devices.
                    IReadOnlyList<StorageFolder> removableFolders = await KnownFolders.RemovableDevices.GetFoldersAsync();

                    foreach (var folder in removableFolders)
                    {
                        file = await GetConfigFile(folder);

                        if (file != null)
                        {
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }

            return file;
        }

        public async Task<DataSource> GetDataSource(DataSourceLocation location)
        {
            DataSource dataSource = null;

            switch (location)
            {
                case DataSourceLocation.None:
                    break;
                case DataSourceLocation.Local:
                    if (localStorageFile != null)
                    {
                        dataSource = await GetDataSourceFromConfigurationFile(localStorageFile);
                    }
                    break;
                case DataSourceLocation.Removable:
                    if (removableStorageFile != null)
                    {
                        dataSource = await GetDataSourceFromConfigurationFile(removableStorageFile);
                    }
                    break;
                default:
                    break;
            }

            return dataSource;
        }

        public async void ActivateDataSource(DataSourceLocation location)
        {
            ActiveDataSourceLocation = location;
            dataSource = await GetDataSource(ActiveDataSourceLocation);
            ThumbnailCache.Instance.Set(dataSource, ActiveDataSourceLocation);
        }

        public async Task<DataSource> GetActiveDataSource()
        {
            if (dataSource == null)
            {
                await ScanDataSource();
                dataSource = await GetDataSource(ActiveDataSourceLocation);
                ThumbnailCache.Instance.Set(dataSource, ActiveDataSourceLocation);
            }

            return dataSource;
        }

        public bool HasDataSource(DataSourceLocation dataSourceLocation)
        {
            switch (dataSourceLocation)
            {
                case DataSourceLocation.None:
                    return false;
                case DataSourceLocation.Local:
                    return localStorageFile != null;
                case DataSourceLocation.Removable:
                    return removableStorageFile != null;
                default:
                    return false;
            }
        }

        private async Task<DataSource> GetDataSourceFromConfigurationFile(StorageFile xmlConfigFile)
        {
            DataSource dataSource = null;

            if (xmlConfigFile != null)
            {
                Trace.TraceInformation("DataSourceManager: GetDataSourceFromConfigurationFile {0}", xmlConfigFile.Path);
                string xmlConfig = await FileIO.ReadTextAsync(xmlConfigFile);

                using (TextReader reader = new StringReader(xmlConfig))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RetroPassConfig));
                    // Call the Deserialize method to restore the object's state.
                    RetroPassConfig configuration = serializer.Deserialize(reader) as RetroPassConfig;

                    string rootFolder = Path.Combine(Path.GetDirectoryName(xmlConfigFile.Path), configuration.relativePath);
                    rootFolder = Path.GetFullPath(rootFolder);

                    if (configuration.type == RetroPassConfig.DataSourceType.LaunchBox)
                    {
                        dataSource = new DataSourceLaunchBox(rootFolder, configuration);
                    }
                    else if (configuration.type == RetroPassConfig.DataSourceType.EmulationStation)
                    {
                        dataSource = new DataSourceEmulationStation(rootFolder, configuration);
                    }

                    /*if (dataSource != null)
					{
						string rootFolder = Path.Combine(Path.GetDirectoryName(xmlConfigFile.Path), configuration.relativePath);
						dataSource.rootFolder = Path.GetFullPath(rootFolder);
					}*/
                }
            }

            return dataSource;
        }

        private async Task<StorageFile> GetConfigFile(StorageFolder folder)
        {
            StorageFile file = null;

            if (folder != null)
            {
                //check if the location exists
                var item = await folder.TryGetItemAsync("RetroPass.xml");

                if (item != null)
                {
                    file = item as StorageFile;
                }
            }

            return file;
        }

        public async Task CopyFileAsync(StorageFile source, StorageFolder destinationContainer, bool overwriteIfNewer)
        {
            // Were we already canceled?
            bool copyFile = true;

            if (tokenSource.Token.IsCancellationRequested)
            {
                tokenSource.Token.ThrowIfCancellationRequested();
            }

            //get existing asset
            if (overwriteIfNewer)
            {
                var existingAssetItem = (StorageFile)await destinationContainer.TryGetItemAsync(source.Name);
                if (existingAssetItem != null)
                {
                    BasicProperties existingAssetItemBasicProperties = await existingAssetItem.GetBasicPropertiesAsync();
                    BasicProperties newAssetItemBasicProperties = await source.GetBasicPropertiesAsync();

                    //do not copy file if timestamps are the same
                    if (newAssetItemBasicProperties.DateModified <= existingAssetItemBasicProperties.DateModified)
                    {
                        copyFile = false;
                    }
                }
            }

            if (copyFile == true)
            {
                await source.CopyAsync(destinationContainer, source.Name, NameCollisionOption.ReplaceExisting);
            }
        }

        public async Task CopyFolderAsync(StorageFolder source, StorageFolder destinationContainer, string desiredName = null)
        {
            StorageFolder destinationFolder = null;
            destinationFolder = await destinationContainer.CreateFolderAsync(
                desiredName ?? source.Name, CreationCollisionOption.OpenIfExists);

            foreach (var file in await source.GetFilesAsync())
            {
                await CopyFileAsync(file, destinationFolder, true);
                //await file.CopyAsync(destinationFolder, file.Name, NameCollisionOption.ReplaceExisting);
            }
            foreach (var folder in await source.GetFoldersAsync(CommonFolderQuery.DefaultQuery))
            {
                await CopyFolderAsync(folder, destinationFolder);
            }
        }

        private async Task CopyToLocalFolderAsync()
        {
            // Perform background work here.
            // Don't directly access UI elements from this method.
            DataSource ds = await GetDataSource(DataSourceLocation.Removable);
            await ds.Load();
            string srcPath = ds.rootFolder;
            string destPath = ApplicationData.Current.LocalCacheFolder.Path;

            StorageFolder folderSrc;
            StorageFolder folderDest;
            //find folder
            try
            {
                folderSrc = await StorageUtils.GetFolderFromPathAsync(srcPath);
                folderDest = await StorageUtils.GetFolderFromPathAsync(destPath);
            }
            catch (Exception)
            {
                return;
            }

            List<string> assets = ds.GetAssets();

            //create config file			
            RetroPassConfig configRemovable = await GetConfiguration(DataSourceLocation.Removable);
            RetroPassConfig config = new RetroPassConfig();
            config.relativePath = "./DataSource";
            config.type = configRemovable.type;
            config.retroarch = ds.retroPassConfig.retroarch;

            //save the file to app local directory
            StorageFile filename = await folderDest.CreateFileAsync("RetroPass.xml", CreationCollisionOption.ReplaceExisting);
            XmlSerializer x = new XmlSerializer(typeof(RetroPassConfig));
            using (TextWriter writer = new StringWriter())
            {
                x.Serialize(writer, config);
                await FileIO.WriteTextAsync(filename, writer.ToString());
                localStorageFile = filename;
            }

            var destinationRootFolder = await folderDest.CreateFolderAsync("DataSource", CreationCollisionOption.OpenIfExists);
            var sourceRootFolderPath = ds.rootFolder;
            var sourceRootFolder = await StorageUtils.GetFolderFromPathAsync(sourceRootFolderPath);

            int progress = 0;

            foreach (var asset in assets)
            {
                string dstAssetRelativeDirectoryPath = Path.GetDirectoryName(asset);
                StorageFolder dstAssetFolder = destinationRootFolder;

                IStorageItem assetItem = await sourceRootFolder.TryGetItemAsync(asset);

                if (assetItem == null)
                {
                    continue;
                }

                //create all subdirectories so asset can be coppied into it
                if (string.IsNullOrEmpty(dstAssetRelativeDirectoryPath) == false)
                {
                    dstAssetFolder = await destinationRootFolder.CreateFolderAsync(dstAssetRelativeDirectoryPath, CreationCollisionOption.OpenIfExists);
                }

                if (assetItem is StorageFile)
                {
                    await CopyFileAsync((StorageFile)assetItem, dstAssetFolder, true);
                }
                else if (assetItem is StorageFolder)
                {
                    await CopyFolderAsync(assetItem as StorageFolder, dstAssetFolder);
                }

                progress++;

                OnImportUpdateProgress?.Invoke((float)progress / assets.Count * 100.0f);
            }
        }

        public async Task<bool> CopyToLocalFolder()
        {
            tokenSource = new CancellationTokenSource();
            ImportFinished = false;
            OnImportStarted?.Invoke();

            try
            {
                await Task.Run(() => CopyToLocalFolderAsync(), tokenSource.Token);
                ImportFinished = true;
            }
            catch (Exception)
            {
                //copy failed
                ImportFinished = false;
            }
            finally
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            OnImportFinished?.Invoke(ImportFinished);
            return ImportFinished;
        }

        public void CancelImport()
        {
            tokenSource.Cancel();
        }

        public bool IsImportInProgress()
        {
            return tokenSource != null;
        }

        public async Task DeleteLocalDataSource()
        {
            ImportFinished = true;

            DataSource ds = await GetDataSource(DataSourceLocation.Local);

            if (ds != null)
            {
                await ds.playlistPlayLater.Delete();
            }

            if (ActiveDataSourceLocation == DataSourceLocation.Local)
            {
                ActiveDataSourceLocation = DataSourceLocation.None;
                dataSource = null;
            }

            StorageFolder destPath = ApplicationData.Current.LocalCacheFolder;
            IStorageItem assetItem = await destPath.TryGetItemAsync("DataSource");
            if (assetItem != null)
            {
                await assetItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            IStorageItem configItem = await destPath.TryGetItemAsync("RetroPass.xml");
            if (configItem != null)
            {
                await configItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            //delete cache
            await ThumbnailCache.Instance.Delete(DataSourceLocation.Local);

            localStorageFile = null;
        }

        public async Task PrepareRetroPassUltimateFolder()
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
                        //Check root foler for RetroPass.xml file.
                        IStorageItem configItem = await rootFolder.TryGetItemAsync("RetroPass.xml");
                        if (configItem == null)
                        {
                           
                            //create config file			
                            RetroPassConfig config = new RetroPassConfig();
                            config.relativePath = "./LaunchBox";
                            config.type = RetroPassConfig.DataSourceType.LaunchBox;

                            //save the file to app local directory
                            StorageFile filename = await rootFolder.CreateFileAsync("RetroPass.xml", CreationCollisionOption.ReplaceExisting);
                            XmlSerializer x = new XmlSerializer(typeof(RetroPassConfig));
                            using (TextWriter writer = new StringWriter())
                            {
                                x.Serialize(writer, config);
                                await FileIO.WriteTextAsync(filename, writer.ToString());
                                localStorageFile = filename;
                            }
                        }

                        // Check removable devices for LaunchPass Folder.
                        retroPassUltimateFolderCurrent = await rootFolder.TryGetItemAsync("LaunchPass") as StorageFolder;

                        if (retroPassUltimateFolderCurrent != null)
                        {
                            ((App)Application.Current).RetroPassRootPath = retroPassUltimateFolderCurrent.Path;
                            StorageFile retroPassUltimateXMLfile = await retroPassUltimateFolderCurrent.GetFileAsync("LaunchPass.xml");

                            if (retroPassUltimateXMLfile != null)
                            {
                                string xmlConfig = await FileIO.ReadTextAsync(retroPassUltimateXMLfile);

                                using (TextReader reader = new StringReader(xmlConfig))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(LaunchPassThemeSettings));
                                    ((App)Application.Current).CurrentThemeSettings = (LaunchPassThemeSettings)serializer.Deserialize(reader);
                                }
                            }

                            var fontFolder = await retroPassUltimateFolderCurrent.GetFolderAsync("Fonts");
                            StorageFolder InstallationFolder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "Fonts"));

                            foreach (var file in await fontFolder.GetFilesAsync())
                            {
                                await file.CopyAsync(InstallationFolder,file.Name,NameCollisionOption.ReplaceExisting);
                            }
                        }
                        else
                        {
                            retroPassUltimateFolderCurrent = await rootFolder.CreateFolderAsync("LaunchPass") as StorageFolder;

                            ((App)Application.Current).RetroPassRootPath = retroPassUltimateFolderCurrent.Path;

                            var bgFolder = await retroPassUltimateFolderCurrent.CreateFolderAsync("Backgrounds");
                            var fontFolder = await retroPassUltimateFolderCurrent.CreateFolderAsync("Fonts");

                            //COPY SAMPLE BACKGROUND AND FONT FILES
                            var bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Main-Default.mp4"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Games-Default.mp4"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Details-Default.png"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Search-Default.png"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Customize-Default.mp4"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            bgStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Background/Settings-Default.mp4"));
                            await bgStoreFile.CopyAsync(bgFolder);

                            var fontStoreFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Fonts/Xbox.ttf"));
                            await fontStoreFile.CopyAsync(fontFolder);

                            StorageFolder InstallationFolder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "Fonts"));

                            foreach (var file in await fontFolder.GetFilesAsync())
                            {
                                await file.CopyAsync(InstallationFolder, file.Name, NameCollisionOption.ReplaceExisting);
                            }

                            //CREATE XML FILE FOR THEME SETTINGS
                            StorageFile retroPassUltimateXMLfile = await retroPassUltimateFolderCurrent.CreateFileAsync("LaunchPass.xml", CreationCollisionOption.ReplaceExisting);

                            if (retroPassUltimateXMLfile != null)
                            {
                                LaunchPassThemeSettings retroPassUltimateDefault = new LaunchPassThemeSettings();
                                retroPassUltimateDefault.Font = "Xbox.ttf";

                                retroPassUltimateDefault.Backgrounds = new Backgrounds()
                                {
                                    Background = new List<Background>()
                                    {
                                         new Background() { Page = "MainPage", File = "Main-Default.mp4" },
                                         new Background() { Page = "GamePage", File = "Games-Default.mp4" },
                                         new Background() { Page = "DetailsPage", File = "Details-Default.png" },
                                         new Background() { Page = "SearchPage", File = "Search-Default.png" },
                                         new Background() { Page = "CustomizePage", File = "Customize-Default.mp4" },
                                         new Background() { Page = "SettingsPage", File = "Settings-Default.mp4" },
                                    }
                                };

                                retroPassUltimateDefault.BoxArtType = "Box - Front";

                                XmlSerializer x = new XmlSerializer(typeof(LaunchPassThemeSettings));
                                using (TextWriter writer = new StringWriter())
                                {
                                    x.Serialize(writer, retroPassUltimateDefault);
                                    await FileIO.WriteTextAsync(retroPassUltimateXMLfile, writer.ToString());
                                }

                                ((App)Application.Current).CurrentThemeSettings = retroPassUltimateDefault;
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
