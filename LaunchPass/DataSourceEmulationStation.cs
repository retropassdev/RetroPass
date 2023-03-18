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

namespace RetroPass
{
    //for loading ES gamelist xml files which have a list of games, in /<platform>/ directory
    [Serializable, XmlRoot("gameList")]
    public class PlaylistPlatformES
    {
        [XmlElement("game")]
        public List<GameES> games;
    }

    [Serializable, XmlRoot("systemList")]
    public class SystemListEmulatorsES
    {
        public class System
        {
            public string name;
            public string fullname;
            public string path;
            public string command;
        }

        [XmlElement("system")]
        public List<System> systems;
    }

    [XmlRoot(ElementName = "game")]
    public class GameES : Game
    {
        [XmlElement(ElementName = "path")] public override string ApplicationPathFull { get; set; }
        [XmlElement(ElementName = "name")] public override string Title { get; set; }
        [XmlElement(ElementName = "thumbnail")] public string Thumbnail { get; set; }
        [XmlElement(ElementName = "image")] public string Screenshot { get; set; }
        [XmlElement(ElementName = "video")] public string Video { get; set; }
        [XmlElement(ElementName = "desc")] public override string Description { get; set; }
        [XmlElement(ElementName = "releasedate")] public override string ReleaseDate { get; set; }
        [XmlElement(ElementName = "developer")] public override string Developer { get; set; }
        [XmlElement(ElementName = "publisher")] public override string Publisher { get; set; }
        [XmlElement(ElementName = "genre")] public override string Genre { get; set; }

        //public override string BoxFrontFileName { get { return Path.GetFileName(Thumbnail); } }

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
            //BoxFrontFileName = sb.ToString();
            if (string.IsNullOrEmpty(Thumbnail) == false)
            {
                BoxFrontFileName = Path.GetFileNameWithoutExtension(Thumbnail);
            }
            else if (string.IsNullOrEmpty(Screenshot) == false)
            {
                BoxFrontFileName = Path.GetFileNameWithoutExtension(Screenshot);
            }
            BoxFrontContentName = Path.GetFileNameWithoutExtension(ApplicationPath);

