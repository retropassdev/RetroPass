using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RetroPass
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		public static readonly string SettingsAutoPlayVideo = "SettingsAutoPlayVideo";
		public static readonly string SettingsMuteVideo = "SettingsMuteVideo";
		public static readonly string SettingsPlayFullScreenVideo = "SettingsPlayFullScreenVideo";
		public static readonly string SettingsLoggingEnabled = "SettingsLoggingEnabled";
		public static readonly string SettingsMode = "SettingsMode";
		public static readonly string SettingsMainPageLayout = "SettingsMainPageLayout";
		public static readonly string SettingsCollectionPageLayout = "SettingsCollectionPageLayout";
		public static readonly string SettingsImageStretch = "SettingsImageStretch";

		public enum SettingsMainPageLayoutType
		{
			Fixed,
			OriginalAspect
		}

		public enum SettingsCollectionPageLayoutType
		{
			Fixed,
			OriginalAspect,
			ApproximateAspect
		}

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();

			//bool result = Windows.UI.ViewManagement.ApplicationViewScaling.TrySetDisableLayoutScaling(true);
			this.RequiresPointerMode = Windows.UI.Xaml.ApplicationRequiresPointerMode.WhenRequested;
			this.Suspending += OnSuspending;
			this.UnhandledException += OnUnhandledException;

			var settings = ApplicationData.Current;
			//If getting LocalSettings before getting LocalCacheFoler, the first time the app starts, it gets restarted.
			//Strange behaviour and possibly some kind of a bug, but this prevents it.
			var localCacheFolder = settings.LocalCacheFolder;

			var localSettings = settings.LocalSettings;

			if (localSettings.Values[SettingsAutoPlayVideo] == null)
			{
				localSettings.Values[SettingsAutoPlayVideo] = false;
			}

			if (localSettings.Values[SettingsPlayFullScreenVideo] == null)
			{
				localSettings.Values[SettingsPlayFullScreenVideo] = true;
			}

			if (localSettings.Values[SettingsMuteVideo] == null)
			{
				localSettings.Values[SettingsMuteVideo] = "None";
			}

			if (localSettings.Values[SettingsLoggingEnabled] == null)
			{
				localSettings.Values[SettingsLoggingEnabled] = false;
			}

			if (localSettings.Values[SettingsMode] == null)
			{
				localSettings.Values[SettingsMode] = "Default";
			}

			if (localSettings.Values[SettingsMainPageLayout] == null)
			{
				localSettings.Values[SettingsMainPageLayout] = SettingsMainPageLayoutType.OriginalAspect.ToString();
			}

			if (localSettings.Values[SettingsCollectionPageLayout] == null)
			{
				localSettings.Values[SettingsCollectionPageLayout] = SettingsCollectionPageLayoutType.ApproximateAspect.ToString();
			}

			if (localSettings.Values[SettingsImageStretch] == null)
			{
				localSettings.Values[SettingsImageStretch] = Windows.UI.Xaml.Media.Stretch.Uniform.ToString();
			}
		}

		private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			Trace.TraceError(e.Message);
			Trace.TraceError(e.Exception.StackTrace);
			Trace.Flush();
		}

		private void OnLaunchedOrActivated(ApplicationExecutionState previousExecutionState, bool prelaunchActivated)
		{
			Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

			//if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
			{
				this.FocusVisualKind = FocusVisualKind.Reveal;
			}

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;
				//TODO: set theme color
				//rootFrame.Background

				if (previousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (prelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					//On first load, wait until theme resources are initalized and then go to main page
					if (ThemeManager.Instance.IsInitialized == false)
					{
						ThemeManager.Instance.ThemeInitialization_Finished += ThemeResourcesLoaded;
						ThemeManager.Instance.Init();
					}
					else
					{
						rootFrame.Navigate(typeof(MainPage));
					}
				}
				// Ensure the current window is active
				//ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
				//ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(960, 540);
				Window.Current.Activate();
			}
		}

		private void ThemeResourcesLoaded()
		{
			Frame frame = Window.Current.Content as Frame;
			ThemeManager.Instance.ThemeInitialization_Finished -= ThemeResourcesLoaded;
			frame.Navigate(typeof(MainPage));
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			OnLaunchedOrActivated(e.PreviousExecutionState, e.PrelaunchActivated);
		}

		protected override void OnActivated(IActivatedEventArgs args)
		{
			OnLaunchedOrActivated(args.PreviousExecutionState, false);
			base.OnActivated(args);
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}
