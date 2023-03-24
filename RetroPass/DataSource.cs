using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RetroPass
{
	[XmlRoot(ElementName = "Game")]
	public class GameRetroPass : Game
	{
		public GameRetroPass() : base()
		{

		}
		public GameRetroPass(Game game)
		{
			Title = game.Title;
			DataRootFolder = game.DataRootFolder;
			GamePlatform = game.GamePlatform;
			CoreName = game.CoreName;
			ApplicationPath = game.ApplicationPath;
			ApplicationPathFull = game.ApplicationPathFull;
			BoxFrontFileName = game.BoxFrontFileName;
			BoxFrontContentName = game.BoxFrontContentName;
			VideoTitle = game.VideoTitle;
			ImageFile = game.ImageFile;
			Description = game.Description;
			Developer = game.Developer;
			Publisher = game.Publisher;
			ReleaseDate = game.ReleaseDate;
			Genre = game.Genre;
			RootFolder = Path.GetFileName(DataRootFolder);
		}
		[XmlElement(ElementName = "ApplicationPath")] public override string ApplicationPath { get; set; }
		[XmlElement(ElementName = "Title")] public override string Title { get; set; }
		[XmlElement(ElementName = "CoreName")] public override string CoreName { get; set; }
		[XmlElement(ElementName = "Notes")] public override string Description { get; set; }
		[XmlElement(ElementName = "ReleaseDate")] public override string ReleaseDate { get; set; }
		[XmlElement(ElementName = "Developer")] public override string Developer { get; set; }
		[XmlElement(ElementName = "Publisher")] public override string Publisher { get; set; }
		[XmlElement(ElementName = "Genre")] public override string Genre { get; set; }
		[XmlElement(ElementName = "GamePlatform")] public override Platform GamePlatform { get; set; }
		[XmlElement(ElementName = "RootFolder")] public string RootFolder { get; set; }

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
			BoxFrontFileName = sb.ToString();
			BoxFrontContentName = Path.GetFileNameWithoutExtension(ApplicationPath);
		}

		//public override string BoxFrontFileName { get; set; }
	}

	//for loading LaunchBox platform xml files which have a list of games, in /Data/Platforms directory
	[Serializable, XmlRoot("RetroPass")]
	public class PlaylistRetroPass
	{
		[XmlElement("Game", typeof(GameRetroPass))]
		public List<GameRetroPass> games;
	}

	public abstract class DataSource
	{
		public enum Status
		{
			Unavailable,
			Active,
			Inactive,
		};

		public List<Platform> Platforms = new List<Platform>();
		public List<Playlist> Playlists = new List<Playlist>();
		public Action<Platform> PlatformImported;
		public Action<Playlist> PlaylistImported;

		public string rootFolder;
		public RetroPassConfig retroPassConfig;

		[NonSerialized]
		public Status status;

		public DataSource(string rootFolder, RetroPassConfig retroPassConfig)
		{
			this.rootFolder = rootFolder;
			this.retroPassConfig = retroPassConfig;
		}

		public abstract Task Load();
		public abstract List<string> GetAssets();
	}
}
