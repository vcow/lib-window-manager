#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Linq;
#endif
using System.Collections.Generic;
using Base.WindowManager.Extensions.ScreenLockerExtension;
using UnityEngine;
using Zenject;

namespace Sample
{
	[CreateAssetMenu(fileName = "ScreenLockerSettings", menuName = "Screen Locker Settings")]
	public class ScreenLockerSettings : ScriptableObjectInstaller<ScreenLockerSettings>
	{
#pragma warning disable 649
		[SerializeField] private List<ScreenLockerBase> _screenLockers = new List<ScreenLockerBase>();
#pragma warning restore 649

		public override void InstallBindings()
		{
			Container.Bind<ScreenLockerSettings>().FromInstance(this).AsSingle();
		}

		public IReadOnlyList<ScreenLockerBase> ScreenLockers => _screenLockers;

#if UNITY_EDITOR
		[MenuItem("Tools/Game Settings/Screen Locker Manager")]
		private static void FindAndSelectWindowManager()
		{
			var instance = Resources.FindObjectsOfTypeAll<ScreenLockerSettings>().FirstOrDefault();
			if (!instance)
			{
				LoadAllPrefabs();
				instance = Resources.FindObjectsOfTypeAll<ScreenLockerSettings>().FirstOrDefault();
			}

			if (instance)
			{
				Selection.activeObject = instance;
				return;
			}

			Debug.LogError("Can't find prefab of ScreenLockerSettings.");
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