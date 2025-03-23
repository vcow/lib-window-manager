using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Plugins.vcow.WindowManager
{
	/// <summary>
	/// WindowManager implementation.
	/// </summary>
	public class WindowManager : IWindowManager, IDisposable
	{
		public delegate void InstantiateWindowHook(IWindow window);

		private readonly struct DelayedWindow : IComparable<DelayedWindow>
		{
			private readonly long _timestamp;

			public DelayedWindow(IWindow window, bool isUnique, bool overlap, string windowGroup)
			{
				Window = window;
				IsUnique = isUnique;
				Overlap = overlap;
				WindowGroup = windowGroup;
				_timestamp = DateTime.Now.Ticks;
			}

			public IWindow Window { get; }
			public bool IsUnique { get; }
			public bool Overlap { get; }
			public string WindowGroup { get; }

			public int CompareTo(DelayedWindow other)
			{
				if (other.IsUnique && !IsUnique) return 1;
				if (!other.IsUnique && IsUnique) return -1;
				if (_timestamp > other._timestamp) return 1;
				if (_timestamp < other._timestamp) return -1;
				return 0;
			}
		}

		private static int _windowCtr;
		private bool _isDisposed;

		private readonly Dictionary<string, Window> _windowsMap;

		private readonly List<(IWindow window, int index, string group)> _openedWindows = new();

		private readonly SortedSet<DelayedWindow> _delayedWindows = new();
		private readonly Dictionary<string, bool> _isUniqueMap = new();

		public event WindowOpenedHandler WindowOpenedEvent;
		public event WindowClosedHandler WindowClosedEvent;

		private readonly HashSet<string> _knownGroups = new();
		private readonly int _startCanvasSortingOrder;
		private readonly SortedList<int, string> _groupHierarchy;
		private InstantiateWindowHook _instantiateWindowHook;

		private const int UnknownGroupSortingOrder = 0;
		private readonly HashSet<string> _unknownGroupHierarchy;

		public WindowManager(WindowManagerSettings settings, InstantiateWindowHook instantiateWindowHook = null)
		{
			_startCanvasSortingOrder = settings.StartCanvasSortingOrder;
			_instantiateWindowHook = instantiateWindowHook;

			_windowsMap = settings.WindowLibraries?.SelectMany(p => p.Windows)
				.GroupBy(window => window.WindowId)
				.Select(windows =>
				{
					var window = windows.First();
#if UNITY_EDITOR
					var numWindows = windows.Count();
					if (numWindows > 1)
					{
						Debug.LogErrorFormat("There are {0} registered windows for the {1} Window identifier.",
							numWindows, window.WindowId);
					}
#endif
					return window;
				})
				.ToDictionary(window => window.WindowId) ?? new Dictionary<string, Window>();

			_groupHierarchy = new SortedList<int, string>(settings.GroupHierarchy?
				.Distinct()
				.Select((s, i) => (index: i, group: s))
				.ToDictionary(tuple => tuple.index, tuple => tuple.group) ?? new Dictionary<int, string>());

			_unknownGroupHierarchy = new HashSet<string>();
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			foreach (var window in _openedWindows.Select(tuple => tuple.window))
			{
				window.CloseWindowEvent -= OnCloseWindow;
				window.DestroyWindowEvent -= OnDestroyWindow;
				window.StateChangedEvent -= OnDeactivateWindow;
				window.Dispose();
			}

			_openedWindows.Clear();
			_delayedWindows.Clear();
			_isUniqueMap.Clear();

			WindowOpenedEvent = null;
			WindowClosedEvent = null;

			_instantiateWindowHook = null;
		}

		public int CloseAll(params object[] args)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call CloseAll() method in the disposed WindowManager.");
				return 0;
			}

			var windows = args.Where(o => o is IWindow).Cast<IWindow>().ToArray();
			var others = args.Where(o => !(o is IWindow)).ToArray();
			if (others.Length > 0)
			{
				windows = windows.Concat(GetWindows(others)).ToArray();
			}

			foreach (var window in windows.Distinct())
			{
				if (_delayedWindows.Any(delayedWindow => delayedWindow.Window == window) ||
				    !window.Close(true))
				{
					OnCloseWindow(window, null);
				}
			}

			return windows.Length;
		}

		public IWindow GetWindow(object arg)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call GetWindow() method in the disposed WindowManager.");
				return null;
			}

			return GetWindows(arg).FirstOrDefault();
		}

		public IReadOnlyList<IWindow> GetWindows(params object[] args)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call GetWindows() method in the disposed WindowManager.");
				return Array.Empty<IWindow>();
			}

			var allWindows = new[]
				{
					_openedWindows.Select(tuple => tuple.window),
					_delayedWindows.Select(delayed => delayed.Window)
				}
				.SelectMany(enumerable => enumerable).ToArray();
			if (args.Length <= 0)
			{
				return allWindows;
			}

			var windows = new HashSet<IWindow>();
			foreach (var arg in args)
			{
				switch (arg)
				{
					case string stringArg:
						windows.UnionWith(allWindows.Where(window => window.WindowId == stringArg));
						break;
					case Type typeArg:
						if (!typeof(IWindow).IsAssignableFrom(typeArg))
							throw new NotSupportedException("IWindow types supported only.");
						windows.UnionWith(allWindows.Where(window => window.GetType() == typeArg));
						break;
					default:
						throw new NotSupportedException("GetWindows() received unsupported argument. " +
						                                "Strings WindowId and IWindow types supported only.");
				}
			}

			return windows.ToArray();
		}

		public IReadOnlyList<IWindow> GetCurrentUnique(string groupId = null)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call GetCurrentUnique() method in the disposed WindowManager.");
				return Array.Empty<IWindow>();
			}

			var groups = (groupId == null ? _knownGroups.ToArray() : new[] { groupId })
				.Where(GetIsUniqueForGroup).ToArray();
			return groups.Length > 0
				? _openedWindows.Where(tuple => groups.Contains(tuple.group)).Select(tuple => tuple.window).ToArray()
				: Array.Empty<IWindow>();
		}

		public IWindow ShowWindow(string windowId, object[] args = null, bool? isUnique = null,
			bool? overlap = null, string windowGroup = null)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call ShowWindow() method in the disposed WindowManager.");
				return null;
			}

			if (!_windowsMap.TryGetValue(windowId, out var prefab))
			{
				Debug.LogErrorFormat("Window with Id {0} isn't registered in Manager.", windowId);
				return null;
			}

			var window = InstantiateWindow(windowId, prefab);
			_instantiateWindowHook?.Invoke(window);
			window.SetArgs(args ?? Array.Empty<object>());

			var isUniqueFlag = isUnique ?? window.IsUnique;
			var overlapFlag = overlap ?? window.Overlap;
			var groupId = windowGroup ?? window.WindowGroup ?? string.Empty;

			var openedWindowsFromGroup = _openedWindows
				.Where(tuple => tuple.group == groupId).ToList();
			if (GetIsUniqueForGroup(groupId) || isUniqueFlag && openedWindowsFromGroup.Count > 0)
			{
				window.Canvas.gameObject.SetActive(false);
				_delayedWindows.Add(new DelayedWindow(window, isUniqueFlag, overlapFlag, groupId));
			}
			else
			{
				DoApplyWindow(window, isUniqueFlag, overlapFlag, groupId);
			}

			_knownGroups.Add(groupId);
			return window;
		}

		protected virtual IWindow InstantiateWindow(string windowId, Window prefab)
		{
			var instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
			instance.name = windowId;

			var window = instance.GetComponent<IWindow>();
			Assert.IsNotNull(window, "Window prefab must implements IWindow.");

			return window;
		}

		protected virtual void DestroyWindow(IWindow window)
		{
			Object.Destroy(window.Canvas.gameObject);
		}

		public bool RegisterWindow(Window windowPrefab, bool overrideExisting = false)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call RegisterWindow() method in the disposed WindowManager.");
				return false;
			}

			var windowId = windowPrefab.WindowId;
			if (!_windowsMap.TryAdd(windowId, windowPrefab))
			{
				if (!overrideExisting) return false;

				CloseAll(windowId);
				_windowsMap[windowId] = windowPrefab;
			}

			return true;
		}

		public bool UnregisterWindow(string windowId)
		{
			if (_isDisposed)
			{
				Debug.LogError("Try to call UnregisterWindow() method in the disposed WindowManager.");
				return false;
			}

			return _windowsMap.Remove(windowId);
		}

		private int GetOrderOffsetForGroup(string groupId)
		{
			if (string.IsNullOrEmpty(groupId))
			{
				return 0;
			}

			if (!_groupHierarchy.Values.Contains(groupId))
			{
				if (_unknownGroupHierarchy.Add(groupId))
				{
					Debug.LogWarningFormat(
						"There is no predefined group {0} in the WindowsTanagerSettings. New one created, sorting order starts from {1}.",
						groupId, UnknownGroupSortingOrder + _startCanvasSortingOrder);
				}

				return UnknownGroupSortingOrder;
			}

			var index = _groupHierarchy.Single(pair => pair.Value == groupId).Key;
			//TODO: Increase that value if more than 100 windows will be open.
			return (index + 1) * 100;
		}

		private bool GetIsUniqueForGroup(string groupId)
		{
			return _isUniqueMap.TryGetValue(groupId, out var isUnique) && isUnique;
		}

		private void SetIsUniqueForGroup(string groupId, bool value)
		{
			_isUniqueMap[groupId] = value;
		}

		private void DoApplyWindow(IWindow window, bool isUnique, bool overlap, string windowGroup)
		{
			SetIsUniqueForGroup(windowGroup, isUnique);

			window.CloseWindowEvent += OnCloseWindow;
			window.DestroyWindowEvent += OnDestroyWindow;

			var openedWindowsFromGroupCount = _openedWindows.Count(tuple => tuple.group == windowGroup);
			window.Canvas.sortingOrder = _startCanvasSortingOrder + openedWindowsFromGroupCount +
			                             GetOrderOffsetForGroup(windowGroup);

			var openedWindowsFromGroup = _openedWindows
				.Where(tuple => tuple.group == windowGroup).ToList();
			var overlappedWindow = openedWindowsFromGroup.OrderBy(tuple => tuple.index).LastOrDefault().window;

			_openedWindows.Add((window, _windowCtr++, windowGroup));

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

			WindowOpenedEvent?.Invoke(this, window);
		}

		private void OnDestroyWindow(IWindow window, object _)
		{
			Debug.LogWarningFormat("The window {0} wasn't closed before destroy.", window.WindowId);
			OnCloseWindow(window, _);
		}

		private void OnCloseWindow(IWindow window, object _)
		{
			int i;
			string groupId;
			var record = _openedWindows.FirstOrDefault(tuple => tuple.window == window);
			if (record != default)
			{
				i = record.index;
				groupId = record.group;

				record.window.CloseWindowEvent -= OnCloseWindow;
				record.window.DestroyWindowEvent -= OnDestroyWindow;

				_openedWindows.Remove(record);
			}
			else
			{
				i = 0;
				groupId = window.WindowGroup ?? string.Empty;
			}

			var resetUnique = true;
			if (window.IsInactiveOrDeactivated())
			{
				DestroyWindow(window);
				var delayedWindow = _delayedWindows.FirstOrDefault(call => call.Window == window);
				if (delayedWindow.Window == window)
				{
					groupId = delayedWindow.WindowGroup;
					_delayedWindows.Remove(delayedWindow);
					resetUnique = false;
				}
			}
			else
			{
				window.StateChangedEvent += OnDeactivateWindow;
			}

			var openedWindowsFromGroup = _openedWindows
				.Where(tuple => tuple.group == groupId).ToList();

			var overlappedWindow = openedWindowsFromGroup
				.Aggregate(((IWindow window, int index, string group))default,
					(res, tuple) =>
					{
						if (tuple.index >= i) return tuple;
						if (res.window != null && res.index > tuple.index) return res;
						return tuple;
					}).window;

			if (overlappedWindow != null && overlappedWindow.IsInactiveOrDeactivated())
			{
				overlappedWindow.Activate();
			}

			if (resetUnique)
			{
				SetIsUniqueForGroup(groupId, false);
			}

			WindowClosedEvent?.Invoke(this, window.WindowId);

			_delayedWindows.ToList().ForEach(call =>
			{
				if (call.WindowGroup != groupId ||
				    GetIsUniqueForGroup(groupId) ||
				    call.IsUnique && openedWindowsFromGroup.Count > 0)
				{
					return;
				}

				_delayedWindows.Remove(call);

				if (call.Window as Object)
				{
					call.Window.Canvas.gameObject.SetActive(true);
					DoApplyWindow(call.Window, call.IsUnique, call.Overlap, call.WindowGroup);
				}
				else
				{
					Debug.LogWarningFormat("The Window {0} was destroyed before it was displayed.", window.WindowId);
				}
			});
		}

		private void OnDeactivateWindow(IWindow window, WindowState state)
		{
			if (state != WindowState.Inactive)
			{
				return;
			}

			window.StateChangedEvent -= OnDeactivateWindow;
			DestroyWindow(window);
		}

#if DEBUG_DESTRUCTION
		~WindowManager()
		{
			Debug.Log("WindowManager was successfully destroyed.");
		}
#endif
	}
}