using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellFlyoutContentRenderer
	{
		NSViewController ViewController { get; }

		event EventHandler WillAppear;
		event EventHandler WillDisappear;
	}
}
