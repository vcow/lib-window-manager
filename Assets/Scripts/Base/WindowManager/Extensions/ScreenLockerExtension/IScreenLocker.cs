using Base.Activatable;

namespace Base.WindowManager.Extensions.ScreenLockerExtension
{
	/// <summary>
	/// Interface for the screen locker window. Screen locker window should cover
	/// the entire screen and lock mouse input.
	/// </summary>
	public interface IScreenLocker : IActivatable
	{
		/// <summary>
		/// The type of the locker screen.
		/// </summary>
		LockerType LockerType { get; }
	}
}