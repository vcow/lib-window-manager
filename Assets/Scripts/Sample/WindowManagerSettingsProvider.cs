using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using vcow.UIWindowManager;
using Zenject;

namespace Sample
{
	[CreateAssetMenu(fileName = "WindowManagerSettingsProvider", menuName = "Window Manager/Window Manager Settings Provider")]
	public class WindowManagerSettingsProvider : ScriptableObjectInstaller<WindowManagerSettingsProvider>
	{
		[FormerlySerializedAs("GroupHierarchy")] [SerializeField] private string[] _groupHierarchy;
		[FormerlySerializedAs("StartCanvasSortingOrder")] [SerializeField] private int _startCanvasSortingOrder;
		[FormerlySerializedAs("_windowProviders")] [SerializeField] private WindowsPrefabLibrary[] _windowLibraries;

		public override void InstallBindings()
		{
			Container.Bind<WindowManagerSettings>().FromMethod(WindowManagerSettingsFactory).AsTransient();
		}

		private WindowManagerSettings WindowManagerSettingsFactory() => new WindowManagerSettings
		{
			GroupHierarchy = _groupHierarchy,
			WindowLibraries = _windowLibraries,
			StartCanvasSortingOrder = _startCanvasSortingOrder
		};

#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Window Manager Settings")]
		private static void FindWindowManagerSettingsProvider()
		{
			var instance = Resources.FindObjectsOfTypeAll<WindowManagerSettingsProvider>().FirstOrDefault();
			if (!instance)
			{
				LoadAllPrefabs();
				instance = Resources.FindObjectsOfTypeAll<WindowManagerSettingsProvider>().FirstOrDefault();
			}

			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of WindowManagerSettingsProvider.");
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