using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		NSViewController ViewController { get; }

		NSView View { get; }

		void AttachFlyout(IShellContext context, NSViewController content);
	}
}
