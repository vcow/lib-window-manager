using System.Collections.Generic;
using UnityEngine;

namespace Base.WindowManager
{
    [CreateAssetMenu(fileName = "CommonWindowProvider", menuName = "Window Manager/Common Window Provider")]
    public class CommonWindowProvider : WindowProviderBase
    {
#pragma warning disable 649
        [SerializeField] private Window[] _windows = new Window[0];
#pragma warning restore 649

        public override IReadOnlyList<Window> Windows => _windows;
    }
}