using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellSectionRootHeader : IDisposable
	{
		NSViewController ViewController { get; }
		ShellSection ShellSection { get; set; }
	}
}
