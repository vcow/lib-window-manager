using Base.WindowManager.ScreenLockerExtension;

namespace Sample
{
	public class ScreenLockerManagerExt : ScreenLockerManager
	{
		public ScreenLockerManagerExt(ScreenLockerSettings screenLockerSettings)
			: base(screenLockerSettings.ScreenLockers)
		{
		}
	}
}