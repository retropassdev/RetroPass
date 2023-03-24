using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace RetroPass
{
	public class PlaylistPlayLater : Playlist
	{
		private string fileName = "PlayLater.xml";

		Dictionary<string, PlaylistItem> PlaylistItemsDict = new Dictionary<string, PlaylistItem>();
		StorageFolder folder = ApplicationData.Current.LocalCacheFolder;

		public PlaylistPlayLater()
		{
			Name = "Play later";
		}

		private string PlaylistItemKey(PlaylistItem playlistItem)
		{
			return playlistItem.game.Title + "_" + playlistItem.game.GamePlatform.Name;
		}

		public bool GameExists(PlaylistItem playlistItem)
		{
			return PlaylistItemsDict.ContainsKey(PlaylistItemKey(playlistItem));
		}

		public void UpdatePlaylistPlayLater(PlaylistItem playlistItem)
		{
			PlaylistItem plExists = PlaylistItemsDict.GetValueOrDefault(PlaylistItemKey(playlistItem));

			if (plExists != null)
			{
				RemoveFromGames(plExists);
			}
			else
			{
				PlaylistItem plItem = new PlaylistItem();
				plItem.playlist = this;
				plItem.game = playlistItem.game;
				AddToGames(plItem);
			}
		}

		private async Task Save()
		{
			//save the file to app local directory
			StorageFile filename = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
			XmlSerializer x = new XmlSerializer(typeof(PlaylistRetroPass));
			using (TextWriter writer = new StringWriter())
			{
				var playlistRetroPass = new PlaylistRetroPass();
				playlistRetroPass.games = PlaylistItems.Select(t => t.game).OfType<GameRetroPass>().ToList();

				//platform paths must be relative when saving
				foreach (var game in playlistRetroPass.games)
				{
					game.GamePlatform.BoxFrontPath = game.GamePlatform.BoxFrontPath == "" ? "" : Path.GetRelativePath(game.DataRootFolder, game.GamePlatform.BoxFrontPath);
					game.GamePlatform.ScreenshotGameplayPath = game.GamePlatform.ScreenshotGameplayPath == "" ? "" : Path.GetRelativePath(game.DataRootFolder, game.GamePlatform.ScreenshotGameplayPath);
					game.GamePlatform.ScreenshotGameSelectPath = game.GamePlatform.ScreenshotGameSelectPath == "" ? "" : Path.GetRelativePath(game.DataRootFolder, game.GamePlatform.ScreenshotGameSelectPath);
					game.GamePlatform.ScreenshotGameTitlePath = game.GamePlatform.ScreenshotGameTitlePath == "" ? "" : Path.GetRelativePath(game.DataRootFolder, game.GamePlatform.ScreenshotGameTitlePath);
					game.GamePlatform.VideoPath = game.GamePlatform.VideoPath == "" ? "" : Path.GetRelativePath(game.DataRootFolder, game.GamePlatform.VideoPath);
				}

				//GameRetroPass[] games = PlaylistItems.Select(t => t.game).ToArray();
				x.Serialize(writer, playlistRetroPass);

				foreach (var game in playlistRetroPass.games)
				{
					game.GamePlatform.BoxFrontPath = game.GamePlatform.BoxFrontPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.BoxFrontPath));
					game.GamePlatform.ScreenshotGameplayPath = game.GamePlatform.ScreenshotGameplayPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameplayPath));
					game.GamePlatform.ScreenshotGameSelectPath = game.GamePlatform.ScreenshotGameSelectPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameSelectPath));
					game.GamePlatform.ScreenshotGameTitlePath = game.GamePlatform.ScreenshotGameTitlePath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameTitlePath));
					game.GamePlatform.VideoPath = game.GamePlatform.VideoPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.VideoPath));
				}

				await FileIO.WriteTextAsync(filename, writer.ToString());
			}
		}

		public async Task Load(List<DataSource> dataSources)
		{
			// if the file doesn't exist
			if (await folder.TryGetItemAsync(fileName) == null)
			{
				return;
			}

			//PlaylistItemsDict.Clear();
			//PlaylistItems.Clear();
			//PlaylistItemsLandingPage.Clear();

			StorageFile filename = await StorageUtils.GetFileAsync(folder, fileName);
			string xmlPlaylist = await FileIO.ReadTextAsync(filename);
			XmlSerializer x = new XmlSerializer(typeof(PlaylistRetroPass));
			PlaylistRetroPass playlistRetroPass = null;

			using (TextReader textReader = new StringReader(xmlPlaylist))
			{
				try
				{
					playlistRetroPass = (PlaylistRetroPass)x.Deserialize(textReader);
				}
				catch (Exception e)
				{
					//if xml is invalid, just delete it and return
					Trace.TraceWarning("PlaylistPlayLater: Delete invalid xml {0}", filename.Path);
					await this.Delete();
				}
			}

			if (playlistRetroPass != null)
			{
				foreach (var game in playlistRetroPass.games)
				{
					//search for proper root folder in all data sources
					//each game in PlayLater playlist has rootFolder which is main folder without a volume
					//for example: e:\\DataSource, rootFolder is DataSource
					//when PlayLater list is loaded immediately on start, we need to find data source with the same root folder
					//and create full dataRootFolder
					DataSource dataSource = dataSources.FirstOrDefault(t => Path.GetFileName(t.rootFolder) == game.RootFolder);

					if(dataSource == null)
					{
						continue;
					}

					string dataRootfolder = dataSource.rootFolder;
					game.DataRootFolder = dataRootfolder;
					//game.GamePlatform = p;//platform read directly from file
					game.GamePlatform.BoxFrontPath = game.GamePlatform.BoxFrontPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.BoxFrontPath));
					game.GamePlatform.ScreenshotGameplayPath = game.GamePlatform.ScreenshotGameplayPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameplayPath));
					game.GamePlatform.ScreenshotGameSelectPath = game.GamePlatform.ScreenshotGameSelectPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameSelectPath));
					game.GamePlatform.ScreenshotGameTitlePath = game.GamePlatform.ScreenshotGameTitlePath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.ScreenshotGameTitlePath));
					game.GamePlatform.VideoPath = game.GamePlatform.VideoPath == "" ? "" : Path.GetFullPath(Path.Combine(game.DataRootFolder, game.GamePlatform.VideoPath));
					game.ApplicationPathFull = Path.GetFullPath(Path.Combine(dataRootfolder, game.ApplicationPath));
					game.Init();

					PlaylistItem playlistItem = AddPlaylistItem(game);
					PlaylistItemsDict.Add(PlaylistItemKey(playlistItem), playlistItem);
				}
			}
		}

		public async Task Delete()
		{
			// if the file doesn't exist
			if (await folder.TryGetItemAsync(fileName) == null)
			{
				return;
			}

			StorageFile filename = await StorageUtils.GetFileAsync(folder, fileName);
			await filename.DeleteAsync(StorageDeleteOption.PermanentDelete);
			//clear recent playlist
			this.ClearLastPlayedSettings();
		}

		private void RemoveFromGames(PlaylistItem plItem)
		{
			PlaylistItems.Remove(plItem);
			PlaylistItemsDict.Remove(PlaylistItemKey(plItem));

			if (PlaylistItemsLandingPage.Contains(plItem))
			{
				//try to just replace the item instead of remove, to avoid fade in ui
				int plItemIndex = PlaylistItemsLandingPage.IndexOf(plItem);
				UpdateGamesLandingPage();
			}
			Save();
		}

		private void AddToGames(PlaylistItem plItem)
		{
			//need to convert game into GameRetroPass
			//this copies values from specific Game subclass to GameRetroPass
			var game = new GameRetroPass(plItem.game);
			game.GamePlatform = plItem.game.GamePlatform.Copy(); //keep copies of GamePlatform object, because it is manipulated on Save()

			PlaylistItem playlistItem = AddPlaylistItem(game);
			PlaylistItemsDict.Add(PlaylistItemKey(playlistItem), playlistItem);

			//refresh landing page
			if (PlaylistItemsLandingPage.Count < 5)
			{
				PlaylistItemsLandingPage.Add(playlistItem);
			}
			Save();
		}
	}
}
