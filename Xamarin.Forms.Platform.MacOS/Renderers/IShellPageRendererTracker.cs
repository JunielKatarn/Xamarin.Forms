using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellPageRendererTracker : IDisposable
	{
		bool IsRootPage { get; set; }

		NSViewController ViewController { get; set; }

		Page Page { get; set; }
	}
}
