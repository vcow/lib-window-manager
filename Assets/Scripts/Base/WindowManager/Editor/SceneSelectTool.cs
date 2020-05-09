#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Base.WindowManager.Editor
{
	public class SceneSelectTool : EditorWindow
	{
		private Vector2 _scrollPos;
		private bool _showAllScenes;

		[MenuItem("Tools/Scene Select Tool")]
		internal static void Init()
		{
			var window = (SceneSelectTool) GetWindow(typeof(SceneSelectTool), false, "Scene Select Tool");
			window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 200f, 400f);
		}

		internal void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
			GUILayout.Space(10);
			GUILayout.Label("Settings", EditorStyles.boldLabel);
			_showAllScenes = GUILayout.Toggle(_showAllScenes, "Show All Scenes in the Project");
			GUILayout.Space(10);
			GUILayout.Label("Scenes", EditorStyles.boldLabel);
			// ReSharper disable once InconsistentNaming
			var scenesGUIDs = AssetDatabase.FindAssets("t:Scene");
			var scenesPaths = _showAllScenes
				? scenesGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray()
				: scenesGUIDs.Select(AssetDatabase.GUIDToAssetPath).Where(s => s.StartsWith("Assets/Scenes/")).ToArray();
			DrawScenesList(scenesPaths);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void DrawScenesList(IEnumerable<string> scenesPaths)
		{
			var scenesPathsArray = scenesPaths.ToArray();
			for (var i = 0; i < scenesPathsArray.Length; i++)
			{
				var path = scenesPathsArray[i];
				var sceneName = Path.GetFileNameWithoutExtension(path);
				var pressed = GUILayout.Button(sceneName,
					new GUIStyle(GUI.skin.GetStyle("Button")) {alignment = TextAnchor.MiddleLeft});
				if (pressed)
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
						EditorSceneManager.OpenScene(path);
			}
		}
	}
}
#endif