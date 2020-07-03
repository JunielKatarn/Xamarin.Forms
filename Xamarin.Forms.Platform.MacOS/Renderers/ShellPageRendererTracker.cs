using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker, IFlyoutBehaviorObserver
	{
		IShellContext _context;
		FlyoutBehavior _flyoutBehavior;
		WeakReference<NSViewController> _rendererRef;
		Page _page;
		NSCache _nSCache;

		protected virtual void OnRendererSet()
		{
			//TODO? NavigationItem
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//TODO: Collectioniew extensions
		}

		//public async void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		//{
		//	_flyoutBehavior = behavior;
		//	//TODO
		//}

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
			_nSCache = new NSCache();
			//TODO: //_context.Shell.PropertyChanged ++ 
		}

		#region IShellPageRendererTracker

		bool IShellPageRendererTracker.IsRootPage { get; set; }

		NSViewController IShellPageRendererTracker.ViewController
		{
			get
			{
				_rendererRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				_rendererRef = new WeakReference<NSViewController>(value);
				//TODO? OnRendererSet
			}
		}

		Page IShellPageRendererTracker.Page
		{
			get
			{
				return _page;
			}
			set
			{
				if (_page == value)
					return;

				var oldPage = _page;
				_page = value;

				//TODO? OnPageSet
			}
		}

		void IDisposable.Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion IShellPageRendererTracker

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			throw new NotImplementedException();
		}

		#endregion IFlyoutBehaviorObserver
	}
}
