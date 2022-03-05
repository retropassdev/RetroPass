using System;
using System.Collections.Generic;
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
				//GameRetroPass[] games = PlaylistItems.Select(t => t.game).ToArray();
				x.Serialize(writer, playlistRetroPass);
				await FileIO.WriteTextAsync(filename, writer.ToString());
			}
		}

		public async Task Load(string dataRootfolder)
		{
			// if the file doesn't exist
			if (await folder.TryGetItemAsync(fileName) == null)
			{
				return;
			}

			StorageFile filename = await StorageUtils.GetFileAsync(folder, fileName);
			string xmlPlaylist = await FileIO.ReadTextAsync(filename);
			XmlSerializer x = new XmlSerializer(typeof(PlaylistRetroPass));

			using (TextReader textReader = new StringReader(xmlPlaylist))
			{
				var playlistRetroPass = (PlaylistRetroPass)x.Deserialize(textReader);

				foreach (var game in playlistRetroPass.games)
				{
					game.DataRootFolder = dataRootfolder;
					//game.GamePlatform = p;//platform read directly from file
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
			PlaylistItem playlistItem = new PlaylistItem();
			//this copies values from specific Game subclass to GameRetroPass
			playlistItem.game = new GameRetroPass(plItem.game);

			PlaylistItems.Add(playlistItem);
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
