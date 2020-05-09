using Base.ScreenLocker;

namespace Sample
{
	public class GameScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.GameLoader;
	}
}