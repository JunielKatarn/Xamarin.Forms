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

		IShellFlyoutRenderer FlyoutRenderer => throw new NotImplementedException();

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public VisualElement Element { get; private set; }
		public NSView NativeView => FlyoutRenderer.View;
		public Shell Shell => Element as Shell;
		public NSViewController ViewController => FlyoutRenderer.ViewController;

		protected virtual IShellFlyoutRenderer CreateFlyoutRenderer() => throw new NotImplementedException();

		protected virtual IShellNavBarAppearanceTracker CreateShellNavBarAppearanceTracker => throw new NotImplementedException();

		protected virtual IShellPageRendererTracker CreateShellPageRendererTracker() => throw new NotImplementedException();

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer => throw new NotImplementedException();

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item) => throw new NotImplementedException();

		protected virtual IShellItemTransition CreateShellItemTransition() => throw new NotImplementedException();

		protected virtual IShellSearchResultsRenderer CreateShellSearchResultsRenderer() => throw new NotImplementedException();

		protected virtual IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection) => throw new NotImplementedException();

		protected virtual IShellTabBarAppearanceTracker CreateShellTabBarAppearanceTracker() => throw new NotImplementedException();

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
	}
}
