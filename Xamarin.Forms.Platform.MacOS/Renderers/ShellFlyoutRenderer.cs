using CoreAnimation;
using CoreGraphics;
using Foundation;
using MediaPlayer;
using System;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellFlyoutRenderer : NSViewController, IShellFlyoutRenderer, IFlyoutBehaviorObserver, IAppearanceObserver
	{
		const string FlyoutAnimationName = "Flyout";
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		bool _gestureActive;
		bool _isOpen;
		NSViewAnimation _flyoutAnimation; //UIViewPropertyAnimator
		Color _backdropColor;

		public NSAnimationCurve AnimationCurve { get; set; } = NSAnimationCurve.EaseOut;

		public int AnimationDuration { get; set; } = 250;

		double AnimationDurationInSeconds => AnimationDuration / 1000.0;

		public IShellFlyoutTransition FlyoutTransition { get; set; }

		IShellContext Context { get; set; }

		NSViewController Detail { get; set; }

		IShellFlyoutContentRenderer Flyout { get; set; }

		bool IsOpen
		{
			get
			{
				return _isOpen;
			}

			set
			{
				if (_isOpen == value)
					return;

				_isOpen = value;
				Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, value);
			}
		}

		NSPanGestureRecognizer PanGestureRecognizer { get; set; }

		Shell Shell { get; set; }

		IShellController ShellController => Shell;

		NSView TapoffView { get; set; }

		//TODO? UIKeyCommand

		//TODO: Remove?
		public NSView NativeView => throw new NotImplementedException();

		//TODO: Remove?
		public NSViewController ViewController => throw new NotImplementedException();

		//[Foundation.Export("tabForward:")]
		//[Internals.Preserve(Conditional = true)]
		//void TabForward, TabBackward?

		void HandlePanGesture(NSPanGestureRecognizer pan)
		{
			var translation = pan.TranslationInView(View).X;
			double openProgress = 0;
			double openLimit = Flyout.ViewController.View.Frame.Width;

			if (IsOpen)
			{
				openProgress = 1 - (-translation / openLimit);
			}
			else
			{
				openProgress = translation / openLimit;
			}

			openProgress = Math.Min(Math.Max(openProgress, 0.0), 1.0);
			var openPixels = openLimit * openProgress;

			switch (pan.State)
			{
				case NSGestureRecognizerState.Changed:
					_gestureActive = true;

					if (TapoffView == null)
						AddTapoffView();

					if (_flyoutAnimation != null)
					{
						TapoffView.Layer.RemoveAllAnimations();
						_flyoutAnimation?.StopAnimation();
						_flyoutAnimation = null;
					}

					TapoffView.Layer.Opacity = (float)openProgress;

					FlyoutTransition.LayoutViews(View.Bounds, (nfloat)openProgress, Flyout.ViewController.View, Detail.View, _flyoutBehavior);
					break;

				case NSGestureRecognizerState.Ended:
					_gestureActive = false;
					if (IsOpen)
					{
						if (openProgress < .8)
							IsOpen = false;
					}
					else
					{
						if (openProgress > 0.2)
						{
							IsOpen = true;
						}
					}
					LayoutSidebar(true);
					break;
			}
		}

		void LayoutSidebar(bool animate)
		{
			if (_gestureActive)
				return;

			if (animate && _flyoutAnimation != null)
				return;

			if (!animate && _flyoutAnimation != null)
			{
				_flyoutAnimation.StopAnimation();
				_flyoutAnimation = null;
			}

			// start animations...
			if (IsOpen)
				UpdateTapoffView();

			if (animate && TapoffView != null)
			{
				var tapOffViewAnimation = CABasicAnimation.FromKeyPath(@"opacity");
				tapOffViewAnimation.BeginTime = 0;
				tapOffViewAnimation.Duration = AnimationDurationInSeconds;
				tapOffViewAnimation.SetFrom(NSNumber.FromFloat(TapoffView.Layer.Opacity));
				tapOffViewAnimation.SetTo(NSNumber.FromFloat(IsOpen ? 1 : 0));
				tapOffViewAnimation.FillMode = CAFillMode.Forwards;
				//TapoffView.RemoveOnCompletion = false;

				_flyoutAnimation = new NSViewAnimation();
				//TODO: Have it flyouttransition.LayoutViews?

				//_flyoutAnimation.AddObserver()
				//TODO: AddCompletion?

				_flyoutAnimation.StartAnimation();
				View.Layout();
			}
			else if(_flyoutAnimation == null)
			{
				FlyoutTransition.LayoutViews(View.Bounds, IsOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, _flyoutBehavior);
				UpdateTapoffView();

				if (TapoffView != null)
				{
					TapoffView.Layer.Opacity = IsOpen ? 1 : 0;
				}
			}

			void UpdateTapoffView()
			{
				if (IsOpen && _flyoutBehavior == FlyoutBehavior.Flyout)
					AddTapoffView();
				else
					RemoveTapoffView();
			}
		}

		void AddTapoffView()
		{
			if (TapoffView != null)
				return;

			TapoffView = new NSView(View.Bounds);
			TapoffView.Layer.Opacity = 0;
			View.AddSubview(TapoffView, NSWindowOrderingMode.Below, Flyout.ViewController.View);
			//UpdateTapoffViewBackgrouundColor();
			var recognizer = new AppKit.NSClickGestureRecognizer(t =>
			{
				IsOpen = false;
				LayoutSidebar(true);
			});

			TapoffView.AddGestureRecognizer(recognizer);
		}

		void RemoveTapoffView()
		{
			if (TapoffView == null)
				return;

			TapoffView.RemoveFromSuperview();
			TapoffView = null;
		}

		void UpdateFlowDirection(bool readdViews = false)
		{
			View.UpdateFlowDirection(Shell);
			Flyout?.ViewController?.View.UpdateFlowDirection(Shell);
			Detail?.View?.UpdateFlowDirection(Shell);

			if (readdViews)
			{
				if (Detail?.View != null)
					Detail.View.RemoveFromSuperview();

				if (Flyout?.ViewController?.View != null)
					Flyout.ViewController.View.RemoveFromSuperview();

				if (Detail?.View != null)
					View.AddSubview(Detail.View);

				if (Flyout?.ViewController?.View != null)
					View.AddSubview(Flyout.ViewController.View);
			}
		}

		void UpdateTapoffViewBackgroundColor()
		{
			if (TapoffView == null)
				return;

			if (_backdropColor != Color.Default)
			{ }//TODO? Set background color
			else
			{ }
		}

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				var isPresented = Shell.FlyoutIsPresented;
				if (IsOpen != isPresented)
				{
					IsOpen = isPresented;
					LayoutSidebar(true);
				}
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection(true);
			}
		}

		public void FocusSearch(bool forwardDirection)
		{
			var element = Shell.CurrentItem as ITabStopElement;
			var tabIndexes = element?.GetTabIndexesOnParentPage(out _);
			if (tabIndexes == null)
				return;

			int tabIndex = element.TabIndex;
			element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex);
			if (element is ShellItem item)
				Shell.CurrentItem = item;
			else if (element is VisualElement ve)
				ve.Focus();
		}

		#region NSViewController

		public override void ViewDidLayout()
		{
			base.ViewDidLayout();

			if (_flyoutAnimation == null)
				LayoutSidebar(false);
		}

		public override void ViewWillAppear()
		{
			
			base.ViewWillAppear();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			AddChildViewController(Detail);
			View.AddSubview(Detail.View);

			Flyout = Context.CreateShellFlyoutContentRenderer();
			AddChildViewController(Flyout.ViewController);
			View.AddSubview(Flyout.ViewController.View);
			View.AddGestureRecognizer(PanGestureRecognizer);

			(Shell as IShellController).AddFlyoutBehaviorObserver(this);
			UpdateFlowDirection();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
					ShellController.RemoveAppearanceObserver(this);

					_disposed = true;

					//TODO: Shell.PropertyChanged -= OnShellProp...
					Shell.PropertyChanged -= OnShellPropertyChanged;
					(Shell as IShellController).RemoveFlyoutBehaviorObserver(this);

					Context = null;
					Shell = null;
					Detail = null;
				}
			}
		}

		#endregion NSViewController

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			_backdropColor = Color.Default;

			UpdateTapoffViewBackgroundColor();
		}

		#endregion IAppearanceObserver

		#region IShellFlyoutRenderer

		NSViewController IShellFlyoutRenderer.ViewController => this;

		NSView IShellFlyoutRenderer.View => View;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, NSViewController content)
		{
			Context = context;
			Shell = Context.Shell;
			Detail = content;

			Shell.PropertyChanged += OnShellPropertyChanged;

			PanGestureRecognizer = new NSPanGestureRecognizer(HandlePanGesture);
			PanGestureRecognizer.ShouldRecognizeSimultaneously += (a, b) =>
			{
				// This handles tapping outside the open flyout
				if (a is NSPanGestureRecognizer pr && pr.State == NSGestureRecognizerState.Failed &&
					/*b is UITapGestureRecognizer*/ b.State == NSGestureRecognizerState.Ended && IsOpen)
				{
					IsOpen = false;
					LayoutSidebar(true);
				}

				return false;
			};

			PanGestureRecognizer.ShouldReceiveTouch += (sender, touch) =>
			{
				if (!context.AllowFlyoutGesture || _flyoutBehavior != FlyoutBehavior.Flyout)
					return false;
				var view = View;
				CGPoint loc = touch.GetLocation(View);
				if (View is NSSlider ||
					//View is MPVolumeView
					(loc.X > view.Frame.Width * 0.1 && !IsOpen))
					return false;

				return true;
			};

			ShellController.AddAppearanceObserver(this, Shell);
		}

		// macOS does not have swipe view, does it?
		bool IsSwipeView(NSView view)
		{
			return false;
		}

		#endregion IShellFlyoutRenderer

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			if (behavior == FlyoutBehavior.Locked)
				IsOpen = true;
			else if (behavior == FlyoutBehavior.Disabled)
				IsOpen = false;
			LayoutSidebar(false);
		}

		#endregion IFlyoutBehaviorObserver
	}
}
