using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellSectionRootRenderer : IDisposable
	{
		bool ShowNavBar { get; }

		NSViewController ViewController { get; }
	}
}
