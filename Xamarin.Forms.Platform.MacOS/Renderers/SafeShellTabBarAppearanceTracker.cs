using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SafeShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
	{
		//NSColor _defaultBarTint;
		NSColor _defaultTint;
		//NSColor _defaultUnselectedTint;

		#region IShellTabBarAppearanceTracker

		void IDisposable.Dispose()
		{
		}

		void IShellTabBarAppearanceTracker.ResetAppearance(NSTabViewController controller)
		{
			if (_defaultTint == null)
				return;
		}

		void IShellTabBarAppearanceTracker.SetAppearance(NSTabViewController controller, ShellAppearance appearance)
		{
			//IShellAppearanceElement appearanceElement = appearance;
			//var backgroundColor = appearanceElement.EffectiveTabBarBackgroundColor;
			//var foregroundColor = appearanceElement.EffectiveTabBarForegroundColor;
			//var disabledColor = appearanceElement.EffectiveTabBarDisabledColor;
			//var unselectedColor = appearanceElement.EffectiveTabBarUnselectedColor;
			//var titleColor = appearanceElement.EffectiveTabBarTitleColor;

			//var tabBar = controller.TabView;

			if (_defaultTint == null)
			{
				// Does TabView have colors?
			}
		}

		void IShellTabBarAppearanceTracker.UpdateLayout(NSTabViewController controller)
		{
		}

		#endregion IShellTabBarAppearanceTracker
	}
}
