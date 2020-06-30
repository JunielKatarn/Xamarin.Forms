using System;
using AppKit;

/// <summary>
/// This one requires a UINavigationController equivalent.
/// Stub for now
/// </summary>
namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellNavBarAppearanceTracker : IDisposable
	{
		void ResetAppearance(object controller);
		void SetAppearance(object controller, ShellAppearance appearance);
		void UpdateLayout(object controller);
		void SetHasShadow(object controller, bool hasShadow);
	}
}
