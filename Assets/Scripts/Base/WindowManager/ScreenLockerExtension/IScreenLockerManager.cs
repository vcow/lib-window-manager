using System;

namespace Base.WindowManager.ScreenLockerExtension
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
		/// <param name="type">Тип снимаемой блокировки, если null, снимаются все блокировки.</param>
		void Unlock(Action<LockerType> completeCallback, LockerType? type = null);

		/// <summary>
		/// Задать блокировщик экрана.
		/// </summary>
		/// <param name="type">Тип задаваемого блокировщика.</param>
		/// <param name="screenLockerPrefab">Префаб блокировщика.</param>
		void SetScreenLocker(LockerType type, ScreenLocker screenLockerPrefab);
	}
}