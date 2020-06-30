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
