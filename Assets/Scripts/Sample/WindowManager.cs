using Base.WindowManager;
using Zenject;

namespace Sample
{
	public sealed class WindowManager : WindowManagerBase
	{
#pragma warning disable 649
		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		protected override int StartCanvasSortingOrder => 1000;

		protected override void InitWindow(IWindow window, object[] args)
		{
			base.InitWindow(window, args);
			_container.Inject(window);
		}
	}
}