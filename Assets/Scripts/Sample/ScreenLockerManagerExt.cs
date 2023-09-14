using Base.WindowManager.Extensions.ScreenLockerExtension;

namespace Sample
{
	public class ScreenLockerManagerExt : ScreenLockerManagerBase
	{
		public ScreenLockerManagerExt(ScreenLockerSettings screenLockerSettings)
			: base(screenLockerSettings.ScreenLockers)
		{
		}
	}
}