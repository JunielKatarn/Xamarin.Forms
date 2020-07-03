using CoreGraphics;
using AppKit;
using System;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellNavBarAppearanceTracker : IShellNavBarAppearanceTracker
	{
		CGColor _defaultBarTint; // Using CoreGraphics.CGColor instead of UIKit.UIColor
		CGColor _defaultTint;
		NSStringAttributes _defaultTitleAttributes;
		float _shadowopacity = float.MinValue;
		CGColor _shadowColor;

		// Using NSViewController due to the lack of "NSNavigationController"
		public void UpdateLayout(NSViewController controller)
		{
		}

		// Likely unusiable. Can macOS apps have a nav bar?
		public void ResetAppearance(NSViewController controller)
		{
			if (_defaultTint != null)
			{
				//var navBar = controller.TouchBar;
			}
		}

		public void SetAppearance(NSViewController controller, ShellAppearance appearance)
		{
			var background = appearance.BackgroundColor;
			var foreground = appearance.ForegroundColor;
			var titleColor = appearance.TitleColor;

			// No nav bar on macOS?
		}

		#region ShellNavBarAppearanceTracker

		void IDisposable.Dispose()
		{
		}

		void IShellNavBarAppearanceTracker.ResetAppearance(object controller)
		{
			throw new NotImplementedException();
		}

		void IShellNavBarAppearanceTracker.SetAppearance(object controller, ShellAppearance appearance)
		{
			throw new NotImplementedException();
		}

		void IShellNavBarAppearanceTracker.SetHasShadow(object controller, bool hasShadow)
		{
			throw new NotImplementedException();
		}

		void IShellNavBarAppearanceTracker.UpdateLayout(object controller)
		{
			throw new NotImplementedException();
		}

		#endregion ShellNavBarAppearanceTracker
	}
}
