using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sample
{
	[DisallowMultipleComponent]
	public class LineController : MonoBehaviour
	{
		private class ShowWindow : UnityEvent<string, bool, bool>
		{
		}

#pragma warning disable 649
		[SerializeField] private string _windowId;
		[SerializeField] private Button _button;
		[SerializeField] private Toggle _uniqueToggle;
		[SerializeField] private Toggle _overlapToggle;
#pragma warning restore 649

		public UnityEvent<string, bool, bool> ShowWindowEvent { get; } = new ShowWindow();

		private void Start()
		{
			_button.onClick.AddListener(
				() => ShowWindowEvent.Invoke(_windowId, _uniqueToggle.isOn, _overlapToggle.isOn));
		}

		private void OnDestroy()
		{
			_button.onClick.RemoveAllListeners();
			ShowWindowEvent.RemoveAllListeners();
		}
	}
}