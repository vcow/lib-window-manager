using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Plugins.vcow.WindowManager
{
	public struct WindowManagerSettings
	{
		public IEnumerable<string> GroupHierarchy;
		public IEnumerable<WindowsPrefabLibrary> WindowLibraries;
		public int StartCanvasSortingOrder;
	}
}