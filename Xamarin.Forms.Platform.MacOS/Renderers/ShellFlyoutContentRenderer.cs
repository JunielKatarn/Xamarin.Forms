using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellFlyoutContentRenderer : NSViewController, IShellFlyoutContentRenderer
	{
		NSVisualEffectView _blurView;
		NSImageView _bgImage;
		readonly IShellContext _shellContext;
		NSContainerView _headerView;//TODO? UIContainerView
		//ShellTableViewController _tableViewController;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_shellContext = context;

			var header = (context.Shell as IShellController).FlyoutHeader;
			if (header != null)
				_headerView = new NSContainerView((context.Shell as IShellController).FlyoutHeader);

			//_tableviewC...

			//AddChildViewController(_tableVie...)

			context.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		//protected virtual ShellTableViewController CreateShellTableViewController()

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(
				Shell.FlyoutBackgroundColorProperty,
				Shell.FlyoutBackgroundImageProperty,
				Shell.FlyoutBackgroundImageAspectProperty))
				UpdateBackground();
			else
				UpdateFlowDirection();
		}

		void UpdateFlowDirection()
		{
			//_tableV
			_headerView.UpdateFlowDirection(_shellContext.Shell);
		}

		protected virtual void UpdateBackground()
		{
			var color = _shellContext.Shell.FlyoutBackgroundColor;
			//View.BackgroundColor...

			//if (View.BackgroundColor...)

			UpdateFlyoutBgImageAsync();
		}

		async void UpdateFlyoutBgImageAsync()
		{
			// image
			var imageSource = _shellContext.Shell.FlyoutBackgroundImage;
			if (imageSource == null || !_shellContext.Shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				_bgImage.RemoveFromSuperview();
				_bgImage.Image?.Dispose();
				_bgImage.Image = null;
				return;
			}

			using (var nativeImage = await imageSource.GetNativeImageAsync())
			{
				if (View == null)
					return;

				if (nativeImage == null)
				{
					_bgImage?.RemoveFromSuperview();
					return;
				}

				_bgImage.Image = nativeImage;
				switch (_shellContext.Shell.FlyoutBackgroundImageAspect)
				{
					default:
					case Aspect.AspectFit:
						//_bgImage.ContentMode = ...
						break;
					case Aspect.AspectFill:
						break;
					case Aspect.Fill:
						break;
				}

				if (_bgImage.Superview != View)
				{ }//InsertSubview
			}
		}

		void OnElementSelected(Element element)
		{
			(_shellContext.Shell as IShellController).OnFlyoutItemSelected(element);
		}

		#region IShellFlyoutContentRenderer

		NSViewController IShellFlyoutContentRenderer.ViewController => this;

		public event EventHandler /*IShellFlyoutContentRenderer.*/WillAppear;

		public event EventHandler /*IShellFlyoutContentRenderer.*/WillDisappear;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//View.AddSubview(//_tableViewC)
			if (_headerView != null)
				View.AddSubview(_headerView);

			//_tableviewC...

			//var effect...
			_blurView = new NSVisualEffectView();
			_blurView.Frame = View.Bounds;
			_bgImage = new NSImageView
			{
				Frame = View.Bounds,
				//ContentMode
				//ClipsToBounds = true
			};

			UpdateBackground();
			UpdateFlowDirection();
		}

		public override void ViewWillAppear()
		{
			UpdateFlowDirection();
			base.ViewWillAppear();
			WillAppear?.Invoke(this, EventArgs.Empty);
		}

		public override void ViewWillDisappear()
		{
			base.ViewWillDisappear();

			WillDisappear?.Invoke(this, EventArgs.Empty);
		}

		#endregion IShellFlyoutContentRenderer
	}
}
