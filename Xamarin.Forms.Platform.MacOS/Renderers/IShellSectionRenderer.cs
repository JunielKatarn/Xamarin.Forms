using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellSectionRenderer : IDisposable
	{
		bool IsInMoreTab { get; set; }
		ShellSection ShellSection { get; set; }
		NSViewController ViewController { get; }
	}
}
