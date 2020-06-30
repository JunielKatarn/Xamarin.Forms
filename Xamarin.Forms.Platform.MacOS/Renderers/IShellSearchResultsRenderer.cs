using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellSearchResultsRenderer : IDisposable
	{
		NSViewController ViewController { get; }

		SearchHandler searchHandler { get; set; }

		event EventHandler<object> ItemSelected;
	}
}
