using Base.Activatable;
using Base.WindowManager;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public class ModalPopup : ModalPopupBase
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

		public override string WindowId => "modal_popup";

		public override void Activate(bool immediately = false)
		{
			if (this.IsActiveOrActivated()) return;
			ActivatableState = immediately ? ActivatableState.Active : ActivatableState.ToActive;
			ValidateState();
		}

		public override void Deactivate(bool immediately = false)
		{
			if (this.IsInactiveOrDeactivated()) return;
			ActivatableState = immediately ? ActivatableState.Inactive : ActivatableState.ToInactive;
			ValidateState();
		}

		public override void SetArgs(object[] args)
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

		protected override void Start()
		{
			base.Start();

			_closeButton.onClick.AddListener(() => Close());

			PopupCanvasGroup.interactable = false;
			PopupCanvasGroup.alpha = 0;
			Popup.localScale = Vector3.one * 0.1f;
			RawImage.color = Color.clear;

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

			switch (ActivatableState)
			{
				case ActivatableState.Active:
					PopupCanvasGroup.interactable = true;
					PopupCanvasGroup.alpha = 1;
					Popup.localScale = Vector3.one;
					RawImage.color = new Color(0, 0, 0, 0.5f);
					break;
				case ActivatableState.Inactive:
					PopupCanvasGroup.interactable = false;
					PopupCanvasGroup.alpha = 0;
					Popup.localScale = Vector3.one * 0.1f;
					RawImage.color = Color.clear;
					break;
				case ActivatableState.ToActive:
					_tween = DOTween.Sequence()
						.Append(Popup.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack))
						.Join(RawImage.DOFade(0.5f, 0.5f))
						.Join(PopupCanvasGroup.DOFade(1, 0.3f))
						.OnComplete(() =>
						{
							_tween = null;
							PopupCanvasGroup.interactable = true;
							ActivatableState = ActivatableState.Active;
						});
					break;
				case ActivatableState.ToInactive:
					PopupCanvasGroup.interactable = false;
					_tween = DOTween.Sequence()
						.Append(Popup.DOScale(Vector3.one * 0.1f, 1f).SetEase(Ease.InBack))
						.Join(DOTween.Sequence()
							.Append(RawImage.DOFade(0, 0.5f))
							.Join(PopupCanvasGroup.DOFade(0, 0.3f).SetDelay(0.2f))
							.SetDelay(0.5f))
						.OnComplete(() =>
						{
							_tween = null;
							ActivatableState = ActivatableState.Inactive;
						});
					break;
			}
		}
	}
}