using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellRenderer : NSViewController, IShellContext, IVisualElementRenderer, IEffectControlProvider
	{
		[Internals.Preserve(Conditional = true)]
		public ShellRenderer()
		{
		}

		IShellItemRenderer _currentShellItemRenderer;
		bool _disposed;
		IShellFlyoutRenderer _flyoutRenderer;

		IShellFlyoutRenderer FlyoutRenderer
		{
			get
			{
				if (_flyoutRenderer == null)
				{
					FlyoutRenderer = CreateFlyoutRenderer();
					FlyoutRenderer.AttachFlyout(this, this);
				}
				return _flyoutRenderer;
			}

			set
			{
				_flyoutRenderer = value;
			}
		}

		//public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		//public VisualElement Element { get; private set; }
		//public NSView NativeView => FlyoutRenderer.View;
		public Shell Shell => Element as Shell;
		//public NSViewController ViewController => FlyoutRenderer.ViewController;

		protected virtual IShellFlyoutRenderer CreateFlyoutRenderer()
		{
			// HACK
			if (NSApplication.SharedApplication?.Delegate?.GetType()?.FullName == "XamarinFormsPreviewer.macOS.AppDelegate")
			{
				return new DesignerFlyoutRenderer(this);
			}

			return new ShellFlyoutRenderer()
			{
				FlyoutTransition = new SlideFlyoutTransition()
			};
		}

		protected virtual IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker()
		{
			return new ShellNavBarAppearanceTracker();//TODO? SafeShell...
		}

		protected virtual IShellPageRendererTracker CreatePageRendererTracker()
		{
			return new ShellPageRendererTracker(this);
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutContentRenderer(this);
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new ShellItemRenderer(this)
			{
				ShellItem = item
			};
		}

		protected virtual IShellItemTransition CreateShellItemTransition()
		{
			return new ShellItemTransition();
		}

		protected virtual IShellSearchResultsRenderer CreateShellSearchResultsRenderer()
		{
			return new ShellSearchResultsRenderer(this);
		}

		protected virtual IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
		{
			return new ShellSectionRenderer(this);
		}

		protected virtual IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
		{
			return new ShellTabBarAppearanceTracker();
		}

		protected virtual void OnCurrentItemChanged()
		{
			var currentItem = Shell.CurrentItem;
			if (_currentShellItemRenderer?.ShellItem != currentItem)
			{
				var newController = CreateShellItemRenderer(currentItem);
				SetCurrentShellItemController(newController);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				OnCurrentItemChanged();
				UpdateFlowDirection();
			}
			else if(e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection(true);
			}
		}

		protected virtual void OnElementSet(Shell element)
		{
			if (element == null)
				return;

			element.PropertyChanged += OnElementPropertyChanged;
		}

		protected async void SetCurrentShellItemController(IShellItemRenderer value)
		{
			var oldRenderer = _currentShellItemRenderer;
			var newRenderer = value;

			_currentShellItemRenderer = value;

			AddChildViewController(newRenderer.ViewController);
			View.AddSubview(newRenderer.ViewController.View);
			//View.SendSubviewToBack(newRenderer.ViewController.View);

			newRenderer.ViewController.View.Frame = View.Bounds;

			if (oldRenderer != null)
			{
				var transition = CreateShellItemTransition();
				await transition.Transition(oldRenderer, newRenderer);

				oldRenderer.ViewController.RemoveFromParentViewController();
				oldRenderer.ViewController.View.RemoveFromSuperview();
				oldRenderer.Dispose();
			}
		}

		protected virtual void UpdateBackgroundColor()
		{
			var color = Shell.BackgroundColor;
			if (color.IsDefault)
				color = NSColor.WindowBackground.ToColor();

			//FlyoutRenderer.View.BackgroundColor
		}

		void UpdateFlowDirection(bool readdViews = false)
		{
			if (_currentShellItemRenderer?.ViewController == null)
				return;

			_currentShellItemRenderer.ViewController.View.UpdateFlowDirection(Element);
			View.UpdateFlowDirection(Element);

			if (readdViews)
			{
				_currentShellItemRenderer.ViewController.View.RemoveFromSuperview();
				View.AddSubview(_currentShellItemRenderer.ViewController.View);
				//View.SendSubviewToBack(_currentShellItemRenderer.ViewController.View);
			}
		}

		void SetupCurrentShellItem()
		{
			if (Shell.CurrentItem == null)
			{
				throw new InvalidOperationException("Shell CurrentItem should not be null");
			}
			else if (_currentShellItemRenderer == null)
			{
				OnCurrentItemChanged();
			}
		}

		#region IShellContext

		bool IShellContext.AllowFlyoutGesture
		{
			get
			{
				ShellSection shellSection = Shell?.CurrentItem?.CurrentItem;
				if (shellSection == null)
					return true;
				return shellSection.Stack.Count <= 1;
			}
		}

		Shell IShellContext.Shell => Element as Shell;

		IShellItemRenderer IShellContext.CurrentShellItemRenderer => _currentShellItemRenderer;

		IShellNavBarAppearanceTracker IShellContext.CreateNavBarAppearanceTracker()
		{
			return CreateNavBarAppearanceTracker();
		}

		IShellPageRendererTracker IShellContext.CreatePageRendererTracker()
		{
			return CreatePageRendererTracker();
		}

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			return CreateShellFlyoutContentRenderer();
		}

		IShellSearchResultsRenderer IShellContext.CreateShellSearchResultsRenderer()
		{
			return CreateShellSearchResultsRenderer();
		}

		IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection)
		{
			return CreateShellSectionRenderer(shellSection);
		}

		IShellTabBarAppearanceTracker IShellContext.CreateTabBarAppearanceTracker()
		{
			return CreateTabBarAppearanceTracker();
		}

		#endregion IShellContext

		#region IVisualElementRenderer

		public VisualElement /*IVisualElementRenderer.*/Element { get; private set; }

		NSView IVisualElementRenderer.NativeView => FlyoutRenderer.View;

		NSViewController IVisualElementRenderer.ViewController => FlyoutRenderer.ViewController;

		public event EventHandler<VisualElementChangedEventArgs> /*IVisualElementRenderer.*/ElementChanged;

		SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint) => new SizeRequest(new Size(100, 100));

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (Element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
			Element = element;
			OnElementSet(Element as Shell);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
		}

		void IVisualElementRenderer.SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		#endregion IVisualElementRenderer

		#region IEffectControlProvider

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			throw new NotImplementedException();
		}

		#endregion IEffectControlProvider

		#region NSViewController

		public override void ViewDidLayout()
		{
			base.ViewDidLayout();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#endregion NSViewController

		// this won't work on the previewer if it's private
		internal class DesignerFlyoutRenderer : IShellFlyoutRenderer
		{
			readonly NSViewController _parent;

			public DesignerFlyoutRenderer(NSViewController parent)
			{
				_parent = parent;
			}

			public NSViewController ViewController => _parent;

			public NSView View => _parent.View;

			public void AttachFlyout(IShellContext context, NSViewController content)
			{
			}

			public void Dispose()
			{

			}
		}
	}
}
