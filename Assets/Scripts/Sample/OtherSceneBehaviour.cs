using Base.ScreenLocker;
using Zenject;

namespace Sample
{
	public class OtherSceneBehaviour : MonoInstaller<OtherSceneBehaviour>
	{
#pragma warning disable 649
		[Inject] private readonly LazyInject<IScreenLockerManager> _screenLockerManager;
#pragma warning restore 649
		
		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			_screenLockerManager.Value.Unlock(null);
		}
	}
}