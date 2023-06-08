using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RetroPass
{
	public sealed partial class FocusControl : UserControl
	{
		public bool Enabled
		{
			get { return (bool)GetValue(EnabledProperty); }
			set { SetValue(EnabledProperty, value); }
		}

		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.Register(
				"Enabled",
				typeof(bool),
				typeof(FocusControl),
				new PropertyMetadata(false, OnEnabledPropertyChanged));

		public FocusControl()
		{
			this.InitializeComponent();
		}

		private static async void OnEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (FocusControl)d;
			var isEnabled = (bool)e.NewValue;

			if (isEnabled)
			{
				control.MyPopup.IsOpen = true;
			}
			else
			{
				control.MyPopup.IsOpen = false;
			}

			await Refresh(isEnabled, control);
		}

		private static async Task Refresh(bool isEnabled, FocusControl control)
		{
			FrameworkElement parent = control.Parent as FrameworkElement;

			if (isEnabled)
			{
				Thickness focusVisualMargin = control.FocusVisualMargin;
				focusVisualMargin.Left -= 1;
				focusVisualMargin.Top -= 1;
				focusVisualMargin.Right -= 1;
				focusVisualMargin.Bottom -= 1;
				double focusVisualMarginWidth = (focusVisualMargin.Left + focusVisualMargin.Right) * -1;
				double focusVisualMarginHeight = (focusVisualMargin.Top + focusVisualMargin.Bottom) * -1;

				control.RetroPassFocus.Width = parent.ActualWidth + focusVisualMarginWidth;
				control.RetroPassFocus.Height = parent.ActualHeight + focusVisualMarginHeight;
				control.RetroPassFocus.Margin = focusVisualMargin;

				control.RetroPassBorder.Width = parent.ActualWidth + focusVisualMarginWidth;
				control.RetroPassBorder.Height = parent.ActualHeight + focusVisualMarginHeight;
				control.RetroPassBorder.Margin = focusVisualMargin;

				control.RetroPassFocus.Visibility = Visibility.Visible;
				control.MyPopup.Visibility = Visibility.Visible;

				await Task.Delay(1);

				await control.FocusAnimation2.StartAsync();
			}
			else
			{
				control.FocusAnimation2.Stop();
				control.MyPopup.Visibility = Visibility.Collapsed;				
				control.RetroPassFocus.Visibility = Visibility.Collapsed;
				control.MyPopup.IsOpen = false;
			}
		}
	}
}
