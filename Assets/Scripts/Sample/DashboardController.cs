using Base.WindowManager;
using UnityEngine;
using Zenject;

namespace Sample
{
	[DisallowMultipleComponent]
	public class DashboardController : MonoBehaviour
	{
		private static int _ctr;

#pragma warning disable 649
		[SerializeField] private LineController[] _lines = new LineController[0];

		[Inject] private readonly IWindowManager _windowManager;
#pragma warning restore 649

		private void Start()
		{
			foreach (var line in _lines)
			{
				line.ShowWindowEvent.AddListener(OnShowWindow);
			}
		}

		private void OnShowWindow(string windowId, bool isUnique, bool overlap)
		{
			_windowManager.ShowWindow(windowId, new object[] {++_ctr}, isUnique, overlap);
		}
	}
}