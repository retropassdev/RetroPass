using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace RetroPass.SettingsPages
{
	public sealed partial class SettingsAddDataSourcePage : ContentDialog
	{
		private DataSourceManager dataSourceManager;
		private string path;
		private (DataSource dataSource, List<DataSourceManager.ValidationResult> validationResult) validation;

		private static string MessageDuplicatePath = "Data source at this location already added.";
		private static string MessageUnknownDataSourceType = "Not a LaunchBox or Emulation Station directory.";
		private static string MessageDuplicateName = "Data source with the same name already exists.";
		private static string MessageUnknown = "Unknown";

		public ContentDialogResult result;

		public SettingsAddDataSourcePage(DataSourceManager dataSourceManager, string path)
		{			
			this.InitializeComponent();
			this.dataSourceManager = dataSourceManager;
			this.path = path;
			_ = Validate(dataSourceManager, path);
			
			ButtonConfirm.IsEnabled = false;
		}

		private void RefreshUI()
		{
			if (validation.dataSource != null)
			{
				TextBoxDataSourceName.Text = validation.dataSource.retroPassConfig.name;
				TextBoxDataSourceType.Text = validation.dataSource.retroPassConfig.type.ToString();
				TextBoxDataSourcePath.Text = validation.dataSource.rootFolder;
			}
			else
			{
				TextBoxDataSourceName.Text = Path.GetFileName(path);
				TextBoxDataSourceType.Text = MessageUnknown;
				TextBoxDataSourcePath.Text = path;
			}

			ButtonConfirm.IsEnabled = validation.validationResult.Count > 0 ? false : true;

			TextBoxDataSourcePathValidation.Text = validation.validationResult.Contains(DataSourceManager.ValidationResult.DUPLICATE_PATH) ? MessageDuplicatePath : "";
			TextBoxDataSourceTypeValidation.Text = validation.validationResult.Contains(DataSourceManager.ValidationResult.UNKNOWN_DATA_SOURCE_TYPE) ? MessageUnknownDataSourceType : "";
			TextBoxDataSourceNameValidation.Text = validation.validationResult.Contains(DataSourceManager.ValidationResult.DUPLICATE_NAME) ? MessageDuplicateName : "";
		}

		private async Task Validate(DataSourceManager dataSourceManager, string path)
		{
			validation = await dataSourceManager.ValidateDataSource(path);
			RefreshUI();
		}

		private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
		{
			var dataSources = dataSourceManager.dataSources;
			
			if(validation.dataSource != null)
			{
				validation.dataSource.retroPassConfig.name = (sender as TextBox).Text;

				if (dataSources.FirstOrDefault(t => t.retroPassConfig.name == validation.dataSource.retroPassConfig.name) != null)
				{
					if (validation.validationResult.Contains(DataSourceManager.ValidationResult.DUPLICATE_NAME) == false)
					{
						validation.validationResult.Add(DataSourceManager.ValidationResult.DUPLICATE_NAME);
					}
				}
				else
				{
					if (validation.validationResult.Contains(DataSourceManager.ValidationResult.DUPLICATE_NAME) == true)
					{
						validation.validationResult.Remove(DataSourceManager.ValidationResult.DUPLICATE_NAME);
					}
				}

				RefreshUI();
			}
		}		

		private async void ButtonConfirm_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			await dataSourceManager.AddDataSource(validation.dataSource);
			result = ContentDialogResult.Primary;
			Hide();
		}
	}
}
