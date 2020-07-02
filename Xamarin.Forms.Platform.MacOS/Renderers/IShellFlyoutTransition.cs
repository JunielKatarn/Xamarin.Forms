using CoreGraphics;
using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellFlyoutTransition
	{
		void LayoutViews(CGRect bounds, nfloat openPercent, NSView flyout, NSView shell, FlyoutBehavior behavior);
	}
}
