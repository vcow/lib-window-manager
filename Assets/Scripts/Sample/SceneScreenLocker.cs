using Base.ScreenLocker;

namespace Sample
{
	public class SceneScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.SceneLoader;
	}
}