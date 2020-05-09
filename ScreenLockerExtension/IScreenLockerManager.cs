using System;

namespace Base.ScreenLocker
{
	[Serializable]
	public enum LockerType
	{
		GameLoader,
		SceneLoader,
		BusyWait
	}

	public interface IScreenLockerManager
	{
		/// <summary>
		/// Признак того, что какая-либо блокировка включена.
		/// </summary>
		bool IsLocked { get; }

		/// <summary>
		/// Включить блокировку указанного типа.
		/// </summary>
		/// <param name="type">Тип блокировки.</param>
		/// <param name="completeCallback">Коллбек на завершение активации блокировки.</param>
		void Lock(LockerType type, Action completeCallback);

		/// <summary>
		/// Снять текущую блокировку.
		/// </summary>
		/// <param name="completeCallback">Коллбек на завершение деактивации блокировки.</param>
		void Unlock(Action completeCallback);
	}
}