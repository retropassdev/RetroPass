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
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			AutoPlayVideoCheckBox.IsChecked = (bool)ApplicationData.Current.LocalSettings.Values[App.SettingsAutoPlayVideo];
		
			//set first selected
			var buttonsMode = this.RadioButtonsMode.Children.OfType<RadioButton>();
			var selectedButtonMode = buttonsMode.FirstOrDefault(t => t.Tag as string == ThemeManager.Instance.CurrentMode.ToString());
			if(selectedButtonMode != null)
			{
				selectedButtonMode.IsChecked = true;
			}
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
    }
}