            Trace.TraceInformation("GameES: Title Init: {0}" + BoxFrontFileName);
            Trace.TraceInformation("GameES: BoxFrontContentName Init: {0}" + BoxFrontContentName);
        }
    }

    internal class DataSourceEmulationStation : DataSource
    {
        public DataSourceEmulationStation(string rootFolder, RetroPassConfig retroPassConfig) : base(rootFolder, retroPassConfig)
        {
        }

        public override List<string> GetAssets()
        {
            List<string> assets = new List<string>();

            //assets.Add("media");
            //assets.Add("playlists");

            foreach (var platform in Platforms)
            {
                //this is full path so remove root
                int rootFolderLength = rootFolder.Length + 1;

                string gamelistPath = Path.Combine(rootFolder, platform.SourceName, "gamelist.xml");
                assets.Add(gamelistPath.Substring(rootFolderLength));

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

        public override async Task Load()
        {
            ///////////////////load Play later playlist///////////////////////////////////////////////
            await playlistPlayLater.Load(rootFolder);
            playlistPlayLater.UpdateGamesLandingPage();
            Playlists.Add(playlistPlayLater);
            PlaylistImported?.Invoke(playlistPlayLater);

            StorageFolder dataFolder = await StorageUtils.GetFolderFromPathAsync(rootFolder);
            var playlistFolders = await dataFolder.GetFoldersAsync();

            //find es_systems.cfg file
            SystemListEmulatorsES systemsList = null;
            QueryOptions options = new QueryOptions();
            options.ApplicationSearchFilter = "System.FileName:~\"" + "es_systems.cfg" + "\"";
            options.FolderDepth = FolderDepth.Deep;
            StorageFileQueryResult queryResult = dataFolder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> sortedFiles = await queryResult.GetFilesAsync();
            if (sortedFiles.Count > 0)
            {
                string xmlSystems = await FileIO.ReadTextAsync(sortedFiles[0]);
                using (TextReader reader = new StringReader(xmlSystems))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SystemListEmulatorsES));
                    systemsList = serializer.Deserialize(reader) as SystemListEmulatorsES;
                }
            }
            else
            {
                Trace.TraceError("es_systems.cfg not found in " + dataFolder.Path);
                return;
            }

            //find gamelist.xml files
            options = new QueryOptions();
            options.ApplicationSearchFilter = "System.FileName:~\"" + "gamelist.xml" + "\"";
            options.FolderDepth = FolderDepth.Deep;
            queryResult = dataFolder.CreateFileQueryWithOptions(options);
            IReadOnlyList<StorageFile> gamelistFiles = await queryResult.GetFilesAsync();

            ///////////////////read and parse games for each platform///////////////////////////////////////////////
            foreach (var gamelistFile in gamelistFiles)
            {
                //find the platform this gamelist belongs to
                string platformName = Path.GetFileName(Path.GetDirectoryName(gamelistFile.Path));

                //find the system based on the platform name
                SystemListEmulatorsES.System system = systemsList.systems.Find(t => t.name == platformName);

                if (system == null)
                {
                    continue;
                }

                char[] chars = { '\\', '/' };
                int index = system.command.IndexOf(".dll");
                if (index == -1)
                {
                    continue;
                }
                int indexStart = system.command.LastIndexOfAny(chars, index);
                if (indexStart == -1)
                {
                    continue;
                }
                string coreName = system.command.Substring(indexStart + 1, index + 3 - indexStart);

                //////////////////////////////////import platform games as playlist/////////////////////////
                string xmlPlatform = await FileIO.ReadTextAsync(gamelistFile);

                var playlistTmp = new Playlist();
                using (TextReader reader = new StringReader(xmlPlatform))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PlaylistPlatformES));
                    PlaylistPlatformES platformGames = serializer.Deserialize(reader) as PlaylistPlatformES;

                    //////////////////////////////////create platform/////////////////////////
                    Platform platform = null;

                    //init platform just once
                    if (platform == null)
                    {
                        platform = new Platform();
                        platform.Name = system.fullname;
                        platform.SourceName = system.name;
                        platform.SetEmulatorType(system.command);

                        string platformPath = dataFolder.Path;

                        //try to find path in any of the games
                        platform.BoxFrontPath = platformGames.games.Where(t => string.IsNullOrEmpty(t.Thumbnail) == false).Select(t => t.Thumbnail).DefaultIfEmpty(string.Empty).First();

                        //if game list file doesn't use thumbnails, try screenshot
                        if (string.IsNullOrEmpty(platform.BoxFrontPath))
                        {
                            platform.BoxFrontPath = platformGames.games.Where(t => string.IsNullOrEmpty(t.Screenshot) == false).Select(t => t.Screenshot).DefaultIfEmpty(string.Empty).First();
                        }

                        platform.BoxFrontPath = platform.BoxFrontPath == "" ? "" : Path.GetFullPath(platformPath + Path.GetDirectoryName(platform.BoxFrontPath));
                        Trace.TraceInformation("DataSourceEmulationStation: Platform BoxFrontPath: {0}", platform.BoxFrontPath);

                        //find folder
                        platform.BoxFrontFolder = await StorageUtils.GetFolderFromPathAsync(platform.BoxFrontPath);

                        platform.ScreenshotGameTitlePath = platformGames.games.Where(t => string.IsNullOrEmpty(t.Screenshot) == false).Select(t => t.Screenshot).DefaultIfEmpty(string.Empty).First();
                        platform.ScreenshotGameTitlePath = platform.ScreenshotGameTitlePath == "" ? "" : Path.GetFullPath(platformPath + Path.GetDirectoryName(platform.ScreenshotGameTitlePath));
                        Trace.TraceInformation("DataSourceEmulationStation: Platform ScreenshotGameTitlePath: {0}", platform.ScreenshotGameTitlePath);

                        platform.ScreenshotGameplayPath = "";
                        platform.ScreenshotGameSelectPath = "";

                        platform.VideoPath = platformGames.games.Where(t => string.IsNullOrEmpty(t.Video) == false).Select(t => t.Video).DefaultIfEmpty(string.Empty).First();
                        platform.VideoPath = platform.VideoPath == "" ? "" : Path.GetFullPath(platformPath + Path.GetDirectoryName(platform.VideoPath));
                        Trace.TraceInformation("DataSourceEmulationStation: Platform VideoPath: {0}", platform.VideoPath);

                        Platforms.Add(platform);
                        PlatformImported?.Invoke(platform);
                    }

                    string retroArchRomPlatformPath = "";
                    if (retroPassConfig.retroarch != null && string.IsNullOrEmpty(retroPassConfig.retroarch.romPath) == false)
                    {
                        var mapping = retroPassConfig.retroarch.dirMappings.FirstOrDefault(t => t.src == platform.SourceName);

                        if (mapping != null)
                        {
                            retroArchRomPlatformPath = Path.Combine(retroPassConfig.retroarch.romPath, mapping.dst);
                        }
                    }

                    playlistTmp.Name = platform.Name;
                    foreach (var game in platformGames.games)
                    {
                        game.ApplicationPath = game.ApplicationPathFull;

                        //if user wants to use roms from retroarch, create a proper path from RetroPass config settings
                        if (string.IsNullOrEmpty(retroArchRomPlatformPath) == false)
                        {
                            string fileName = Path.GetFileName(game.ApplicationPathFull);
                            game.ApplicationPathFull = Path.Combine(retroArchRomPlatformPath, fileName);
                        }
                        else
                        {
                            game.ApplicationPathFull = Path.GetFullPath(Path.Combine(rootFolder, game.ApplicationPathFull));
                        }

                        game.DataRootFolder = rootFolder;
                        game.GamePlatform = platform;
                        //set default path for this playlist;
                        game.CoreName = coreName;
                        game.Init();
                        Trace.TraceInformation("DataSourceEmulationStation: Add game: {0}", game.ApplicationPath);
                        playlistTmp.AddPlaylistItem(game);
                    }
                }
                playlistTmp.Sort();
                playlistTmp.UpdateGamesLandingPage();
                Playlists.Add(playlistTmp);
                PlaylistImported?.Invoke(playlistTmp);
            }
        }
    }
}