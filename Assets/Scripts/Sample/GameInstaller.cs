using Base.WindowManager;
using Zenject;

namespace Sample
{
	public class GameInstaller : MonoInstaller<GameInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<IWindowManager>().FromComponentInNewPrefabResource(@"WindowManager").AsSingle();
		}
	}
}