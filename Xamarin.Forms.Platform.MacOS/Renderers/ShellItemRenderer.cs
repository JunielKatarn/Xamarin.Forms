using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Foundation;
using ObjCRuntime;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellItemRenderer : NSTabViewController, IShellItemRenderer, IAppearanceObserver // IUINavigationControllerDelegate
	{
		readonly IShellContext _context;
		readonly Dictionary<NSViewController, IShellSectionRenderer> _sectionRenderers = new Dictionary<NSViewController, IShellSectionRenderer>();
		IShellTabBarAppearanceTracker _appearanceTracker;
		ShellSection _currentSection;
		Page _displayedPage;
		bool _disposed;
		ShellItem _shellItem;
		bool _switched = true;

		public ShellItemRenderer(IShellContext context)
		{
			_context = context;
		}

		public ShellItem ShellItem
		{
			get => _shellItem;
			set
			{
				if (_shellItem == value)
					return;
				_shellItem = value;
				OnShellItemSet(_shellItem);
				CreateTabRenderers();
			}
		}

		IShellSectionRenderer CurrentRenderer { get; set; }

		public IShellItemController ShellItemController => ShellItem;

		//Export...
		//public virtual void DidShowViewController

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				GoTo(ShellItem.CurrentItem);
			}
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellSection shellSection in e.OldItems)
				{
					var renderer = RendererForShellContent(shellSection);
					if (renderer != null)
					{
						//ViewControllers = ViewControllers.Remove(renderer.viewController);
						//CustomizableViewControllers = Array.Empty<NSViewController>();
						RemoveRenderer(renderer);
					}
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				var items = ShellItemController.GetItems();
				var count = items.Count;
				NSViewController[] viewControllers = new NSViewController[count];

				int maxTabs = 5; // fetch this a better way
				bool willUseMore = count > maxTabs;

				int i = 0;
				bool goTo = false; // it's possible we are in a transitionary state and should not nav
				var current = ShellItem.CurrentItem;
				for(int j = 0; j < items.Count; j++)
				{
					var shellContent = items[j];
					var renderer = RendererForShellContent(shellContent) ?? _context.CreateShellSectionRenderer(shellContent);

					if (willUseMore && j >= maxTabs - 1)
						renderer.IsInMoreTab = true;
					else
						renderer.IsInMoreTab = false;

					renderer.ShellSection = shellContent;

					AddRenderer(renderer);
					viewControllers[i++] = renderer.ViewController;
					if (shellContent == current)
						goTo = true;
				}

				//ViewControllers = viewControllers;
				//CustomizableViewControllers = Array.Empty<NSViewController>();

				if (goTo)
					GoTo(ShellItem.CurrentItem);
			}

			UpdateTabBarHidden();
		}

		protected virtual void OnShellItemSet(ShellItem shellItem)
		{
			_appearanceTracker = _context.CreateTabBarAppearanceTracker();
			shellItem.PropertyChanged += OnElementPropertyChanged;
			(_context.Shell as IShellController).AddAppearanceObserver(this, shellItem);
			ShellItemController.ItemsCollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.IsEnabledProperty.PropertyName)
			{
				var shellSection = sender as ShellSection;
				var renderer = RendererForShellContent(shellSection);
				//var index = ViewControllers.ToList().IndexOf(renderer.ViewController);
				//TabBar.Items[index].Enabled = shellSection.IsEnabled;
			}
		}

		protected virtual void UpdateShellAppearance(ShellAppearance appearance)
		{
			if (appearance == null)
			{
				_appearanceTracker.ResetAppearance(this);
				return;
			}
			_appearanceTracker.SetAppearance(this, appearance);
		}

		void AddRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.ContainsKey(renderer.ViewController))
				return;
			_sectionRenderers[renderer.ViewController] = renderer;
			renderer.ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		void CreateTabRenderers()
		{
			if (ShellItem.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {ShellItem}. Title: {ShellItem.Title}. Route: {ShellItem.Route}.");

			var items = ShellItemController.GetItems();
			var count = items.Count;
			int maxTabs = 5; // fetch this a better way
			bool willUseMore = count > maxTabs;

			NSViewController[] viewControllers = new NSViewController[count];
			int i = 0;
			foreach (var shellContent in items)
			{
				var renderer = _context.CreateShellSectionRenderer(shellContent);

				renderer.IsInMoreTab = willUseMore && i >= maxTabs - 1;

				renderer.ShellSection = shellContent;
				AddRenderer(renderer);
				viewControllers[i++] = renderer.ViewController;
			}

			//ViewControllers = viewControllers;
			//CustomizableViewControllers = Array.Empty<NSViewController>();

			UpdateTabBarHidden();

			// Make sure we are at the right item
			GoTo(ShellItem.CurrentItem);

			// now that they are applied we can se the enabled stat of the TabBar items
			for(i=0; i< viewControllers.Length; i++)
			{
				var renderer = RendererForViewController(viewControllers[i]);
				if (!renderer.ShellSection.IsEnabled)
				{
					//TabBar.ItemsProperty[i].Enabled = false;
				}
			}
		}

		void GoTo(ShellSection shellSection)
		{
			if (shellSection == null || _currentSection == shellSection)
				return;
			var renderer = RendererForShellContent(shellSection);
			//if (renderer?.ViewController != SelectedViewController)
			//	SelectedViewController = renderer.ViewController;
			CurrentRenderer = renderer;

			if (_currentSection != null)
			{
				(_currentSection as IShellSectionController).RemoveDisplayedPageObserver(this);
			}

			_currentSection = shellSection;

			if (_currentSection != null)
			{
				(_currentSection as IShellSectionController).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
				_switched = true;
			}
		}

		void OnDisplayedPageChanged(Page page)
		{
			if (page == _displayedPage)
				return;

			if (_displayedPage != null)
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

			_displayedPage = page;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;

				if (!_currentSection.Stack.Contains(_displayedPage) || _switched)
				{
					_switched = false;
					UpdateTabBarHidden();
				}
			}
		}

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
				UpdateTabBarHidden();
		}

		void RemoveRenderer(IShellSectionRenderer renderer)
		{
			if (_sectionRenderers.Remove(renderer.ViewController))
				renderer.ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;

			renderer?.Dispose();

			if (CurrentRenderer == renderer)
				CurrentRenderer = null;
		}

		IShellSectionRenderer RendererForShellContent(ShellSection shellSection)
		{
			// Not efficient!
			foreach (var item in _sectionRenderers)
			{
				if (item.Value.ShellSection == shellSection)
					return item.Value;
			}
			return null;
		}

		IShellSectionRenderer RendererForViewController(NSViewController viewController)
		{
			// Efficient!
			if (_sectionRenderers.TryGetValue(viewController, out var value))
				return value;
			return null;
		}

		void UpdateTabBarHidden()
		{
			if (ShellItemController == null)
				return;

			//TabBar.Hidden = !ShellItemController.ShowTabs;
		}

		#region NSTabViewController

		// override SelectedViewController

		#endregion NSTabViewController

		#region NSViewController

		public override void ViewDidLayout()
		{
			base.ViewDidLayout();

			_appearanceTracker?.UpdateLayout(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//ShouldSelectTabViewItem
		}

		public override void ViewWillLayout()
		{
			UpdateTabBarHidden();
			base.ViewWillLayout();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				foreach (var kvp in _sectionRenderers.ToList())
				{
					var renderer = kvp.Value;
					RemoveRenderer(renderer);
				}

				if (_displayedPage != null)
					_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

				if (_currentSection != null)
					(_currentSection as IShellSectionController).RemoveDisplayedPageObserver(this);

				_sectionRenderers.Clear();
				ShellItem.PropertyChanged -= OnElementPropertyChanged;
				(_context.Shell as IShellController).ItemsCollectionChanged -= OnItemsCollectionChanged;

				CurrentRenderer = null;
				_shellItem = null;
				_currentSection = null;
				_currentSection = null;
				_displayedPage = null;
			}
		}

		#endregion NSViewController

		#region IShellItemRenderer

		NSViewController IShellItemRenderer.ViewController => this;

		#endregion IShellItemRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			UpdateShellAppearance(appearance);
		}

		#endregion IAppearanceObserver
	}
}
