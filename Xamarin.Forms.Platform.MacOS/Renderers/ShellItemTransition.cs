using System.Threading.Tasks;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ShellItemTransition : IShellItemTransition
	{
		#region IShellItemTransition

		Task IShellItemTransition.Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
		{
			TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
			var oldView = oldRenderer.ViewController.View;
			var newView = newRenderer.ViewController.View;
			newView.AlphaValue = 0;

			newView.Superview.AddSubview(newView, NSWindowOrderingMode.Above, oldView);

			NSAnimationContext.RunAnimation((context) => newView.AlphaValue = 1, () => task.TrySetResult(true));

			return task.Task;
		}

		#endregion IShellItemTransition
	}
}
