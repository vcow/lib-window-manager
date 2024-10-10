using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace vcow.UIWindowManager
{
    [CreateAssetMenu(fileName = "WindowsPrefabLibrary", menuName = "Window Manager/Windows Prefab Library")]
    public class WindowsPrefabLibrary : ScriptableObject
    {
        [SerializeField] private Window[] _windows;
        public IReadOnlyList<Window> Windows => _windows;
    }
}