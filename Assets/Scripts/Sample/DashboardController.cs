using System.Linq;
using Base.WindowManager;
using Sample.Windows;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Sample
{
	[DisallowMultipleComponent]
	public class DashboardController : MonoBehaviour
	{
		private static int _ctr;

#pragma warning disable 649
		[SerializeField] private Button _otherSceneButton;
		[SerializeField] private LineController[] _lines = new LineController[0];

		[Inject] private readonly IWindowManager _windowManager;
#pragma warning restore 649

		private void Start()
		{
			foreach (var line in _lines)
			{
				line.ShowWindowEvent.AddListener(OnShowWindow);
			}

			_otherSceneButton.onClick.AddListener(() => SceneManager.LoadScene(@"OtherScene"));
		}

		private void OnDestroy()
		{
			_otherSceneButton.onClick.RemoveAllListeners();
		}

		private void OnShowWindow(string windowId, bool isUnique, bool overlap)
		{
			_windowManager.ShowWindow(windowId, new object[] {++_ctr}, isUnique, overlap);
/*
			Debug.LogFormat("{0}", string.Join("; ", _windowManager.GetWindows("popup", typeof(FullscreenWindow2))
				.Select(window => window.WindowId)));
*/
		}
	}
}