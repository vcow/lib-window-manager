using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using vcow.UIWindowManager;

// ReSharper disable once CheckNamespace
namespace UIWindowManager.Editor
{
	[CustomEditor(typeof(WindowManager), true)]
	public class WindowManagerEditor : UnityEditor.Editor
	{
		private ReorderableList _groupHierarchyList;
		private ReorderableList _windowProvidersList;

		private void OnEnable()
		{
			var hierarchyProp = serializedObject.FindProperty(@"_groupHierarchy");
			_groupHierarchyList = new ReorderableList(serializedObject, hierarchyProp, false, true, true, true);
			_groupHierarchyList.drawHeaderCallback += rect =>
			{
				GUI.Label(rect, new GUIContent(hierarchyProp.displayName));
			};
			_groupHierarchyList.drawElementCallback += (rect, index, active, focused) =>
			{
				EditorGUI.PropertyField(rect, hierarchyProp.GetArrayElementAtIndex(index), GUIContent.none, true);
			};

			var providersProp = serializedObject.FindProperty(@"_windowProviders");
			_windowProvidersList = new ReorderableList(serializedObject, providersProp, false, true, true, true);
			_windowProvidersList.drawHeaderCallback += rect =>
			{
				GUI.Label(rect, new GUIContent(providersProp.displayName));
			};
			_windowProvidersList.drawElementCallback += (rect, index, active, focused) =>
			{
				EditorGUI.PropertyField(rect, providersProp.GetArrayElementAtIndex(index), GUIContent.none, true);
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (Application.isPlaying) GUI.enabled = false;
			_groupHierarchyList.DoLayoutList();
			_windowProvidersList.DoLayoutList();
			GUI.enabled = true;
			serializedObject.ApplyModifiedProperties();
		}
	}
}