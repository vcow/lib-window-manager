using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.WindowManager.Editor
{
	[CustomEditor(typeof(WindowManagerBase), true)]
	public class WindowManagerEditor : UnityEditor.Editor
	{
		private ReorderableList _windowsList;

		private void OnEnable()
		{
			var prop = serializedObject.FindProperty(@"_windows");
			_windowsList = new ReorderableList(serializedObject, prop, false, true, true, true);
			_windowsList.drawHeaderCallback += rect => { GUI.Label(rect, new GUIContent(prop.displayName)); };
			_windowsList.drawElementCallback += (rect, index, active, focused) =>
			{
				EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (Application.isPlaying) GUI.enabled = false;
			_windowsList.DoLayoutList();
			GUI.enabled = true;
			serializedObject.ApplyModifiedProperties();
		}
	}
}