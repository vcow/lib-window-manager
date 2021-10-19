using Base.Activatable;

namespace Base.WindowManager.ScreenLockerExtension
{
	public interface IScreenLocker : IActivatable
	{
		LockerType LockerType { get; }
	}
}