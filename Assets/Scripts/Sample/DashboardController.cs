using Plugins.vcow.WindowManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Sample
{
	[DisallowMultipleComponent]
	public class DashboardController : MonoBehaviour
	{
		private static int _ctr;

		[SerializeField] private LineController[] _lines = new LineController[0];

		[Inject] private readonly IWindowManager _windowManager;

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
		}

		public void OnLoadScene()
		{
			SceneManager.LoadScene(@"OtherScene");
		}

		public void OnWait()
		{
		}
	}
}