using System.Collections.Generic;
using Base.WindowManager;
using UnityEngine;

namespace Sample
{
    [CreateAssetMenu(fileName = "CommonWindowProvider", menuName = "Window Manager/Common Window Provider")]
    public class CommonWindowProvider : WindowProviderBase
    {
#pragma warning disable 649
        [SerializeField] private Window[] _windows;
#pragma warning restore 649

        public override IReadOnlyList<Window> Windows => _windows;
    }
}