using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace RetroPass
{
	public class ThemeManager
	{
		public bool IsInitialized { get; private set; }
		public ElementTheme CurrentMode { get; private set; }
		public Action ThemeInitialization_Finished;

		private static ThemeManager instance = null;

		public static ThemeManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ThemeManager();
				}
				return instance;
			}
		}

		public IEnumerable<string> GetModeNames()
		{
			return Enum.GetNames(typeof(ElementTheme));
		}

		public void Init()
		{
			ChangeMode(ApplicationData.Current.LocalSettings.Values[App.SettingsMode] as string);
			IsInitialized = true;
			ThemeInitialization_Finished?.Invoke();
		}

		public void ChangeMode(string mode)
		{
			if (Enum.GetNames(typeof(ElementTheme)).Contains(mode))
			{
				ElementTheme requestedMode = (ElementTheme)Enum.Parse(typeof(ElementTheme), mode);

				//if the mode is different from current mode and string value matches
				if (Window.Current.Content is FrameworkElement frameworkElement && frameworkElement.RequestedTheme != requestedMode)
				{
					frameworkElement.RequestedTheme = requestedMode;
					CurrentMode = requestedMode;
					ApplicationData.Current.LocalSettings.Values[App.SettingsMode] = mode;
				}
			}
		}
	}
}
