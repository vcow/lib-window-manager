using vcow.UIWindowManager;
using Zenject;

namespace Sample
{
	public class GameInstaller : MonoInstaller<GameInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesTo<WindowManager>().AsSingle()
				.WithArguments((WindowManager.InstantiateWindowHook)InstantiateWindowHook);
		}

		private void InstantiateWindowHook(IWindow window)
		{
			var instance = (Window)window;
			Container.InjectGameObject(instance.gameObject);
		}
	}
}