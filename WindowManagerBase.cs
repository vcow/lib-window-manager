using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Base.WindowManager
{
	[DisallowMultipleComponent]
	public abstract class WindowManagerBase : MonoBehaviour, IWindowManager
	{
		private Dictionary<string, Window> _windowsMap = new Dictionary<string, Window>();

#pragma warning disable 649
		[SerializeField] private Window[] _windows = new Window[0];
#pragma warning restore 649

		protected abstract int StartCanvasSortingOrder { get; }

		protected virtual void Awake()
		{
			_windowsMap = _windows.ToDictionary(window => window.WindowId, window => window);
		}
	}
}