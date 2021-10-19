using Base.WindowManager;
using Base.WindowManager.ScreenLockerExtension;
using UnityEngine;
using UnityEngine.SceneManagement;
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
		[Inject] private readonly LazyInject<IScreenLockerManager> _screenLockerManager;
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
/*
			Debug.LogFormat("{0}", string.Join("; ", _windowManager.GetWindows("popup", typeof(FullscreenWindow2))
				.Select(window => window.WindowId)));
*/
		}

		public void OnLoadGame()
		{
			_screenLockerManager.Value.Lock(LockerType.GameLoader, () => _screenLockerManager.Value.Unlock(null));
		}

		public void OnLoadScene()
		{
			_screenLockerManager.Value.Lock(LockerType.SceneLoader, () => SceneManager.LoadScene(@"OtherScene"));
		}

		public void OnWait()
		{
			_screenLockerManager.Value.Lock(LockerType.BusyWait, () => _screenLockerManager.Value.Unlock(null));
		}
	}
}