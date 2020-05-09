#if UNITY_EDITOR
using Base.ScreenLocker;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.WindowManager.ScreenLockerExtension.Editor
{
	[CustomEditor(typeof(ScreenLockerManagerBase), true)]
	public class ScreenLockerManagerEditor : UnityEditor.Editor
	{
		private ReorderableList _lockersList;

		private void OnEnable()
		{
			var prop = serializedObject.FindProperty(@"_lockers");
			_lockersList = new ReorderableList(serializedObject, prop, false, true, true, true);
			_lockersList.drawHeaderCallback += rect => { GUI.Label(rect, new GUIContent(prop.displayName)); };
			_lockersList.drawElementCallback += (rect, index, active, focused) =>
			{
				EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (Application.isPlaying) GUI.enabled = false;
			_lockersList.DoLayoutList();
			GUI.enabled = true;
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
