using Base.Activatable;
using Base.WindowManager;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public class Popup : PopupBase
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

		public override string WindowId => "popup";

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

		private void Start()
		{
			_closeButton.onClick.AddListener(() => Close());

			PopupCanvasGroup.interactable = false;
			PopupCanvasGroup.alpha = 0;
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

			switch (ActivatableState)
			{
				case ActivatableState.Active:
					PopupCanvasGroup.interactable = true;
					PopupCanvasGroup.alpha = 1;
					Popup.localScale = Vector3.one;
					break;
				case ActivatableState.Inactive:
					PopupCanvasGroup.interactable = false;
					PopupCanvasGroup.alpha = 0;
					Popup.localScale = Vector3.one * 0.1f;
					break;
				case ActivatableState.ToActive:
					_tween = DOTween.Sequence()
						.Append(Popup.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack))
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
						.Join(PopupCanvasGroup.DOFade(0, 0.3f).SetDelay(0.7f))
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