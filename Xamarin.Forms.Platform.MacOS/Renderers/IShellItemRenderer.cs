using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellItemRenderer : IDisposable
	{
		ShellItem ShellItem { get; set; }

		NSViewController ViewController { get; }
	}
}
