using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Media;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace RetroPass
{
	public sealed partial class FocusControl : UserControl
	{
		FrameworkElement parentElement;
		UIElement parentPanelItem;
		Panel parentPanel;

		public bool Enabled
		{
			get { return (bool)GetValue(EnabledProperty); }
			set { SetValue(EnabledProperty, value); }
		}

		public bool IsPopup
		{
			get { return (bool)GetValue(IsPopupProperty); }
			set { SetValue(IsPopupProperty, value); }
		}

		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.Register(
				"Enabled",
				typeof(bool),
				typeof(FocusControl),
				new PropertyMetadata(false, OnEnabledPropertyChanged));

		public static readonly DependencyProperty IsPopupProperty =
			DependencyProperty.Register(
				"IsPopup",
				typeof(bool),
				typeof(FocusControl),
				new PropertyMetadata(true, OnIsPopupPropertyChanged));


		public FocusControl()
		{
			this.InitializeComponent();
			this.Loaded += FocusControl_Loaded;
		}

		private void FocusControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (Resources.TryGetValue("RevealShadow", out var resource) && resource is AttachedCardShadow shadow)
			{
				RetroPassBorder.CornerRadius = this.CornerRadius;
				shadow.CornerRadius = this.CornerRadius.TopLeft;
			}

			if (IsPopup)
			{
				RetroPassGrid.Children.Clear();
				RetroPassPopup.Child = RetroPassFocusRoot;
				
			}
			else
			{
				RetroPassPopup.Child = null;
				RetroPassGrid.Children.Add(RetroPassFocusRoot);
				parentElement = this.FindAscendant<ButtonBase>();
				parentPanel = parentElement.FindAscendant<Panel>();
				parentPanelItem = this.FindAscendant<SelectorItem>() as UIElement;

				if (parentElement != null)
				{
					parentElement.LosingFocus += ParentElement_LosingFocus;
					parentElement.GettingFocus += ParentElement_GettingFocus;
				}
			}

			this.Loaded -= FocusControl_Loaded;
		}

		private void ParentElement_LosingFocus(UIElement sender, Windows.UI.Xaml.Input.LosingFocusEventArgs args)
		{		
			if (parentPanelItem != null)
			{
				Canvas.SetZIndex(parentPanelItem, 0);
			}
			//Enabled = false;
		}

		private void ParentElement_GettingFocus(UIElement sender, Windows.UI.Xaml.Input.GettingFocusEventArgs args)
		{
			if (parentPanelItem != null)
			{
				Canvas.SetZIndex(parentPanelItem, 1);
			}
			//Enabled = true;
		}

		private static async void OnEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (FocusControl)d;
			var isEnabled = (bool)e.NewValue;

			if (isEnabled)
			{
				if (control.IsPopup)
				{
					control.RetroPassPopup.Visibility = Visibility.Visible;
					control.RetroPassPopup.IsOpen = true;
				}
				else
				{
					control.RetroPassGrid.Visibility = Visibility.Visible;
				}
			}
			else
			{
				if (control.IsPopup)
				{
					control.RetroPassPopup.Visibility = Visibility.Collapsed;
					control.RetroPassPopup.IsOpen = false;
				}
				else
				{
					control.RetroPassGrid.Visibility = Visibility.Collapsed;
				}
			}

			await Refresh(isEnabled, control);
		}

		private static void OnIsPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			
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
				if(control.IsPopup)
				{
					control.RetroPassPopup.Visibility = Visibility.Visible;
				}
				else
				{
					control.RetroPassGrid.Visibility = Visibility.Visible;
				}

				await Task.Delay(1);

				await control.FocusAnimation2.StartAsync();
			}
			else
			{
				control.FocusAnimation2.Stop();
				if (control.IsPopup)
				{
					control.RetroPassPopup.Visibility = Visibility.Collapsed;
					control.RetroPassFocus.Visibility = Visibility.Collapsed;
					control.RetroPassPopup.IsOpen = false;
				}
				else
				{
					control.RetroPassGrid.Visibility = Visibility.Collapsed;
					control.RetroPassFocus.Visibility = Visibility.Collapsed;
				}
			}
		}
	}
}
