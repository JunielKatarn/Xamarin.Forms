using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.MacOS
{
	public interface IShellItemTransition
	{
		Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer);
	}
}
