using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellSectionRootRenderer : NSViewController, IShellSectionRootRenderer
	{
		const int HeaderHeight = 35;
		IShellContext _shellContext;
		NSView _blurView;
		NSView _containerArea;
		ShellContent _currentContent;
		int _currentIndex = 0;
		IShellSectionRootHeader _header;
		IVisualElementRenderer _isanimatingOut;
		Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		IShellPageRendererTracker _tracker;
		bool _didLayoutSubviews;
		int _lastTabThickness = int.MinValue;
		Thickness _lastInset;
		bool _isDisposed;

		ShellSection ShellSection { get; set; }

		IShellSectionController ShellSectionController => ShellSection;

		public ShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
			_shellContext = shellContext;
			_shellContext.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		protected virtual void LayoutRenderers()
		{
			if (_isanimatingOut != null)
				return;

			var items = ShellSectionController.GetItems();
			for(int i=0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					view.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}
		}

		protected virtual void LoadRenderers()
		{
			Dictionary<ShellContent, Page> createdPages = new Dictionary<ShellContent, Page>();
			var contentItems = ShellSectionController.GetItems();

			// pre create all teh pages in case the visibility of a page
			// removes the page from shell
			for(int i = 0; i < contentItems.Count; i++)
			{
				ShellContent item = contentItems[i];
				var page = (item as IShellContentController).GetOrCreateContent();
				createdPages.Add(item, page);
			}

			var currentItem = ShellSection.CurrentItem;
			contentItems = ShellSectionController.GetItems();

			for(int i=0; i<contentItems.Count; i++)
			{
				ShellContent item = contentItems[i];

				if (_renderers.ContainsKey(item))
					continue;

				Page page = null;
				if (!createdPages.TryGetValue(item, out page))
				{
					page = (item as IShellContentController).GetOrCreateContent();
					contentItems = ShellSectionController.GetItems();
				}

				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);
				AddChildViewController(renderer.ViewController);

				if (item == currentItem)
				{
					_containerArea.AddSubview(renderer.NativeView);
					_currentContent = currentItem;
					_currentIndex = i;
				}

				_renderers[item] = renderer;
			}
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_isDisposed)
				return;

			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var newContent = ShellSection.CurrentItem;
				var oldContent = _currentContent;

				if (newContent == null)
					return;

				if (_currentContent == null)
				{
					_currentContent = newContent;
					_currentIndex = ShellSectionController.GetItems().IndexOf(_currentContent);
					_tracker.Page = (newContent as IShellContentController).Page;
					return;
				}

				var items = ShellSectionController.GetItems();
				if (items.Count == 0)
					return;

				var oldIndex = _currentIndex;
				var newIndex = items.IndexOf(newContent);
				var oldRenderer = _renderers[oldContent];

				// this means the currently visible item has been removed
				if (oldIndex == -1 && _currentIndex <= newIndex)
				{
					newIndex++;
				}

				_currentContent = newContent;
				_currentIndex = newIndex;

				if (!_renderers.ContainsKey(newContent))
					return;

				var currentRenderer = _renderers[newContent];

				// -1 == slide left, 1 == slide right
				int motionDirection = newIndex > oldIndex ? -1 : 1;

				_containerArea.AddSubview(currentRenderer.NativeView);
				_isanimatingOut = oldRenderer;

				currentRenderer.NativeView.Frame = new CGRect(-motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);

				if (oldRenderer.NativeView != null)
					oldRenderer.NativeView.Frame = _containerArea.Bounds;

				NSAnimationContext.RunAnimation((context) =>
				{
					currentRenderer.NativeView.Frame = _containerArea.Bounds;

					if (oldRenderer.NativeView != null)
						oldRenderer.NativeView.Frame = new CGRect(motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				},
				() =>
				{
					if (_isDisposed)
						return;

					if (oldRenderer.NativeView != null && _renderers.ContainsKey(oldContent))
						oldRenderer.NativeView.RemoveFromSuperview();

					_isanimatingOut = null;
					_tracker.Page = (newContent as IShellContentController).Page;

					if (!ShellSectionController.GetItems().Contains(oldContent) && _renderers.ContainsKey(oldContent))
					{
						_renderers.Remove(oldContent);

						if (oldRenderer.NativeView != null)
						{
							oldRenderer.ViewController.RemoveFromParentViewController();
							oldRenderer.Dispose();
						}
					}
				});
			}
		}

		protected virtual IShellSectionRootHeader CreateShellSectionRootHeader(IShellContext shellContext)
		{
			return new ShellSectionRootHeader(shellContext);
		}

		protected virtual void UpdateHeaderVisibility()
		{
			bool visible = ShellSectionController.GetItems().Count > 1;

			if (visible)
			{
				if (_header == null)
				{
					_header = CreateShellSectionRootHeader(_shellContext);
					_header.ShellSection = ShellSection;

					AddChildViewController(_header.ViewController);
					View.AddSubview(_header.ViewController.View);
				}
				_blurView.Hidden = false;
				LayoutHeader();
			}
			else
			{
				if (_header != null)
				{
					_header.ViewController.View.RemoveFromSuperview();
					_header.ViewController.RemoveFromParentViewController();
					_header.Dispose();
					_header = null;
				}
				_blurView.Hidden = true;
			}
		}

		void UpdateFlowDirection()
		{
			if (_shellContext?.Shell?.CurrentItem?.CurrentItem == ShellSection)
				this.View.UpdateFlowDirection(_shellContext.Shell);
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_isDisposed)
				return;

			// Make sure we do this after the header has a chance to react
			Device.BeginInvokeOnMainThread(UpdateHeaderVisibility);

			if (e.OldItems != null)
			{
				foreach (ShellContent oldItem in e.OldItems)
				{
					// if current item is removed will be handled by the currentitem property changed event
					// That way render is swapped out cleanly once the new current item is set
					if (_currentContent == oldItem)
						continue;

					var oldRenderer = _renderers[oldItem];

					if (oldRenderer == _isanimatingOut)
						continue;

					if (e.OldStartingIndex < _currentIndex)
						_currentIndex--;

					_renderers.Remove(oldItem);
					oldRenderer.NativeView.RemoveFromSuperview();
					oldRenderer.ViewController.RemoveFromParentViewController();
					oldRenderer.Dispose();
				}
			}

			if (e.NewItems != null)
			{
				foreach(ShellContent newItem in e.NewItems)
				{
					if (_renderers.ContainsKey(newItem))
						continue;

					var page = (newItem as IShellContentController).GetOrCreateContent();
					var renderer = Platform.CreateRenderer(page);
					Platform.SetRenderer(page, renderer);

					AddChildViewController(renderer.ViewController);
					_renderers[newItem] = renderer;
				}
			}
		}

		void LayoutHeader()
		{
			if (ShellSection == null)
				return;

			int tabThickness = 0;
			if (_header != null)
			{
				tabThickness = HeaderHeight;
				var headerTop = 0; // TopLayoutGuide.Length
				CGRect frame = new CGRect(View.Bounds.X, headerTop, View.Bounds.Width, HeaderHeight);
				_blurView.Frame = frame;
				_header.ViewController.View.Frame = frame;
			}

			nfloat left;
			nfloat top;
			nfloat right;
			nfloat bottom;
			left = 0;
			top = 0; // TopLayoutGuide.Length;
			right = 0;
			bottom = View.Frame.Height; // BottomLayoutGuide.Length

			if (_didLayoutSubviews)
			{
				var newInset = new Thickness(left, top, right, bottom);
				if (newInset != _lastTabThickness || tabThickness != _lastTabThickness)
				{
					_lastTabThickness = tabThickness;
					_lastInset = new Thickness(left, top, right, bottom);
					(ShellSection as IShellSectionController).SendInsetChanged(_lastInset, _lastTabThickness);
				}
			}
		}

		#region NSViewController

		public override void ViewDidLayout()
		{
			_didLayoutSubviews = true;
			base.ViewDidLayout();

			_containerArea.Frame = View.Bounds;

			LayoutRenderers();

			LayoutHeader();
		}

		public override void ViewDidLoad()
		{
			if (_isDisposed)
				return;

			if (ShellSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {ShellSection}. Title: {ShellSection.Title}. Route: {ShellSection.Route}.");
			base.ViewDidLoad();

			_containerArea = new NSView();

			View.AddSubview(_containerArea);

			LoadRenderers();

			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
			ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

			//_blurView = new NSView();
			//UIVisualEffect blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
			//_blurView = new UIVisualEffectView(blurEffect);
			_blurView = new NSVisualEffectView();

			View.AddSubview(_blurView);

			UpdateHeaderVisibility();

			var tracker = _shellContext.CreatePageRendererTracker();
			tracker.IsRootPage = true;
			tracker.ViewController = this;

			if (ShellSection.CurrentItem != null)
				tracker.Page = (ShellSection.CurrentItem as IShellContentController).GetOrCreateContent();
			_tracker = tracker;
			UpdateFlowDirection();
		}

		public override void ViewWillAppear()
		{
			if (_isDisposed)
				return;

			UpdateFlowDirection();
			base.ViewWillAppear();
		}

		//ViewSafeAreaInsetsDidChange?

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing && ShellSection != null)
			{
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;

				RemoveFromParentViewController();

				_header?.Dispose();
				_tracker?.Dispose();

				foreach (var renderer in _renderers)
				{
					var oldRenderer = renderer.Value;

					if (oldRenderer.NativeView != null)
						oldRenderer.NativeView.RemoveFromSuperview();

					if (oldRenderer.ViewController != null)
						oldRenderer.ViewController.RemoveFromParentViewController();

					var element = oldRenderer.Element;
					oldRenderer.Dispose();
					element?.ClearValue(Platform.RendererProperty);
				}

				_renderers.Clear();
			}

			if (disposing)
			{
				_shellContext.Shell.PropertyChanged -= HandleShellPropertyChanged;
			}

			_shellContext = null;
			ShellSection = null;
			_header = null;
			_tracker = null;
			_currentContent = null;
			_isDisposed = true;
		}

		#endregion NSViewController

		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => throw new NotImplementedException();

		NSViewController IShellSectionRootRenderer.ViewController => throw new NotImplementedException();

		#endregion IShellSectionRootRenderer
	}
}
