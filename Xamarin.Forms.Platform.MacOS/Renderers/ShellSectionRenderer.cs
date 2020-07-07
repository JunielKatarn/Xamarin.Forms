using Foundation;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	// See https://stackoverflow.com/questions/18608983/navigation-between-nsviewcontroller
	public class ShellSectionRenderer : NSViewController /*UINavigationControlelr */, IShellSectionRenderer, IAppearanceObserver
	{
		IShellContext _context;

		readonly Dictionary<Element, IShellPageRendererTracker> _trackers =
			new Dictionary<Element, IShellPageRendererTracker>();

		IShellNavBarAppearanceTracker _appearanceTracker;

		Dictionary<NSViewController, TaskCompletionSource<bool>> _completionTasks =
			new Dictionary<NSViewController, TaskCompletionSource<bool>>();

		Page _displayedPage;
		bool _disposed;
		bool _firstLayoutCompleted;
		TaskCompletionSource<bool> _popCompletionTask;
		IShellSectionRootRenderer _renderer;
		ShellSection _shellSection;
		bool _ignorePopCall;

		void Foo()
		{
			_ignorePopCall.GetType();
		}

		public ShellSectionRenderer(IShellContext context)
		{
			//Delegate = new NavDelegate(this);
			_context = context;
			_context.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		[Export("navigationBar:shouldPopItem:")]
		[Internals.Preserve(Conditional = true)]
		public bool ShouldPopItem(NSView navigationBar, NSObject item)
		{
			//// this means teh pop is already done, nothing we can do
			//if (ChildViewControllers.Length < NavigationBar.Items.Length)
			//	return true;

			foreach (var tracker in _trackers)
			{
				// if(tracker.Value.ViewController == TopViewController)
				if (tracker.Value.ViewController == ChildViewControllers.First())
				{
					var behavior = Shell.GetBackButtonBehavior(tracker.Value.Page);
					var command = behavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
					var commandParameter = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null);

					if (command != null)
					{
						if (command.CanExecute(commandParameter))
						{
							command.Execute(commandParameter);
						}

						return false;
					}

					break;
				}
			}

			bool allowPop = ShouldPop();

			if (allowPop)
			{
				// Do not remove, wonky behavior on some versions of iOS if you don't dispatch
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
				{
					_popCompletionTask = new TaskCompletionSource<bool>();
					SendPoppedOnCompletion(_popCompletionTask.Task);
					//PopViewController(true);
				});
			}
			else
			{
				//for (int i = 0; i < NavigationBar.Subviews.Length; i++)
				for (int i=0; i< View.Subviews.Length; i++)
				{
					var child = View.Subviews[i];
					if (child.AlphaValue != 1)
						NSAnimationContext.RunAnimation((context) => child.AlphaValue = 1, () => { });
				}
			}

			return false;
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BaseShellItem.TitleProperty.PropertyName)
				UpdateTabBarItem();
			else if (e.PropertyName == BaseShellItem.IconProperty.PropertyName)
				UpdateTabBarItem();
		}

		protected virtual IShellSectionRootRenderer CreateShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			return new ShellSectionRootRenderer(shellSection, shellContext);
		}

		protected virtual void LoadPages()
		{
			_renderer = CreateShellSectionRootRenderer(ShellSection, _context);
		}

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				UpdateNavigationBarHidden();
			else if (e.PropertyName == Shell.NavBarHasShadowProperty.PropertyName)
				UpdateNavigationBarHasShadow();
		}

		void PushPage(Page page, bool animated, TaskCompletionSource<bool> completionSource = null)
		{
			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.ViewController = renderer.ViewController;
			tracker.Page = page;

			_trackers[page] = tracker;

			if (completionSource != null)
				_completionTasks[renderer.ViewController] = completionSource;

			//PushViewController(renderer.ViewController, animated);
		}

		async void SendPoppedOnCompletion(Task popTask)
		{
			if (popTask == null)
			{
				throw new ArgumentNullException(nameof(popTask));
			}

			var poppedPage = _shellSection.Stack[_shellSection.Stack.Count-1];

			// this is used to setup appearance changes based on the incoming page
			(_shellSection as IShellSectionController).SendPopping(popTask);

			await popTask;

			DisposePage(poppedPage);
		}

		bool ShouldPop()
		{
			var shellItem = _context.Shell.CurrentItem;
			var shellSection = shellItem?.CurrentItem;
			var shellContent = shellSection?.CurrentItem;
			var stack = shellSection?.Stack.ToList();

			stack?.RemoveAt(stack.Count - 1);

			return (_context.Shell as IShellController).ProposeNavigation(ShellNavigationSource.Pop, shellItem, shellSection, shellContent, stack, true);
		}

		void UpdateNavigationBarHidden()
		{
			//SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), true);
		}

		void UpdateNavigationBarHasShadow()
		{
			_appearanceTracker.SetHasShadow(this, Shell.GetNavBarHasShadow(_displayedPage));
		}

		void UpdateShadowImages()
		{
			//NavigationBar.SetValueForKey(NSObject.FromObject(true), new NSString("hidesShadow"));
		}

		void DisposePage(Page page, bool calledFromDispose = false)
		{
			if (_trackers.TryGetValue(page, out var tracker))
			{
				if (!calledFromDispose && tracker.ViewController != null && ChildViewControllers.Contains(tracker.ViewController))
				{
					var list = ChildViewControllers.ToList();
					if (list.Remove(_trackers[page].ViewController))
						ChildViewControllers = list.ToArray();
				}

				tracker.Dispose();
				_trackers.Remove(page);
			}

			var renderer = Platform.GetRenderer(page);
			if (renderer != null)
			{
				renderer.Dispose();
				page.ClearValue(Platform.RendererProperty);
			}
		}

		Element ElementForViewController(NSViewController viewController)
		{
			if (_renderer.ViewController == viewController)
				return ShellSection;

			foreach(var child in ShellSection.Stack)
			{
				if (child == null)
					continue;
				var renderer = Platform.GetRenderer(child);
				if (viewController == renderer.ViewController)
					return child;
			}

			return null;
		}

		protected virtual void UpdateTabBarItem()
		{
			//Title = ShellSection.Title;
			//_ = _context.ApplyNativeImageAsync(ShellSection, ShellSection.IconProperty, icon =>
			//{
			//	TabBarItem = new UITabBarItem(ShellSection.Title, icon, null);
			//	TabBarItem.AccessibilityIdentifier = ShellSection.AutomationId ?? ShellSection.Title;
			//});
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			switch (e.RequestType)
			{
				case NavigationRequestType.Push:
					OnPushRequested(e);
					break;

				case NavigationRequestType.Pop:
					OnPopRequested(e);
					break;

				case NavigationRequestType.PopToRoot:
					OnPopToRootRequested(e);
					break;

				case NavigationRequestType.Insert:
					OnInsertRequested(e);
					break;

				case NavigationRequestType.Remove:
					OnRemoveRequested(e);
					break;
			}
		}

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			var taskSource = new TaskCompletionSource<bool>();
			PushPage(page, animated, taskSource);

			e.Task = taskSource.Task;
		}

		protected virtual async void OnPopRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			_popCompletionTask = new TaskCompletionSource<bool>();
			e.Task = _popCompletionTask.Task;

			//PopViewController(animated);

			await _popCompletionTask.Task;

			DisposePage(page);
		}

		protected virtual async void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			var animated = e.Animated;
			var task = new TaskCompletionSource<bool>();
			var pages = _shellSection.Stack.ToList();

			try
			{
				_ignorePopCall = true;
				_completionTasks[_renderer.ViewController] = task;
				e.Task = task.Task;
				//PopToRootViewController(animated);
			}
			finally
			{
				_ignorePopCall = false;
			}

			await e.Task;

			for (int i = pages.Count - 1; i >= 1; i--)
			{
				var page = pages[i];
				DisposePage(page);
			}
		}

		protected virtual void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var before = e.BeforePage;

			var beforeRenderer = Platform.GetRenderer(before);

			var renderer = Platform.CreateRenderer(page);

			var tracker = _context.CreatePageRendererTracker();
			tracker.ViewController = renderer.ViewController;
			tracker.Page = page;

			_trackers[page] = tracker;

			//ViewControllers.Insert(ViewControllers.IndexOf(beforeRenderer.ViewController), renderer.ViewController);
			AddChildViewController(renderer.ViewController);
		}

		protected virtual void OnRemoveRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;

			var renderer = Platform.GetRenderer(page);
			var viewController = renderer?.ViewController;

			if (viewController == null && _trackers.ContainsKey(page))
				viewController = _trackers[page].ViewController;

			 if (viewController != null)
			{
				if (/*viewController == TopViewController*/ true)
				{
					e.Animated = false;
					OnPopRequested(e);
				}

				var list = ChildViewControllers.ToList();
				if (list.Remove(viewController))
					ChildViewControllers = list.ToArray();
				DisposePage(page);
			}
		}

		protected virtual void OnShellSectionSet()
		{
			_appearanceTracker = _context.CreateNavBarAppearanceTracker();
			UpdateTabBarItem();
			(_context.Shell as IShellController).AddAppearanceObserver(this, ShellSection);
			(ShellSection as IShellSectionController).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
		}

		protected virtual void OnDisplayedPageChanged(Page page)
		{
			if (_displayedPage == page)
				return;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
			}

			_displayedPage = page;

			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;
				UpdateNavigationBarHidden();
				UpdateNavigationBarHasShadow();
			}
		}

		#region NSNavigationController

		internal void UpdateFlowDirection()
		{
			View.UpdateFlowDirection(_context.Shell);
			//NavigationBar.UpdateFlowDirection(_context.Shell);
		}

		public override void ViewWillAppear()
		{
			if (_disposed)
				return;

			UpdateFlowDirection();
			base.ViewWillAppear();
		}

		public override void ViewDidLayout()
		{
			if (_disposed)
				return;

			base.ViewDidLayout();

			_appearanceTracker.UpdateLayout(this);

			if (!_firstLayoutCompleted)
			{
				UpdateShadowImages();
				_firstLayoutCompleted = true;
			}
		}

		public override void ViewDidLoad()
		{
			if (_disposed)
				return;

			base.ViewDidLoad();
			//InteractivePopGestureRecognizer.Delegate = new GestureDelegate(this, ShouldPop);
			UpdateFlowDirection();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				RemoveFromParentViewController();
				_disposed = true;
				_renderer.Dispose();
				_appearanceTracker.Dispose();
				_shellSection.PropertyChanged -= HandlePropertyChanged;
				_context.Shell.PropertyChanged -= HandlePropertyChanged;

				if (_displayedPage != null)
					_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

				(_shellSection as IShellSectionController).NavigationRequested -= OnNavigationRequested;
				(_context.Shell as IShellController).RemoveAppearanceObserver(this);
				(ShellSection as IShellSectionController).RemoveDisplayedPageObserver(this);

				foreach (var tracker in ShellSection.Stack)
				{
					if (tracker == null)
						continue;

					DisposePage(tracker, true);
				}
			}

			_disposed = true;
			_displayedPage = null;
			_shellSection = null;
			_appearanceTracker = null;
			_renderer = null;
			_context = null;

			base.Dispose(disposing);
		}

		#endregion NSNavigationController

		#region IShellSectionRenderer

		bool IShellSectionRenderer.IsInMoreTab { get; set; }

		public ShellSection /*IShellSectionRenderer.*/ShellSection
		{
			get { return _shellSection; }
			set
			{
				if (_shellSection == value)
					return;
				_shellSection = value;
				LoadPages();
				OnShellSectionSet();
				_shellSection.PropertyChanged += HandlePropertyChanged;
				(_shellSection as IShellSectionController).NavigationRequested += OnNavigationRequested;
			}
		}

		NSViewController IShellSectionRenderer.ViewController => this;

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		#endregion IShellSectionRenderer

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				_appearanceTracker.ResetAppearance(this);
			else
				_appearanceTracker.SetAppearance(this, appearance);
		}

		#endregion IAppearanceObserver
	}

	class NavDelegate // : UINavigationControllerDelegate
	{
		readonly ShellSectionRenderer _self;

		public NavDelegate(ShellSectionRenderer renderer)
		{
			_self = renderer;
		}
	}
}
