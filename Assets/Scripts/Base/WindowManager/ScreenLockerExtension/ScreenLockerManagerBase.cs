#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Base.Activatable;
using Base.ScreenLocker;
using Helpers.TouchHelper;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Base.WindowManager.ScreenLockerExtension
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public abstract class ScreenLockerManagerBase : MonoBehaviour, IScreenLockerManager
	{
		private Dictionary<LockerType, ScreenLocker> _screenPrefabs;
		protected abstract bool ManagerDontDestroyOnLoad { get; }

		private IScreenLocker _currentScreenLocker;
		private Action _completeCallback;

		private int _lockId;

#pragma warning disable 649
		[SerializeField] private ScreenLocker[] _lockers = new ScreenLocker[0];
#pragma warning restore 649

		protected virtual void Awake()
		{
			_screenPrefabs = _lockers.ToDictionary(record => record.LockerType, record => record);

			if (ManagerDontDestroyOnLoad)
			{
				DontDestroyOnLoad(gameObject);
			}

			InitManager(GetComponent<Canvas>(), GetComponent<CanvasScaler>(), GetComponent<GraphicRaycaster>());
		}

		protected abstract void InitManager(Canvas canvas, CanvasScaler canvasScaler,
			GraphicRaycaster graphicRaycaster);

		protected abstract void InitLocker(IScreenLocker locker);

		protected virtual void OnDestroy()
		{
			ReleaseHandlers(false);
		}

		private void OnActivateLockerHandler(object sender, EventArgs args)
		{
			var activatableStateChangedArgs = (ActivatableStateChangedEventArgs) args;
			if (activatableStateChangedArgs.CurrentState != ActivatableState.Active) return;
			ReleaseHandlers(true);
		}

		private void OnDeactivateLockerHandler(object sender, EventArgs args)
		{
			var activatableStateChangedEventArgs = (ActivatableStateChangedEventArgs) args;
			if (activatableStateChangedEventArgs.CurrentState != ActivatableState.Inactive) return;
			var mb = _currentScreenLocker as MonoBehaviour;
			if (mb) Destroy(mb.gameObject);
			IsLocked = false;
			ReleaseHandlers(true);
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
			ReleaseHandlers(true);

			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			if (!_screenPrefabs.TryGetValue(type, out var prefab))
			{
				Debug.LogWarningFormat("There is no screen prefab for the {0} lock type.",
					typeof(LockerType).GetEnumName(type));
				IsLocked = false;
				completeCallback?.Invoke();
				return;
			}

			var locker = Instantiate(prefab, transform).GetComponent<IScreenLocker>();
			if (locker == null)
			{
				throw new Exception("Screen locker must implements IScreenLocker.");
			}

			InitLocker(locker);

			if (locker.IsActive())
			{
				Debug.LogWarningFormat("Screen locker for the {0} lock type is active in the initial time.",
					typeof(LockerType).GetEnumName(type));
				IsLocked = true;
				completeCallback?.Invoke();
				return;
			}

			_completeCallback = completeCallback;
			locker.ActivatableStateChangedEvent += OnActivateLockerHandler;

			locker.Activate(IsLocked);
			IsLocked = true;
		}

		public void Unlock(Action completeCallback)
		{
			ReleaseHandlers(true);

			IScreenLocker screenLocker = null;
			foreach (Transform child in transform)
			{
				var l = child.GetComponent<IScreenLocker>();
				if (l == null)
				{
					Destroy(child.gameObject);
				}
				else
				{
					if (screenLocker != null)
					{
						var mb = screenLocker as MonoBehaviour;
						if (mb) Destroy(mb.gameObject);
					}

					screenLocker = l;
				}
			}

			if (screenLocker == null || screenLocker.IsInactive())
			{
				IsLocked = false;
				completeCallback?.Invoke();
				return;
			}

			Assert.IsNull(_currentScreenLocker);
			_completeCallback = completeCallback;
			_currentScreenLocker = screenLocker;
			_currentScreenLocker.ActivatableStateChangedEvent += OnDeactivateLockerHandler;

			_currentScreenLocker.Deactivate();
		}

		// 	\IScreenLockerManager

		private void ReleaseHandlers(bool invokeCallback)
		{
			if (_currentScreenLocker != null)
			{
				_currentScreenLocker.ActivatableStateChangedEvent -= OnActivateLockerHandler;
				_currentScreenLocker.ActivatableStateChangedEvent -= OnDeactivateLockerHandler;
				_currentScreenLocker = null;
			}

			if (invokeCallback && _completeCallback != null)
			{
				// Предотвращение многократного рекурсивного вызова completeCallback.
				var completeCallback = _completeCallback;
				_completeCallback = null;

				completeCallback.Invoke();
			}
			else
			{
				_completeCallback = null;
			}
		}

#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Screen Locker Manager")]
		private static void FindAndSelectWindowManager()
		{
			var instance = Resources.FindObjectsOfTypeAll<ScreenLockerManagerBase>().FirstOrDefault();
			if (!instance)
			{
				LoadAllPrefabs();
				instance = Resources.FindObjectsOfTypeAll<ScreenLockerManagerBase>().FirstOrDefault();
			}

			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of ScreenLockerManager.");
		}

		private static void LoadAllPrefabs()
		{
			Directory.GetDirectories(Application.dataPath, @"Resources", SearchOption.AllDirectories)
				.Select(s => Directory.GetFiles(s, @"*.prefab", SearchOption.TopDirectoryOnly))
				.SelectMany(strings => strings.Select(Path.GetFileNameWithoutExtension))
				.Distinct().ToList().ForEach(s => Resources.LoadAll(s));
		}
#endif
	}
}