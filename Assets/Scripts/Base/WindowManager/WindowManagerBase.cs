using System;
using System.Collections.Generic;
using System.Linq;
using Base.Activatable;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Base.WindowManager
{
	[DisallowMultipleComponent]
	public abstract class WindowManagerBase : MonoBehaviour, IWindowManager
	{
		private struct DelayedWindow : IComparable<DelayedWindow>
		{
			private readonly long _timestamp;

			public DelayedWindow(IWindow window, bool isUnique)
			{
				Window = window;
				IsUnique = isUnique;
				_timestamp = DateTime.Now.Ticks;
			}

			public IWindow Window { get; }
			public bool IsUnique { get; }

			public int CompareTo(DelayedWindow other)
			{
				if (other.IsUnique && !IsUnique) return 1;
				if (!other.IsUnique && IsUnique) return -1;
				if (_timestamp > other._timestamp) return 1;
				if (_timestamp < other._timestamp) return -1;
				return 0;
			}
		}

		private Dictionary<string, Window> _windowsMap = new Dictionary<string, Window>();

		private readonly List<IWindow> _openedWindows = new List<IWindow>();
		private readonly SortedSet<DelayedWindow> _delayedWindows = new SortedSet<DelayedWindow>();
		private bool _isUnique;

#pragma warning disable 649
		[SerializeField] private Window[] _windows = new Window[0];
#pragma warning restore 649

		protected abstract int StartCanvasSortingOrder { get; }

		protected virtual void Awake()
		{
			_windowsMap = _windows.ToDictionary(window => window.WindowId, window => window);
		}

		public IWindow ShowWindow(string windowId, object[] args = null, bool isUnique = false, bool overlap = false)
		{
			if (!_windowsMap.TryGetValue(windowId, out var prefab))
			{
				Debug.LogErrorFormat("Window with Id {0} isn't registered in Manager.", windowId);
				return null;
			}

			var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			instance.name = windowId;

			var window = instance.GetComponent<IWindow>();
			Assert.IsNotNull(window, "Window prefab must implements IWindow.");

			InitWindow(window, args ?? new object[0]);

			if (_isUnique || isUnique && _openedWindows.Count > 0)
			{
				window.Canvas.gameObject.SetActive(false);
				_delayedWindows.Add(new DelayedWindow(window, isUnique));
			}
			else
			{
				_isUnique = isUnique;

				window.CloseWindowEvent += OnCloseWindow;
				window.DestroyWindowEvent += OnDestroyWindow;

				var overlappedWindow = _openedWindows.LastOrDefault();
				_openedWindows.Add(window);

				if (window.IsActive())
				{
					Debug.LogError("Window must be inactive in initial time.");
				}
				else if (!window.IsActiveOrActivated())
				{
					window.Activate();
				}

				if (overlap && overlappedWindow != null && overlappedWindow.IsActiveOrActivated())
				{
					overlappedWindow.Deactivate();
				}
			}

			return window;
		}

		protected virtual void InitWindow(IWindow window, object[] args)
		{
			window.SetArgs(args);
		}

		private void OnCloseWindow(IWindowResult result)
		{
			result.Window.CloseWindowEvent -= OnCloseWindow;
			result.Window.DestroyWindowEvent -= OnDestroyWindow;

			var index = _openedWindows.IndexOf(result.Window);
			var overlappedWindow = index > 0 ? _openedWindows.ElementAt(index - 1) : null;
			_openedWindows.Remove(result.Window);

			if (result.Window.IsInactiveOrDeactivated())
			{
				Destroy(result.Window.Canvas.gameObject);
			}
			else
			{
				result.Window.ActivatableStateChangedEvent += OnWindowDeactivateHandler;
			}

			if (overlappedWindow != null && overlappedWindow.IsInactiveOrDeactivated())
			{
				overlappedWindow.Activate();
			}
		}

		private static void OnWindowDeactivateHandler(IActivatable activatable, ActivatableState state)
		{
			if (state != ActivatableState.Inactive) return;
			activatable.ActivatableStateChangedEvent -= OnWindowDeactivateHandler;
			Destroy(((IWindow) activatable).Canvas.gameObject);
		}

		private void OnDestroyWindow(IWindowResult result)
		{
			Debug.LogWarningFormat("Window {0} was destroyed outside of Close() method.", result.Window.WindowId);
			OnCloseWindow(result);
		}

#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Window Manager")]
		private static void FindAndSelectWindowManager()
		{
			var instance = Resources.FindObjectsOfTypeAll<WindowManagerBase>().FirstOrDefault();
			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of WindowManager.");
		}
#endif
	}
}