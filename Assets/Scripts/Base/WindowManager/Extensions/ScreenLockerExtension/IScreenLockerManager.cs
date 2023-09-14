using System;

namespace Base.WindowManager.Extensions.ScreenLockerExtension
{
	/// <summary>
	/// The locker types.
	/// </summary>
	[Serializable]
	public enum LockerType
	{
		Undefined,
		GameLoader,
		SceneLoader,
		BusyWait
	}

	/// <summary>
	/// The Screen Locker Manager interface.
	/// </summary>
	public interface IScreenLockerManager
	{
		/// <summary>
		/// The flag indicates that some kind of blocking is enabled.
		/// </summary>
		bool IsLocked { get; }

		/// <summary>
		/// Enable a blocking of the specified type.
		/// </summary>
		/// <param name="type">The type of blocking.</param>
		/// <param name="completeCallback">A callback, which call when blocking is finished.</param>
		void Lock(LockerType type, Action completeCallback);

		/// <summary>
		/// Disable a current blocking.
		/// </summary>
		/// <param name="completeCallback">A callback, which call when unblocking is finished.</param>
		/// <param name="type">A type of the blocking to disable, if null all blocks are disabled.</param>
		void Unlock(Action<LockerType> completeCallback, LockerType? type = null);

		/// <summary>
		/// Set a screen locker for the specified type.
		/// </summary>
		/// <param name="type">The type of locker.</param>
		/// <param name="screenLockerPrefab">A prefab of the screen locker window.</param>
		void SetScreenLocker(LockerType type, ScreenLockerBase screenLockerPrefab);
	}
}