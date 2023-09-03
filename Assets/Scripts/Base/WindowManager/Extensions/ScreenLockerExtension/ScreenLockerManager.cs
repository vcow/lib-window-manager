using System;
using System.Collections.Generic;
using System.Linq;
using Base.Activatable;
using Helpers.TouchHelper;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Base.WindowManager.ScreenLockerExtension
{
	public class ScreenLockerManager : IScreenLockerManager, IDisposable
	{
		private readonly Dictionary<LockerType, ScreenLocker> _screenLockerPrefabs;

		private readonly Dictionary<LockerType, ScreenLocker> _activeLockers =
			new Dictionary<LockerType, ScreenLocker>();

		private readonly Dictionary<ScreenLocker, IDisposable> _lockerCompleteHandlers =
			new Dictionary<ScreenLocker, IDisposable>();

		private int _lockId;

		protected ScreenLockerManager(IEnumerable<ScreenLocker> screenLockers)
		{
			_screenLockerPrefabs = screenLockers != null
				? screenLockers.ToDictionary(record => record.LockerType, record => record)
				: new Dictionary<LockerType, ScreenLocker>();
		}

		protected virtual void InitLocker(IScreenLocker locker)
		{
		}

		public void Dispose()
		{
			foreach (var activeLocker in _activeLockers.Values)
			{
				if (!activeLocker) continue;

				activeLocker.Force();
				Object.Destroy(activeLocker.gameObject);
			}

			foreach (var disposable in _lockerCompleteHandlers.Values) disposable.Dispose();
			_lockerCompleteHandlers.Clear();

			_activeLockers.Clear();
		}

		public void SetScreenLocker(LockerType type, ScreenLocker screenLockerPrefab)
		{
			_screenLockerPrefabs[type] = screenLockerPrefab;
		}

		// 	IScreenLockerManager

		public bool IsLocked
		{
			get => _lockId > 0;
			private set
			{
				if (value == IsLocked) return;

				if (value)
				{
					Assert.IsTrue(_lockId == 0);
					_lockId = TouchHelper.Lock();
				}
				else
				{
					Assert.IsTrue(_lockId > 0);
					TouchHelper.Unlock(_lockId);
					_lockId = 0;
				}
			}
		}

		public void Lock(LockerType type, Action completeCallback)
		{
			if (_activeLockers.TryGetValue(type, out var oldLocker))
			{
				oldLocker.Force();
				Object.Destroy(oldLocker.gameObject);
				_activeLockers.Remove(type);

				if (_lockerCompleteHandlers.TryGetValue(oldLocker, out var disposable))
				{
					disposable.Dispose();
					_lockerCompleteHandlers.Remove(oldLocker);
				}
			}

			if (!_screenLockerPrefabs.TryGetValue(type, out var prefab))
			{
				Debug.LogWarningFormat("There is no screen prefab for the {0} lock type.",
					typeof(LockerType).GetEnumName(type));
				IsLocked = false;
				completeCallback?.Invoke();
				return;
			}

			var locker = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<ScreenLocker>();
			if (locker == null)
			{
				throw new Exception("Screen locker must implements IScreenLocker.");
			}

			InitLocker(locker);

			_activeLockers.Add(type, locker);
			IsLocked = true;

			if (locker.IsInactive())
			{
				locker.Activate();
			}

			if (locker.IsActive())
			{
				completeCallback?.Invoke();
				return;
			}

			if (completeCallback != null)
			{
				var handler = locker.ActivatableStateChangesStream
					.Subscribe(new ObserverImpl<ActivatableState>(state =>
					{
						if (state != ActivatableState.Active) return;

						if (_lockerCompleteHandlers.TryGetValue(locker, out var disposable))
						{
							disposable.Dispose();
							_lockerCompleteHandlers.Remove(locker);
						}

						completeCallback.Invoke();
					}));
				_lockerCompleteHandlers.Add(locker, handler);
			}
		}

		public void Unlock(Action<LockerType> completeCallback, LockerType? type = null)
		{
			var unlocked = new List<ScreenLocker>();
			if (type.HasValue)
			{
				if (_activeLockers.TryGetValue(type.Value, out var locker))
				{
					locker.Force();
					unlocked.Add(locker);

					if (_lockerCompleteHandlers.TryGetValue(locker, out var disposable))
					{
						disposable.Dispose();
						_lockerCompleteHandlers.Remove(locker);
					}
				}
			}
			else
			{
				foreach (var screenLocker in _activeLockers.Values)
				{
					screenLocker.Force();
					unlocked.Add(screenLocker);
				}

				foreach (var disposable in _lockerCompleteHandlers.Values) disposable.Dispose();
				_lockerCompleteHandlers.Clear();
			}

			if (unlocked.Count <= 0)
			{
				return;
			}

			foreach (var locker in unlocked)
			{
				if (locker.IsActive())
				{
					locker.Deactivate();
				}

				if (locker.IsInactive())
				{
					_activeLockers.Remove(locker.LockerType);
					IsLocked = _activeLockers.Count > 0;
					completeCallback?.Invoke(locker.LockerType);
					Object.Destroy(locker.gameObject);
					continue;
				}

				var lockerType = locker.LockerType;
				var callback = completeCallback;
				var handler = locker.ActivatableStateChangesStream
					.Subscribe(new ObserverImpl<ActivatableState>(state =>
					{
						if (state != ActivatableState.Inactive) return;

						if (_lockerCompleteHandlers.TryGetValue(locker, out var disposable))
						{
							disposable.Dispose();
							_lockerCompleteHandlers.Remove(locker);
						}

						_activeLockers.Remove(lockerType);
						IsLocked = _activeLockers.Count > 0;
						callback?.Invoke(lockerType);

						Object.Destroy(locker.gameObject);
					}));
				_lockerCompleteHandlers.Add(locker, handler);
			}
		}

		// 	\IScreenLockerManager
	}
}