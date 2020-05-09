using Base.ScreenLocker;

namespace Sample
{
	public class WaitScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.BusyWait;
	}
}