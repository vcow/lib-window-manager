using System;
using System.Collections.Generic;
using System.Linq;
using Base.Activatable;
using Helpers.TouchHelper;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Base.WindowManager.Extensions.ScreenLockerExtension
{
	/// <summary>
	/// A base class for the Screen Locker Manager.
	/// Derived Manager must be a singleton that will receive a list of the ScreenLocker prefabs in his constructor.
	/// </summary>
	public abstract class ScreenLockerManagerBase : IScreenLockerManager, IDisposable
	{
		private readonly Dictionary<LockerType, ScreenLockerBase> _screenLockerPrefabs;

		private readonly Dictionary<LockerType, ScreenLockerBase> _activeLockers =
			new Dictionary<LockerType, ScreenLockerBase>();

		private readonly Dictionary<ScreenLockerBase, Action> _lockCompleteCallbacks =
			new Dictionary<ScreenLockerBase, Action>();

		private readonly Dictionary<ScreenLockerBase, Action<LockerType>> _unlockCompleteCallbacks =
			new Dictionary<ScreenLockerBase, Action<LockerType>>();

		private int _lockId;

		protected ScreenLockerManagerBase(IEnumerable<ScreenLockerBase> screenLockers)
		{
			_screenLockerPrefabs = screenLockers != null
				? screenLockers.GroupBy(record => record.LockerType)
					.Select(lockers =>
					{
						var locker = lockers.First();
						if (Debug.isDebugBuild)
						{
							var numLockers = lockers.Count();
							if (numLockers > 1)
							{
								Debug.LogErrorFormat("There are {0} lockers, specified for the {1} type.",
									numLockers, locker.LockerType);
							}
						}

						return locker;
					})
					.ToDictionary(locker => locker.LockerType)
				: new Dictionary<LockerType, ScreenLockerBase>();
		}

		/// <summary>
		/// This method calls straight after the screen locker is created.Override it if you need some additional
		///	initializations for the screen locker.
		/// </summary>
		/// <param name="locker">Created screen locker.</param>
		protected virtual void InitLocker(IScreenLocker locker)
		{
		}

		public virtual void Dispose()
		{
			foreach (var activeLocker in _activeLockers.Values)
			{
				if (!activeLocker) continue;

				activeLocker.Force();

				activeLocker.ActivatableStateChangedEvent -= OnLockerStateChanged;
				Object.Destroy(activeLocker.gameObject);
			}

			_activeLockers.Clear();
			_lockCompleteCallbacks.Clear();
			_unlockCompleteCallbacks.Clear();
		}

		private void OnLockerStateChanged(IActivatable activatable, ActivatableState state)
		{
			var locker = (ScreenLockerBase)activatable;

			switch (state)
			{
				case ActivatableState.Active: // locked
					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;

					if (_lockCompleteCallbacks.TryGetValue(locker, out var lockCallback))
					{
						_lockCompleteCallbacks.Remove(locker);
						lockCallback.Invoke();
					}

					break;
				case ActivatableState.Inactive: // unlocked
					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;

					if (_activeLockers.TryGetValue(locker.LockerType, out var activeLocker) &&
					    activeLocker == locker)
					{
						_activeLockers.Remove(locker.LockerType);
					}

					if (_unlockCompleteCallbacks.TryGetValue(locker, out var unlockCallback))
					{
						_unlockCompleteCallbacks.Remove(locker);
						unlockCallback.Invoke(locker.LockerType);
					}

					Object.Destroy(locker.gameObject);

					IsLocked = _activeLockers.Count > 0;
					break;
			}
		}

		public void SetScreenLocker(LockerType type, ScreenLockerBase screenLockerPrefab)
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

				oldLocker.ActivatableStateChangedEvent -= OnLockerStateChanged;
				Object.Destroy(oldLocker.gameObject);

				if (_activeLockers.Remove(type))
				{
					// This locker should have be removed from the active lockers in the OnLockerStateChanged handler,
					// if not then he isn't send the ActivatableStateChangedEvent during the Force() call.
					Debug.LogWarningFormat("The locker of type {0} hasn't change his activatable state during the " +
					                       "Force() action.", oldLocker.LockerType);
				}

				if (_lockCompleteCallbacks.TryGetValue(oldLocker, out var oldLockCallback))
				{
					_lockCompleteCallbacks.Remove(oldLocker);
					oldLockCallback.Invoke();
				}

				if (_unlockCompleteCallbacks.TryGetValue(oldLocker, out var oldUnlockCallback))
				{
					_unlockCompleteCallbacks.Remove(oldLocker);
					oldUnlockCallback.Invoke(oldLocker.LockerType);
				}
			}

			if (!_screenLockerPrefabs.TryGetValue(type, out var prefab))
			{
				Debug.LogErrorFormat("There is no screen prefab for the {0} lock type.", type);
				IsLocked = _activeLockers.Count > 0;
				completeCallback?.Invoke();
				return;
			}

			var locker = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<ScreenLockerBase>();
			if (!locker)
			{
				throw new Exception("Screen locker must implements IScreenLocker.");
			}

			InitLocker(locker);

			_activeLockers.Add(type, locker);

			IsLocked = true;

			if (locker.IsInactive())
			{
				if (completeCallback != null)
				{
					_lockCompleteCallbacks.Add(locker, completeCallback);
				}

				locker.ActivatableStateChangedEvent += OnLockerStateChanged;
				locker.Activate();
			}
			else if (locker.IsActive())
			{
				completeCallback?.Invoke();
			}
			else
			{
				Debug.LogErrorFormat("The locker {0} is in wrong initial state {1}.",
					locker.LockerType, locker.ActivatableState);
				completeCallback?.Invoke();
			}
		}

		public void Unlock(Action<LockerType> completeCallback, LockerType? type = null)
		{
			var unlocked = new List<ScreenLockerBase>();
			if (type.HasValue)
			{
				if (_activeLockers.TryGetValue(type.Value, out var locker))
				{
					unlocked.Add(locker);
				}
			}
			else
			{
				unlocked.AddRange(_activeLockers.Values);
			}

			if (unlocked.Count <= 0)
			{
				completeCallback?.Invoke(LockerType.Undefined);
				return;
			}

			foreach (var locker in unlocked)
			{
				if (!locker.IsActive())
				{
					locker.Force();

					if (_lockCompleteCallbacks.TryGetValue(locker, out var lockCallback))
					{
						// This callback should be called and removed from the lock complete callbacks in the
						// OnLockerStateChanged handler, if not then locker isn't send ActivatableStateChanged event
						// during the Force() call.
						Debug.LogWarningFormat("The locker of type {0} hasn't change his activatable state during " +
						                       "the Force() action.", locker.LockerType);
						_lockCompleteCallbacks.Remove(locker);
						lockCallback.Invoke();
					}
				}

				if (locker.IsActive())
				{
					if (completeCallback != null)
					{
						_unlockCompleteCallbacks.Add(locker, completeCallback);
					}

					locker.ActivatableStateChangedEvent += OnLockerStateChanged;
					locker.Deactivate();
				}
				else
				{
					Debug.LogErrorFormat("The locker {0} that is been unlocked wasn't switched to the Active state.",
						locker.LockerType);

					completeCallback?.Invoke(locker.LockerType);

					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;
					Object.Destroy(locker.gameObject);

					_activeLockers.Remove(locker.LockerType);
					_lockCompleteCallbacks.Remove(locker);
					_unlockCompleteCallbacks.Remove(locker);
				}
			}

			IsLocked = _activeLockers.Count > 0;
		}

		// 	\IScreenLockerManager
	}
}