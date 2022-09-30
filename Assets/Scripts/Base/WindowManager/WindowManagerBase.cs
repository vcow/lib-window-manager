#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Base.Activatable;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Base.WindowManager
{
	[DisallowMultipleComponent]
	public abstract class WindowManagerBase : MonoBehaviour, IWindowManager
	{
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

		private Dictionary<string, Window> _windowsMap = new Dictionary<string, Window>();

		private readonly List<(IWindow window, List<IDisposable> closeHandlers, int index, string group)>
			_openedWindows = new List<(IWindow, List<IDisposable>, int, string)>();

		private readonly SortedSet<DelayedWindow> _delayedWindows = new SortedSet<DelayedWindow>();
		private readonly Dictionary<string, bool> _isUniqueMap = new Dictionary<string, bool>();

		private WindowManagerLocalSceneHelper _sceneHelper;

		private readonly ObservableImpl<IWindow> _windowOpenedStream = new ObservableImpl<IWindow>();
		private readonly ObservableImpl<string> _windowClosedStream = new ObservableImpl<string>();

		private readonly HashSet<string> _knownGroups = new HashSet<string>();

#pragma warning disable 649
		[SerializeField] private string[] _groupHierarchy = Array.Empty<string>();
		[SerializeField] private WindowProviderBase[] _windowProviders = Array.Empty<WindowProviderBase>();
#pragma warning restore 649

		protected abstract int StartCanvasSortingOrder { get; }

		protected virtual void Awake()
		{
			_windowsMap = _windowProviders.SelectMany(p => p.Windows)
				.ToDictionary(window => window.WindowId, window => window);
		}

		public IObservable<IWindow> WindowOpenedStream => _windowOpenedStream;

		public IObservable<string> WindowClosedStream => _windowClosedStream;

		public void SetGroupHierarchy(IEnumerable<string> groupHierarchy)
		{
			_groupHierarchy = groupHierarchy != null ? groupHierarchy.ToArray() : Array.Empty<string>();
		}

		private int GetOrderOffsetForGroup(string groupId)
		{
			int index;
			if (string.IsNullOrEmpty(groupId) || (index = Array.IndexOf(_groupHierarchy, groupId)) < 0)
			{
				return 0;
			}

			// Увеличить это значение, если будет открываться одновременно больше 100 окон из одной группы.
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

		public int CloseAll(params object[] args)
		{
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
					OnCloseWindow(window);
				}
			}

			return windows.Length;
		}

		public IWindow GetWindow(object arg)
		{
			return GetWindows(arg).FirstOrDefault();
		}

		public IWindow[] GetWindows(params object[] args)
		{
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

		public IWindow[] GetCurrentUnique(string groupId = null)
		{
			var groups = (groupId == null ? _knownGroups.ToArray() : new[] { groupId })
				.Where(GetIsUniqueForGroup).ToArray();
			return groups.Length > 0
				? _openedWindows.Where(tuple => groups.Contains(tuple.group)).Select(tuple => tuple.window).ToArray()
				: Array.Empty<IWindow>();
		}

		public IWindow ShowWindow(string windowId, object[] args = null, bool? isUnique = null,
			bool? overlap = null, string windowGroup = null)
		{
			if (!_windowsMap.TryGetValue(windowId, out var prefab))
			{
				Debug.LogErrorFormat("Window with Id {0} isn't registered in Manager.", windowId);
				return null;
			}

			if (!_sceneHelper)
			{
				_sceneHelper = new GameObject(@"WindowManagerLocalSceneHelper",
						typeof(WindowManagerLocalSceneHelper))
					.GetComponent<WindowManagerLocalSceneHelper>();
				_sceneHelper.DestroyEvent.AddListener(OnDestroyScene);
			}

			var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
			instance.name = windowId;

			var window = instance.GetComponent<IWindow>();
			Assert.IsNotNull(window, "Window prefab must implements IWindow.");

			InitWindow(window, args ?? Array.Empty<object>());

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

		public bool RegisterWindow(Window windowPrefab, bool overrideExisting = false)
		{
			var windowId = windowPrefab.WindowId;
			if (_windowsMap.ContainsKey(windowId))
			{
				if (!overrideExisting) return false;

				CloseAll(windowId);
				_windowsMap[windowId] = windowPrefab;
			}
			else
			{
				_windowsMap.Add(windowId, windowPrefab);
			}

			return true;
		}

		public bool UnregisterWindow(string windowId)
		{
			return _windowsMap.Remove(windowId);
		}

		private void OnDestroyScene()
		{
			foreach (var closeHandler in _openedWindows.SelectMany(tuple => tuple.closeHandlers))
			{
				closeHandler.Dispose();
			}

			_delayedWindows.Clear();
			_openedWindows.Clear();
			_isUniqueMap.Clear();

			_sceneHelper.DestroyEvent.RemoveListener(OnDestroyScene);
			_sceneHelper = null;
		}

		private void DoApplyWindow(IWindow window, bool isUnique, bool overlap, string windowGroup)
		{
			SetIsUniqueForGroup(windowGroup, isUnique);

			var closeHandler = window.CloseWindowStream
				.Subscribe(new ObserverImpl<WindowResult>(result => OnCloseWindow(result.Window)));
			var destroyHandler = window.DestroyWindowStream
				.Subscribe(new ObserverImpl<WindowResult>(result => OnDestroyWindow(result.Window)));

			var openedWindowsFromGroupCount = _openedWindows.Count(tuple => tuple.group == windowGroup);
			window.Canvas.sortingOrder = StartCanvasSortingOrder + openedWindowsFromGroupCount +
			                             GetOrderOffsetForGroup(windowGroup);

			var openedWindowsFromGroup = _openedWindows
				.Where(tuple => tuple.group == windowGroup).ToList();
			var overlappedWindow = openedWindowsFromGroup.OrderBy(tuple => tuple.index).LastOrDefault().window;

			_openedWindows.Add((window, new List<IDisposable>(2) { closeHandler, destroyHandler },
				_windowCtr++, windowGroup));

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

			_windowOpenedStream.OnNext(window);
		}

		protected virtual void InitWindow(IWindow window, object[] args)
		{
			window.SetArgs(args);
		}

		private void OnDestroyWindow(IWindow window)
		{
			Debug.LogWarningFormat("The window {0} wasn't closed before destroy.", window.WindowId);
			OnCloseWindow(window);
		}

		private void OnCloseWindow(IWindow window)
		{
			int i;
			string groupId;
			var record = _openedWindows.FirstOrDefault(tuple => tuple.window == window);
			if (record != default)
			{
				i = record.index;
				groupId = record.group;
				record.closeHandlers.ForEach(h => h.Dispose());
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
				Destroy(window.Canvas.gameObject);
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
				IDisposable deactivateHandler = null;
				deactivateHandler = window.ActivatableStateChangesStream
					.Subscribe(new ObserverImpl<ActivatableState>(state =>
					{
						if (state != ActivatableState.Inactive) return;
						// ReSharper disable once AccessToModifiedClosure
						deactivateHandler?.Dispose();
						Destroy(window.Canvas.gameObject);
					}));
			}

			var openedWindowsFromGroup = _openedWindows
				.Where(tuple => tuple.group == groupId).ToList();

			var overlappedWindow = openedWindowsFromGroup
				.Aggregate(((IWindow window, List<IDisposable> closeHandlers, int index, string group))default,
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

			_windowClosedStream.OnNext(window.WindowId);

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

#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Window Manager")]
		private static void FindAndSelectWindowManager()
		{
			var instance = Resources.FindObjectsOfTypeAll<WindowManagerBase>().FirstOrDefault();
			if (!instance)
			{
				LoadAllPrefabs();
				instance = Resources.FindObjectsOfTypeAll<WindowManagerBase>().FirstOrDefault();
			}

			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of WindowManager.");
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
