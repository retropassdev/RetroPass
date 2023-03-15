using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RetroPass
{
    [XmlRoot(ElementName = "Game")]
    public class GameLaunchBox : Game
    {
        [XmlElement(ElementName = "ApplicationPath")] public override string ApplicationPath { get; set; }
        [XmlElement(ElementName = "CommandLine")] public string CommandLine { get; set; }
        [XmlElement(ElementName = "Title")] public override string Title { get; set; }
        [XmlElement(ElementName = "Notes")] public override string Description { get; set; }
        [XmlElement(ElementName = "ReleaseDate")] public override string ReleaseDate { get; set; }
        [XmlElement(ElementName = "Developer")] public override string Developer { get; set; }
        [XmlElement(ElementName = "Publisher")] public override string Publisher { get; set; }
        [XmlElement(ElementName = "Genre")] public override string Genre { get; set; }

        [XmlIgnore] public override string ApplicationPathFull { get { return Path.GetFullPath(Path.Combine(DataRootFolder, ApplicationPath)); } }

        public override void Init()
        {
            char replacement = '_';

            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            StringBuilder sb = new StringBuilder(Title);
            var set = new bool[256];
            foreach (var charToReplace in invalidFileNameChars)
            {
                set[charToReplace] = true;
            }
            //set ' also to true
            set['\''] = true;

            for (int i = 0; i < sb.Length; i++)
            {
                var currentCharacter = sb[i];
                if (currentCharacter < 256 && set[currentCharacter])
                {
                    sb[i] = replacement;
                }
            }

            VideoTitle = sb.ToString();
            BoxFrontFileName = sb.ToString();// + "-";
            BoxFrontContentName = Path.GetFileNameWithoutExtension(ApplicationPath);

            //split multiple genre
            Genre = Genre.Replace(";", ", ");

            Trace.TraceInformation("GameRetroPass: BoxFrontFileName Init: {0}" + BoxFrontFileName);
            Trace.TraceInformation("GameRetroPass: BoxFrontContentName Init: {0}" + BoxFrontContentName);
        }
    }

    //for loading LaunchBox playlist xml files in /Data/Playlists directory
    [Serializable, XmlRoot("LaunchBox")]
    public class PlaylistLaunchBox
    {
        public class Playlist
        {
            public string Name;
            public string SortTitle;
        }

        public class PlaylistGame
        {
            public string GameTitle;
            public string GameFileName;
            public string GamePlatform;
        }

        [XmlElement("Playlist")]
        public Playlist _Playlist;
        [XmlElement("PlaylistGame")]
        public PlaylistGame[] PlaylistGames;
    }

    //for loading LaunchBox platform xml files which have a list of games, in /Data/Platforms directory
    [Serializable, XmlRoot("LaunchBox")]
    public class PlaylistPlatformLaunchBox
    {
        [XmlElement("Game", typeof(GameLaunchBox))]
        public List<GameLaunchBox> games;
    }

    //for loading LaunchBox emulator definitions, in Data/Emulators.xml
    [Serializable, XmlRoot("LaunchBox")]
    public class EmulatorsLaunchBox
    {
        public class EmulatorPlatform
        {
            public string Emulator;
            [NonSerialized]
            public string EmulatorPath;
            public string Platform;
            public string CommandLine;
            public bool Default;
        }
        public class Emulator
        {
            public string ApplicationPath;
            public string ID;
        }

        [XmlElement("EmulatorPlatform")]
        public List<EmulatorPlatform> emulatorPlatforms;
        [XmlElement("Emulator")]
        public List<Emulator> emulators;
    }

    //for loading LaunchBox emulator definitions, in Data/Emulators.xml
    [Serializable, XmlRoot("LaunchBox")]
    public class PlatformsLaunchBox
    {
        public class Platform
        {
            public string Name;
            public string SortTitle;
        }

        public class PlatformFolder
        {
            public string MediaType;
            public string FolderPath;
            public string Platform;
        }

        [XmlElement("Platform")]
        public List<Platform> platforms;

        [XmlElement("PlatformFolder")]
        public List<PlatformFolder> platformFolders;
    }

    class DataSourceLaunchBox : DataSource
    {
        public DataSourceLaunchBox(string rootFolder, RetroPassConfig retroPassConfig) : base(rootFolder, retroPassConfig) { }

        public override List<string> GetAssets()
        {
            List<string> assets = new List<string>();

            //assets.Add("LaunchBox.exe");
            assets.Add("Emulators.xml");
            assets.Add("Data\\Emulators.xml");
            assets.Add("Data\\Platforms.xml");
            assets.Add("Data\\Platforms");
            assets.Add("Data\\Playlists");

            foreach (var platform in Platforms)
            {
                //this is full path so remove root
                int rootFolderLength = rootFolder.Length + 1;

                if (platform.BoxFrontPath.Length > rootFolderLength)
                {
                    assets.Add(platform.BoxFrontPath.Substring(rootFolderLength));
                }

                if (platform.ScreenshotGameplayPath.Length > rootFolderLength)
                {
                    assets.Add(platform.ScreenshotGameplayPath.Substring(rootFolderLength));
                }

                if (platform.ScreenshotGameSelectPath.Length > rootFolderLength)
                {
                    assets.Add(platform.ScreenshotGameSelectPath.Substring(rootFolderLength));
                }

                if (platform.ScreenshotGameTitlePath.Length > rootFolderLength)
                {
                    assets.Add(platform.ScreenshotGameTitlePath.Substring(rootFolderLength));
                }

                if (platform.VideoPath.Length > rootFolderLength)
                {
                    assets.Add(platform.VideoPath.Substring(rootFolderLength));
                }
            }

            return assets;
        }

        private string ParseCommandLine(string commandLine)
        {
            string coreName = "";

            if (string.IsNullOrEmpty(commandLine) == false)
            {
                coreName = Path.GetFileName(commandLine).Replace("\"", "");
            }

            return coreName;
        }

        public async Task<List<EmulatorsLaunchBox.EmulatorPlatform>> LoadEmulatorsXml(StorageFolder dataFolder)
        {
            ///////////////////read and parse Emulators.xml///////////////////////////////////////////////
            StorageFile xmlEmulatorsFile = await StorageUtils.GetFileAsync(dataFolder, "Emulators.xml");
            string xmlEmulators = await FileIO.ReadTextAsync(xmlEmulatorsFile);
            List<EmulatorsLaunchBox.EmulatorPlatform> emulatorPlatforms = null;

            using (TextReader reader = new StringReader(xmlEmulators))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EmulatorsLaunchBox));
                // Call the Deserialize method to restore the object's state.
                EmulatorsLaunchBox emulators = serializer.Deserialize(reader) as EmulatorsLaunchBox;
                emulatorPlatforms = emulators.emulatorPlatforms;

                if (emulatorPlatforms != null)
                {
                    emulatorPlatforms.RemoveAll(t => t.Default == false);

                    foreach (var platform in emulatorPlatforms)
                    {
                        EmulatorsLaunchBox.Emulator emulator = emulators.emulators.Find(t => t.ID == platform.Emulator);

                        if (emulator != null)
                        {
                            platform.EmulatorPath = emulator.ApplicationPath;
                        }
                        else
                        {
                            Trace.TraceWarning("DataSourceLaunchBox: Emulator for " + platform.Platform + " not specified.");
                        }
                    }
                }
            }

            return emulatorPlatforms;
        }

        public async Task<PlatformsLaunchBox> LoadPlatformsXml(StorageFolder dataFolder)
        {
            ///////////////////read and parse Platforms.xml///////////////////////////////////////////////
            StorageFile xmlPlatformsFile = await StorageUtils.GetFileAsync(dataFolder, "Platforms.xml");
            //get folders
            string xmlPlatforms = await FileIO.ReadTextAsync(xmlPlatformsFile);
            PlatformsLaunchBox platforms = null;
            using (TextReader reader = new StringReader(xmlPlatforms))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlatformsLaunchBox));
                // Call the Deserialize method to restore the object's state.
                platforms = serializer.Deserialize(reader) as PlatformsLaunchBox;
            }

            return platforms;
        }

        public async Task<List<PlaylistLaunchBox>> LoadPlaylistsXmls(StorageFolder dataFolder)
        {
            List<PlaylistLaunchBox> playlistLaunchBoxList = new List<PlaylistLaunchBox>();

            StorageFolder playlistsFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder + "\\Data\\Playlists");
            IReadOnlyList<StorageFile> playlistFiles = await playlistsFolder.GetFilesAsync();
            foreach (StorageFile xmlPlaylistFile in playlistFiles)
            {
                string xmlPlaylist = await FileIO.ReadTextAsync(xmlPlaylistFile);

                using (TextReader reader = new StringReader(xmlPlaylist))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PlaylistLaunchBox));
                    // Call the Deserialize method to restore the object's state.
                    PlaylistLaunchBox playlistLaunchBox = serializer.Deserialize(reader) as PlaylistLaunchBox;
                    playlistLaunchBoxList.Add(playlistLaunchBox);
                }
            }

            return playlistLaunchBoxList;
        }

        private void AddToDictionaryList<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new List<TValue>());
            }

            dictionary[key].Add(value);
        }

        public List<object> JoinPlaylists(PlatformsLaunchBox platformsLaunchBox, List<PlaylistLaunchBox> playlistsLaunchbox)
        {
            SortedDictionary<string, List<object>> sortedJoinedPlaylists = new SortedDictionary<string, List<object>>();

            foreach (var platform in platformsLaunchBox.platforms)
            {
                if (string.IsNullOrEmpty(platform.SortTitle))
                {
                    AddToDictionaryList(sortedJoinedPlaylists, platform.Name, platform);
                }
                else
                {
                    AddToDictionaryList(sortedJoinedPlaylists, platform.SortTitle, platform);
                }
            }

            foreach (var playlistLaunchbox in playlistsLaunchbox)
            {
                if (string.IsNullOrEmpty(playlistLaunchbox._Playlist.SortTitle))
                {
                    AddToDictionaryList(sortedJoinedPlaylists, playlistLaunchbox._Playlist.Name, playlistLaunchbox);
                }
                else
                {
                    AddToDictionaryList(sortedJoinedPlaylists, playlistLaunchbox._Playlist.SortTitle, playlistLaunchbox);
                }
            }

            List<object> joinedPlaylists = sortedJoinedPlaylists.SelectMany(x => x.Value).ToList();

            return joinedPlaylists;
        }

        public async Task<Playlist> LoadLaunchBoxPlatform(string platformName, PlatformsLaunchBox platforms, List<EmulatorsLaunchBox.EmulatorPlatform> emulatorPlatforms, IReadOnlyList<StorageFile> platformsFiles)
        {
            //this can actually never happen, because all platforms that are not in Emulators.xml are already removed 
            var emulatorPlatform = emulatorPlatforms.Find(t => t.Platform == platformName);
            if (emulatorPlatform == null)
            {
                return null;
            }

            StorageFile xmlPlatformFile = platformsFiles.FirstOrDefault(t => t.Name == platformName + ".xml");

            //this can actually never happen, because all platforms that don't have <platform>.xml are already removed 
            if (xmlPlatformFile == null)
            {
                return null;
            }

            string coreName = ParseCommandLine(emulatorPlatform.CommandLine);

            var platform = new Platform();
            platform.Name = platformName;
            platform.SourceName = platform.Name;
            platform.SetEmulatorType(emulatorPlatform.EmulatorPath);

            platform.BoxFrontPath = platforms.platformFolders.Where(t => t.Platform == platformName && t.MediaType == ((App)Application.Current).CurrentThemeSettings.BoxArtType).Select(t => t.FolderPath).DefaultIfEmpty(string.Empty).First();
            platform.BoxFrontPath = platform.BoxFrontPath == "" ? "" : Path.GetFullPath(Path.Combine(rootFolder, platform.BoxFrontPath));
            Trace.TraceInformation("DataSourceLaunchBox: Platform BoxFrontPath: {0}", platform.BoxFrontPath);

            //find folder
            try
            {
                platform.BoxFrontFolder = await StorageUtils.GetFolderFromPathAsync(platform.BoxFrontPath);
            }
            catch (Exception)
            {
                //return null;
            }

            platform.ScreenshotGameTitlePath = platforms.platformFolders.Where(t => t.Platform == platformName && t.MediaType == "Screenshot - Game Title").Select(t => t.FolderPath).DefaultIfEmpty(string.Empty).First();
            platform.ScreenshotGameTitlePath = platform.ScreenshotGameTitlePath == "" ? "" : Path.GetFullPath(Path.Combine(rootFolder, platform.ScreenshotGameTitlePath));
            Trace.TraceInformation("DataSourceLaunchBox: Platform ScreenshotGameTitlePath: {0}", platform.ScreenshotGameTitlePath);

            platform.ScreenshotGameplayPath = platforms.platformFolders.Where(t => t.Platform == platformName && t.MediaType == "Screenshot - Gameplay").Select(t => t.FolderPath).DefaultIfEmpty(string.Empty).First();
            platform.ScreenshotGameplayPath = platform.ScreenshotGameplayPath == "" ? "" : Path.GetFullPath(Path.Combine(rootFolder, platform.ScreenshotGameplayPath));
            Trace.TraceInformation("DataSourceLaunchBox: Platform ScreenshotGameplayPath: {0}", platform.ScreenshotGameplayPath);

            platform.ScreenshotGameSelectPath = platforms.platformFolders.Where(t => t.Platform == platformName && t.MediaType == "Screenshot - Game Select").Select(t => t.FolderPath).DefaultIfEmpty(string.Empty).First();
            platform.ScreenshotGameSelectPath = platform.ScreenshotGameSelectPath == "" ? "" : Path.GetFullPath(Path.Combine(rootFolder, platform.ScreenshotGameSelectPath));
            Trace.TraceInformation("DataSourceLaunchBox: Platform ScreenshotGameSelectPath: {0}", platform.ScreenshotGameSelectPath);

            platform.VideoPath = platforms.platformFolders.Where(t => t.Platform == platformName && t.MediaType == "Video").Select(t => t.FolderPath).DefaultIfEmpty(string.Empty).First();
            platform.VideoPath = platform.VideoPath == "" ? "" : Path.GetFullPath(Path.Combine(rootFolder, platform.VideoPath));
            Trace.TraceInformation("DataSourceLaunchBox: Platform VideoPath: {0}", platform.VideoPath);

            Platforms.Add(platform);
            PlatformImported?.Invoke(platform);

            ///////////////////read and parse games///////////////////////////////////////////////
            string xmlPlatform = await FileIO.ReadTextAsync(xmlPlatformFile);
            var playlistTmp = new Playlist();

            // GET PLAYLIST PLATFORM IMAGE
            StorageFolder platformImageFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder + "\\Images\\Platforms\\" + platformName + "\\Clear Logo");
            List<string> fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".jpeg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".webp");
            QueryOptions queryOptions = new QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter);
            StorageFileQueryResult queryResult = platformImageFolder.CreateFileQueryWithOptions(queryOptions);
            var files = await queryResult.GetFilesAsync();
            StorageFile imageFile = files != null && files.Count() > 0 ? files[0] : null;
            if (imageFile != null)
                playlistTmp.Thumbnail = await ThumbnailCache.Instance.GetThumbnailAsync(imageFile);

            using (TextReader reader = new StringReader(xmlPlatform))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlaylistPlatformLaunchBox));
                PlaylistPlatformLaunchBox platformGames = serializer.Deserialize(reader) as PlaylistPlatformLaunchBox;

                playlistTmp.Name = platformName;

                foreach (var game in platformGames.games)
                {
                    game.DataRootFolder = rootFolder;
                    game.GamePlatform = platform;

                    //override core per game
                    if (string.IsNullOrEmpty(game.CommandLine) == false)
                    {
                        game.CoreName = ParseCommandLine(game.CommandLine);
                    }
                    else
                    {
                        game.CoreName = coreName;
                    }

                    game.Init();
                    //Trace.TraceInformation("DataSourceLaunchBox: Add game: {0}", game.ApplicationPath);
                    playlistTmp.AddPlaylistItem(game);
                }
            }

            playlistTmp.Sort();
            playlistTmp.UpdateGamesLandingPage();
            Playlists.Add(playlistTmp);
            return playlistTmp;
        }

        public override async Task Load()
        {
            ///////////////////load Play later playlist///////////////////////////////////////////////
            await playlistPlayLater.Load(rootFolder);
            playlistPlayLater.UpdateGamesLandingPage();
            Playlists.Add(playlistPlayLater);
            PlaylistImported?.Invoke(playlistPlayLater);

            //TODO: add platforms from playlistPlayLater to all platforms, so we don't repeat them
            StorageFolder dataFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder + "\\Data");


            ///////////////////read and parse Emulators.xml///////////////////////////////////////////////
            List<EmulatorsLaunchBox.EmulatorPlatform> emulatorPlatforms = await LoadEmulatorsXml(dataFolder);

            ///////////////////read and parse Platforms.xml///////////////////////////////////////////////
            PlatformsLaunchBox platformsLaunchBox = await LoadPlatformsXml(dataFolder);

            if (emulatorPlatforms == null || platformsLaunchBox == null)
            {
                return;
            }

            List<PlaylistLaunchBox> playlistsLaunchbox = await LoadPlaylistsXmls(dataFolder);

            //get all platform xml files
            StorageFolder platformFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder + "\\Data\\Platforms");
            IReadOnlyList<StorageFile> platformsFiles = await platformFolder.GetFilesAsync();

            //remove platforms that don't have en entry in emulator platforms
            platformsLaunchBox.platforms.RemoveAll(i => emulatorPlatforms.FindIndex(t => t.Platform == i.Name) == -1);

            //remove platforms that don't have a valid platform xml file
            platformsLaunchBox.platforms.RemoveAll(i => platformsFiles.FirstOrDefault(t => t.Name == i.Name + ".xml") == null);

            //join and sort platforms and playlists using "SortTitle" property			
            List<object> joinedPlaylists = JoinPlaylists(platformsLaunchBox, playlistsLaunchbox);

            //load one by one
            foreach (var playlist in joinedPlaylists)
            {
                Playlist playlistTmp = null;

                if (playlist is PlatformsLaunchBox.Platform)
                {
                    PlatformsLaunchBox.Platform platformLaunchBox = playlist as PlatformsLaunchBox.Platform;
                    //load platform if it is not already loade
                    //if (Platforms.Exists(p => p.Name == platformLaunchBox.Name) == false)
                    //{
                        //launchbox platforms are loaded as Playlists
                        playlistTmp = await LoadLaunchBoxPlatform(platformLaunchBox.Name, platformsLaunchBox, emulatorPlatforms, platformsFiles);
                        if (playlistTmp == null)
                        {
                            continue;
                        }
                    //}
                    ////Platform can be loaded already. For example, launchbox playlist called "1st playlist" has a game that's from "SNES" platform playlist.
                    ////"SNES" platform playlist will already be loaded, because "1st playlist" is processed earlier when going through joinedPlaylists dictionary.
                    //else
                    //{
                    //    playlistTmp = Playlists.FirstOrDefault(t => t.Name == platformLaunchBox.Name);
                    //}
                }
                else if (playlist is PlaylistLaunchBox)
                {
                    PlaylistLaunchBox playlistLaunchBox = playlist as PlaylistLaunchBox;
                    //find all platform playlists that must be loaded for this launchbox playlist
                    IEnumerable<string> platformNames = playlistLaunchBox.PlaylistGames.Select(x => x.GamePlatform).Distinct();

                    foreach (string platformName in platformNames)
                    {
                        //If platform playlist is not already loaded, load before loading games from launchbox playlists
                        if (Playlists.Exists(t => t.Name == platformName) == false)
                        {
                            //if platform playlist is not loaded for some reason, games in the launchbox playlist will be skipped later
                            await LoadLaunchBoxPlatform(platformName, platformsLaunchBox, emulatorPlatforms, platformsFiles);
                        }
                    }

                    playlistTmp = new Playlist();
                    playlistTmp.Name = playlistLaunchBox._Playlist.Name;

                    // GET PLAYLIST IMAGE
                    StorageFolder platformImageFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder + "\\Images\\Playlists\\" + playlistTmp.Name);
                    List<string> fileTypeFilter = new List<string>();
                    fileTypeFilter.Add(".jpg");
                    fileTypeFilter.Add(".jpeg");
                    fileTypeFilter.Add(".png");
                    fileTypeFilter.Add(".webp");
                    QueryOptions queryOptions = new QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter);
                    StorageFileQueryResult queryResult = platformImageFolder.CreateFileQueryWithOptions(queryOptions);
                    var files = await queryResult.GetFilesAsync();
                    StorageFile imageFile = files != null && files.Count() > 0 ? files[0] : null;
                    if (imageFile != null)
                        playlistTmp.Thumbnail = await ThumbnailCache.Instance.GetThumbnailAsync(imageFile);

                    foreach (var playlistGameLaunchBox in playlistLaunchBox.PlaylistGames)
                    {
                        var gamePlatform = playlistGameLaunchBox.GamePlatform;
                        var gameFileName = playlistGameLaunchBox.GameFileName;
                        //find a game in already loaded Platform playlists
                        Playlist playlistPlatform = Playlists.Find(t => t.Name == gamePlatform);

                        if (playlistPlatform == null)
                        {
                            continue;

                        }
                        //add only if the game with the same file name exists in one of platforms
                        PlaylistItem playlistItemPlatform = playlistPlatform.PlaylistItems.FirstOrDefault(t => t.game.ApplicationPath.EndsWith(gameFileName));

                        if (playlistItemPlatform != null)
                        {
                            playlistTmp.AddPlaylistItem(playlistItemPlatform.game);
                        }
                    }

                    //show playlist only if there is at least one game
                    if (string.IsNullOrEmpty(playlistTmp.Name) == false && playlistTmp.PlaylistItems.Count > 0)
                    {
                        playlistTmp.UpdateGamesLandingPage();
                        Playlists.Add(playlistTmp);
                    }
                }

                if (playlistTmp != null)
                {
                    PlaylistImported?.Invoke(playlistTmp);
                }
            }
        }
    }
}

