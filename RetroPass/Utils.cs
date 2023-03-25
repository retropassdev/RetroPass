using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace RetroPass
{
	public class Utils
	{
		public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
		{
			if (parent == null) return null;
			T foundChild = null;
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				T childType = child as T;

				if (childType == null)
				{
					foundChild = FindChild<T>(child, childName);
					if (foundChild != null) break;
				}
				else if (!string.IsNullOrEmpty(childName))
				{
					var frameworkElement = child as FrameworkElement;
					if (frameworkElement != null && frameworkElement.Name == childName)
					{
						foundChild = (T)child;
						break;
					}

					foundChild = FindChild<T>(child, childName);
					if (foundChild != null) break;
				}
				else
				{
					foundChild = (T)child;
					break;
				}
			}

			return foundChild;
		}
	}
}
