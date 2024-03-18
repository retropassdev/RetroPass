using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RetroPass.SettingsPages
{
	public sealed partial class SettingsPersonalizationPage : Page
	{
		public SettingsPersonalizationPage()
		{
			this.InitializeComponent();

			this.Loaded += SettingsPersonalizationPage_Loaded;
		}

		private void SettingsPersonalizationPage_Loaded(object sender, RoutedEventArgs e)
		{
			AutoPlayVideoCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo];
			PlayFullScreenVideoCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsPlayFullScreenVideo];

			var buttonsMuteVideo = this.RadioButtonsMuteVideo.Children.OfType<RadioButton>();
			string currentMuteVideo = (string)ApplicationData.Current.LocalSettings.Values[App.SettingsMuteVideo];
			var selectedButtonMuteVideo = buttonsMuteVideo.FirstOrDefault(t => t.Tag as string == currentMuteVideo);
			if (selectedButtonMuteVideo != null)
			{
				selectedButtonMuteVideo.IsChecked = true;
			}

			//set first selected
			var buttonsMode = this.RadioButtonsMode.Children.OfType<RadioButton>();
			var selectedButtonMode = buttonsMode.FirstOrDefault(t => t.Tag as string == ThemeManager.Instance.CurrentMode.ToString());
			if (selectedButtonMode != null)
			{
				selectedButtonMode.IsChecked = true;
			}

			var buttonsMainPageLayout = this.RadioButtonsMainPageLayout.Children.OfType<RadioButton>();
			string currentMainPageLayout = (string)ApplicationData.Current.LocalSettings.Values[App.SettingsMainPageLayout];
			var selectedButtonMainPageLayout = buttonsMainPageLayout.FirstOrDefault(t => t.Tag as string == currentMainPageLayout);
			if (selectedButtonMainPageLayout != null)
			{
				selectedButtonMainPageLayout.IsChecked = true;
			}

			var buttonsCollectionsPageLayout = this.RadioButtonsCollectionPageLayout.Children.OfType<RadioButton>();
			string currentCollectionPageLayout = (string)ApplicationData.Current.LocalSettings.Values[App.SettingsCollectionPageLayout];
			var selectedButtonCollectionPageLayout = buttonsCollectionsPageLayout.FirstOrDefault(t => t.Tag as string == currentCollectionPageLayout);
			if (selectedButtonCollectionPageLayout != null)
			{
				selectedButtonCollectionPageLayout.IsChecked = true;
			}

			var buttonsImageStretch = this.RadioButtonsImageStretch.Children.OfType<RadioButton>();
			string currentImageStretch = (string)ApplicationData.Current.LocalSettings.Values[App.SettingsImageStretch];
			var selectedButtonImageStretch = buttonsImageStretch.FirstOrDefault(t => t.Tag as string == currentImageStretch);
			if (selectedButtonImageStretch != null)
			{
				selectedButtonImageStretch.IsChecked = true;
			}

			this.Loaded -= SettingsPersonalizationPage_Loaded;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
		}

		private void AutoPlayVideoCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo] = true;
		}

		private void AutoPlayVideoCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo] = false;
		}

		private void RadioButtonMuteVideo_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton item = sender as RadioButton;
			ApplicationData.Current.LocalSettings.Values[App.SettingsMuteVideo] = item.Tag.ToString();
		}

		private void RadioButtonMode_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton item = sender as RadioButton;
			ThemeManager.Instance.ChangeMode(item.Tag.ToString());
		}

		private void RadioButtonMainPageLayout_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton item = sender as RadioButton;
			App.SettingsMainPageLayoutType tag;

			if (Enum.TryParse(item.Tag.ToString(), out tag))
			{
				ApplicationData.Current.LocalSettings.Values[App.SettingsMainPageLayout] = item.Tag.ToString();
			}
		}

		private void RadioButtonCollectionPageLayout_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton item = sender as RadioButton;
			App.SettingsCollectionPageLayoutType tag;

			if (Enum.TryParse(item.Tag.ToString(), out tag))
			{
				ApplicationData.Current.LocalSettings.Values[App.SettingsCollectionPageLayout] = item.Tag.ToString();
			}
		}

		private void RadioButtonImageStretch_Checked(object sender, RoutedEventArgs e)
		{
			RadioButton item = sender as RadioButton;
			Stretch tag;

			if (Enum.TryParse(item.Tag.ToString(), out tag))
			{
				ApplicationData.Current.LocalSettings.Values[App.SettingsImageStretch] = item.Tag.ToString();
			}
		}

		private void PlayFullScreenVideoCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsPlayFullScreenVideo] = true;
		}

		private void PlayFullScreenVideoCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			ApplicationData.Current.LocalSettings.Values[App.SettingsPlayFullScreenVideo] = false;
		}
	}
}
