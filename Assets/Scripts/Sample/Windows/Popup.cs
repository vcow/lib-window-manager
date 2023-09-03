using Base.Activatable;
using Base.WindowManager;
using Base.WindowManager.Template;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public class Popup : PopupWindowBase<EmptyWindowResult>
	{
		private bool _isStarted;
		private Tween _tween;

#pragma warning disable 649
		[SerializeField] private Button _closeButton;
		[SerializeField] private Text _ctrLabel;
#pragma warning restore 649

		[Inject]
		// ReSharper disable once UnusedMember.Local
		private void Construct(DiContainer container)
		{
			container.InjectGameObject(Popup.gameObject);
		}

		protected override string GetWindowId()
		{
			return "popup";
		}

		protected override void DoSetArgs(object[] args)
		{
			foreach (var arg in args)
			{
				switch (arg)
				{
					case int intVal:
						_ctrLabel.text = $"#{intVal}";
						break;
				}
			}
		}

		protected override void DoActivate(bool immediately)
		{
			if (this.IsActiveOrActivated()) return;
			ActivatableState = immediately ? ActivatableState.Active : ActivatableState.ToActive;
			ValidateState();
		}

		protected override void DoDeactivate(bool immediately)
		{
			if (this.IsInactiveOrDeactivated()) return;
			ActivatableState = immediately ? ActivatableState.Inactive : ActivatableState.ToInactive;
			ValidateState();
		}

		private void Start()
		{
			_closeButton.onClick.AddListener(() => Close());

			var popupCanvasGroup = Popup.GetComponent<CanvasGroup>();
			popupCanvasGroup.interactable = false;
			popupCanvasGroup.alpha = 0;
			Popup.localScale = Vector3.one * 0.1f;

			_isStarted = true;
			ValidateState();
		}

		protected override void OnDestroy()
		{
			_closeButton.onClick.RemoveAllListeners();
			_tween?.Kill();
			base.OnDestroy();
		}

		private void ValidateState()
		{
			if (!_isStarted) return;

			_tween?.Kill();
			_tween = null;

			var popupCanvasGroup = Popup.GetComponent<CanvasGroup>();
			switch (ActivatableState)
			{
				case ActivatableState.Active:
					popupCanvasGroup.interactable = true;
					popupCanvasGroup.alpha = 1;
					Popup.localScale = Vector3.one;
					break;
				case ActivatableState.Inactive:
					popupCanvasGroup.interactable = false;
					popupCanvasGroup.alpha = 0;
					Popup.localScale = Vector3.one * 0.1f;
					break;
				case ActivatableState.ToActive:
					_tween = DOTween.Sequence()
						.Append(Popup.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack))
						.Join(popupCanvasGroup.DOFade(1, 0.3f))
						.OnComplete(() =>
						{
							_tween = null;
							popupCanvasGroup.interactable = true;
							ActivatableState = ActivatableState.Active;
						});
					break;
				case ActivatableState.ToInactive:
					popupCanvasGroup.interactable = false;
					_tween = DOTween.Sequence()
						.Append(Popup.DOScale(Vector3.one * 0.1f, 1f).SetEase(Ease.InBack))
						.Join(popupCanvasGroup.DOFade(0, 0.3f).SetDelay(0.7f))
						.OnComplete(() =>
						{
							_tween = null;
							ActivatableState = ActivatableState.Inactive;
						});
					break;
			}
		}

		~Popup()
		{
			Debug.Log("Popup destroyed.");
		}
	}
}