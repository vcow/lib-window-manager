using System.Collections.Generic;
using UnityEngine;

namespace Base.WindowManager
{
    public abstract class WindowProviderBase : ScriptableObject
    {
        public abstract IReadOnlyList<Window> Windows { get; }
    }
}