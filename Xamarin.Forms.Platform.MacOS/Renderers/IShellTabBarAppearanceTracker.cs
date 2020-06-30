using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellTabBarAppearanceTracker : IDisposable
	{
		void ResetAppearance(NSTabViewController controller);
		void SetAppearance(NSTabViewController controller, ShellAppearance appearance);
		void UpdateLayout(NSTabViewController controller);
	}
}
