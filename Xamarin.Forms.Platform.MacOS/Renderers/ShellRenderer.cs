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

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public VisualElement Element { get; private set; }
		public NSView NativeView => FlyoutRenderer.View;
		public Shell Shell => Element as Shell;
		public NSViewController ViewController => FlyoutRenderer.ViewController;

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

		protected virtual void OnCurrentItemChanged() => throw new NotImplementedException();

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) => throw new NotImplementedException();

		protected virtual void OnElementSet(Shell element) => throw new NotImplementedException();

		protected async void SetCurrentShellItemController(IShellItemRenderer value) => await System.Threading.Tasks.Task.Factory.StartNew(() => { });

		protected virtual void UpdateBackgroundColor() => throw new NotImplementedException();

		void UpdateFlowDirection(bool readdViews = false) => throw new NotImplementedException();

		void SetupCurrentShellItem() => throw new NotImplementedException();

		#region IShellContext

		bool IShellContext.AllowFlyoutGesture => throw new NotImplementedException();

		Shell IShellContext.Shell => throw new NotImplementedException();

		IShellItemRenderer IShellContext.CurrentShellItemRenderer => null;

		IShellNavBarAppearanceTracker IShellContext.CreateNavBarAppearanceTracker() => throw new NotImplementedException();

		IShellPageRendererTracker IShellContext.CreatePageRendererTracker() => throw new NotImplementedException();

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer() => throw new NotImplementedException();

		IShellSearchResultsRenderer IShellContext.CreateShellSearchResultsRenderer() => throw new NotImplementedException();

		IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection) => throw new NotImplementedException();

		IShellTabBarAppearanceTracker IShellContext.CreateTabBarAppearanceTracker() => throw new NotImplementedException();

		#endregion IShellContext

		#region IVisualElementRenderer

		VisualElement IVisualElementRenderer.Element => throw new NotImplementedException();

		NSView IVisualElementRenderer.NativeView => throw new NotImplementedException();

		NSViewController IVisualElementRenderer.ViewController => throw new NotImplementedException();

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			throw new NotImplementedException();
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			throw new NotImplementedException();
		}

		void IVisualElementRenderer.SetElementSize(Size size)
		{
			throw new NotImplementedException();
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
