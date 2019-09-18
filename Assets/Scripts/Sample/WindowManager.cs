using Base.WindowManager;

namespace Sample
{
	public class WindowManager : WindowManagerBase
	{
		protected override int StartCanvasSortingOrder => 1000;
	}
}