using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

			if(Enum.TryParse(item.Tag.ToString(),out tag))
			{
				ApplicationData.Current.LocalSettings.Values[App.SettingsCollectionPageLayout] = item.Tag.ToString();
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
