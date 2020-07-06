using CoreGraphics;
using Foundation;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellSectionRootHeader : NSViewController /*UICollectionViewController*/, IAppearanceObserver, IShellSectionRootHeader
	{
		//UICollectionViewController
		public NSCollectionView CollectionView { get; set; } = new NSCollectionView() { BackgroundColors = new NSColor[1] };

		#region IAppearanceObserver

		Color _defaultBackgroundColor = new Color(0.964);
		Color _defaultForegroundColor = Color.Black;
		Color _defaultUnselectedColor = Color.Black.MultiplyAlpha(0.7);

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		protected virtual void ResetAppearance()
		{
			SetValues(_defaultBackgroundColor, _defaultForegroundColor, _defaultUnselectedColor);
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			SetValues(appearance.BackgroundColor.IsDefault ? _defaultBackgroundColor : appearance.BackgroundColor,
				appearance.ForegroundColor.IsDefault ? _defaultForegroundColor : appearance.ForegroundColor,
				appearance.UnselectedColor.IsDefault ? _defaultUnselectedColor : appearance.UnselectedColor);
		}

		void SetValues(Color backgroundColor, Color foregroundColor, Color unselectedColor)
		{
			CollectionView.BackgroundColors[0] = new Color(backgroundColor.R, backgroundColor.G, backgroundColor.B, 0.863).ToNSColor();

			bool reloadData = _selectedColor != foregroundColor || _unselectedColor != unselectedColor;

			_selectedColor = foregroundColor;
			_unselectedColor = unselectedColor;

			if (reloadData)
				ReloadData();
		}

		// GetCellType
		protected virtual Type GetItemType()
		{
			return typeof(ShellSectionHeaderCell);
		}

		#endregion IAppearanceObserver

		static readonly NSString CellId = new NSString("HeaderCell");

		readonly IShellContext _shellContext;
		NSView _bar;
		NSView _bottomShadow;
		Color _selectedColor;
		Color _unselectedColor;
		bool _isDisposed;

		[Internals.Preserve(Conditional = true)]
		public ShellSectionRootHeader() { }

		[Internals.Preserve(Conditional = true)]
		public ShellSectionRootHeader(IShellContext shellContext)// : base()
		{
			_shellContext = shellContext;
		}

		public double SelectedIndex { get; set; }
		public ShellSection ShellSection { get; set; }
		IShellSectionController ShellSectionController => ShellSection;

		public NSViewController ViewController => this;

		protected void LayoutBar()
		{
			if (SelectedIndex < 0)
				return;

			if (ShellSectionController.GetItems().IndexOf(ShellSection.CurrentItem) != SelectedIndex)
				return;

			var layout = CollectionView.GetLayoutAttributes(NSIndexPath.FromItemSection((int)SelectedIndex, 0));

			if (layout == null)
				return;

			var frame = layout.Frame;

			if (_bar.Frame.Height != 2)
			{
				_bar.Frame = new CGRect(frame.X, frame.Bottom - 2, frame.Width, 2);
			}
			else
			{
				NSAnimationContext.RunAnimation((context) => _bar.Frame = new CGRect(frame.X, frame.Bottom - 2, frame.Width, 2));
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				UpdateSelectedIndex();
			}
		}

		protected virtual void UpdateSelectedIndex(bool animated = false)
		{
			if (ShellSection.CurrentItem == null)
				return;

			SelectedIndex = ShellSectionController.GetItems().IndexOf(ShellSection.CurrentItem);

			if (SelectedIndex < 0)
				return;

			LayoutBar();

			CollectionView.SelectItems(new NSSet(SelectedIndex), NSCollectionViewScrollPosition.CenteredHorizontally);
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ReloadData();
		}

		void ReloadData()
		{
			if (_isDisposed)
				return;

			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();
		}

		#region UICollectionViewController

		public /*override*/ bool CanMoveItem(NSCollectionView collectionView, NSIndexPath indexPath)
		{
			return false;
		}

		//public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		//public /*override*/ /*UICollectionViewCell*/ void GetCell(NSCollectionView collectionView, NSIndexPath indexPath)

		public /*override*/ nint GetItemsCount(NSCollectionView collectionView, nint section)
		{
			return ShellSectionController.GetItems().Count;
		}

		public /*override*/ void ItemDeselected(NSCollectionView collectionView, NSIndexPath indexPath)
		{
			if (CollectionView.GetItem(indexPath) is ShellSectionHeaderCell cell)
			{ } // cell.Label.TextColor ...
		}

		public /*override*/ void ItemSelected(NSCollectionView collectionView, NSIndexPath indexPath)
		{
			var row = 0;//indexPath.Row;

			var item = ShellSectionController.GetItems()[row];

			if (item != ShellSection.CurrentItem)
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, item);

			if (CollectionView.GetItem(indexPath) is ShellSectionHeaderCell cell)
			{ }//cell.Label.TextColor...
		}

		public /*override*/ nint NumberOfSections(NSCollectionView collectionView)
		{
			return 1;
		}

		public /*override*/ bool ShouldSelectItem(NSCollectionView collectionView, NSIndexPath indexPath)
		{
			var row = 0;//indexPath.Row
			var item = ShellSectionController.GetItems()[row];
			IShellController shellController = _shellContext.Shell;

			if (item == ShellSection.CurrentItem)
				return true;

			return shellController.ProposeNavigation(ShellNavigationSource.ShellContentChanged, ShellSection.Parent as ShellItem, ShellSection, item, ShellSection.Stack, true);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				(_shellContext.Shell as IShellController).RemoveAppearanceObserver(this);
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;

				ShellSection = null;
				_bar.RemoveFromSuperview();
				RemoveFromParentViewController();
				_bar.Dispose();
				_bar = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		#endregion UICollectionViewController

		#region NSViewController

		public override void ViewDidLayout()
		{
			if (_isDisposed)
				return;

			base.ViewDidLayout();

			LayoutBar();

			_bottomShadow.Frame = new CGRect(0, CollectionView.Frame.Bottom, CollectionView.Frame.Width, 0.5);
		}

		public override void ViewDidLoad()
		{
			if (_isDisposed)
				return;

			base.ViewDidLoad();

			_bar = new NSView(new CGRect(0, 0, 20, 20));
			_bar.Layer.ZPosition = 9001; // it's over 9000!
			CollectionView.AddSubview(_bar);

			_bottomShadow = new NSView(new CGRect(0, 0, 10, 1));
			_bottomShadow.Layer.ZPosition = 9002;
			CollectionView.AddSubview(_bottomShadow);

			var flowLayout = new NSCollectionViewFlowLayout(); //Layout as NSCollectionViewFlowLayout;
			flowLayout.ScrollDirection = NSCollectionViewScrollDirection.Horizontal;
			flowLayout.MinimumInteritemSpacing = 0;
			flowLayout.MinimumLineSpacing = 0;
			flowLayout.EstimatedItemSize = new CGSize(70, 35);

			CollectionView.RegisterClassForItem(GetItemType(), CellId);

			(_shellContext.Shell as IShellController).AddAppearanceObserver(this, ShellSection);
			ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

			UpdateSelectedIndex();
			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		#endregion NSViewController

		public class ShellSectionHeaderCell : NSCollectionViewItem // UICollectionViewCell
		{
			public NSColor SelectedColor { get; set; }
			public NSColor UnSelectedColor { get; set; }

			[Internals.Preserve(Conditional = true)]
			public ShellSectionHeaderCell() { }

			[Export("initWithFrame:")]
			[Internals.Preserve(Conditional = true)]
			public ShellSectionHeaderCell(CGRect frame) //: base(frame)
			{
				//Label...
				
			}

			#region NSCollectionViewItem

			public override bool Selected
			{
				get => base.Selected;

				set
				{
					base.Selected = value;
					//Label.TextColor = value ? SelectedColor : UnSelectedColor;
				}
			}

			// public UILabel Label { get; }

			// LayoutSubviews, SizeThatFits

			#endregion NSCollectionViewItem
		}
	}
}
