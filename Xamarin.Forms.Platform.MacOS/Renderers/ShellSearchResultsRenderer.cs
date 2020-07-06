using Foundation;
using System;
using System.Collections.Specialized;
using AppKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellSearchResultsRenderer : NSTableView, IShellSearchResultsRenderer
	{
		readonly IShellContext _context;
		DataTemplate _defaultTemplate;

		// Some joke about templates and hoofed animals
		DataTemplate DefaultTemplate
		{
			get
			{
				return _defaultTemplate ?? (_defaultTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, SearchHandler.DisplayMemberName ?? ".");
					label.HorizontalTextAlignment = TextAlignment.Center;
					label.VerticalTextAlignment = TextAlignment.Center;

					return label;
				}));
			}
		}

		protected NSTableViewAnimation DeleteRowsAnimation { get; set; } = NSTableViewAnimation.None;
		protected NSTableViewAnimation InsertRowsAnimation { get; set; } = NSTableViewAnimation.None;
		protected NSTableViewAnimation ReloadRowsAnimation { get; set; } = NSTableViewAnimation.None;
		protected NSTableViewAnimation ReloadSectionsAnimation { get; set; } = NSTableViewAnimation.None;
		private ISearchHandlerController SearchController => SearchHandler;
		private SearchHandler SearchHandler { get; set; }

		public event EventHandler<object> ItemSelected;

		NSIndexPath[] GetPaths(int section, int index, int count)
		{
			var paths = new NSIndexPath[count];
			for(var i = 0; i < paths.Length; i++)
				paths[i] = NSIndexPath.FromItemSection(index + index, section);

			return paths;
		}

		void OnListProxyChanged(object sender, ListProxyChangedEventArgs e)
		{
			if (e.OldList != null)
			{
				(e.OldList as INotifyCollectionChanged).CollectionChanged -= OnProxyCollectionChanged;
			}
			// Full reset
			base.ReloadData();

			if (e.NewList != null)
			{
				(e.NewList as INotifyCollectionChanged).CollectionChanged += OnProxyCollectionChanged;
			}
		}

		void OnProxyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//int section = 0;
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:

					if (e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					base.BeginUpdates();
					//base.InsertRows(GetPaths(section, e.NewStartingIndex, e.NewItems.Count), InsertRowsAnimation);
					base.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					base.BeginUpdates();
					//base.RemoveRows(GetPaths)
					base.EndUpdates();
					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					base.BeginUpdates();
					for(var i = 0; i < e.OldItems.Count; i++)
					{
						var oldIndex = e.OldStartingIndex;
						var newIndex = e.NewStartingIndex;
						{
							oldIndex += i;
							newIndex += i;
						}

						base.MoveRow(oldIndex, newIndex);
					}
					base.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					base.BeginUpdates();
					//base.ReloadData(GetPaths)
					base.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Reset:
					base.ReloadData();
					return;
					
				default:
					break;
			}
		}

		void OnSearchHandlerSet()
		{
			SearchController.ListProxyChanged += OnListProxyChanged;
		}

		public ShellSearchResultsRenderer(IShellContext context)
		{
			_context = context;
		}

		#region NSTableView

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (SearchHandler != null)
				{
					var listProxy = SearchController.ListProxy as INotifyCollectionChanged;
					if (listProxy != null)
						listProxy.CollectionChanged -= OnProxyCollectionChanged;
					SearchController.ListProxyChanged -= OnListProxyChanged;
				}

				SearchHandler = null;
			}
		}

		//public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)

		//public override void RowSelected(UITableView tableView, NSIndexPath indexPath)

		//public override nint NumberOfSections(UITableView tableView)

		//public override nint RowsInSection(UITableView tableView, nint section)

		#endregion NSTableView

		#region IShellSearchResultsRenderer

		NSViewController IShellSearchResultsRenderer.ViewController => throw new NotImplementedException();

		SearchHandler IShellSearchResultsRenderer.searchHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		event EventHandler<object> IShellSearchResultsRenderer.ItemSelected
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

		#endregion IShellSearchResultsRenderer
	}
}
