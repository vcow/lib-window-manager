using Base.Activatable;
using Base.WindowManager.ScreenLockerExtension;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sample
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class CommonScreenLockerBase : ScreenLocker<CommonScreenLockerBase>
	{
		private bool _isStarted;
		private CanvasGroup _canvasGroup;
		private Tween _tween;

		private void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.interactable = false;
			_canvasGroup.alpha = 0;
		}

		private void Start()
		{
			_isStarted = true;
			ValidateState();
		}

		protected override void OnDestroy()
		{
			_tween?.Kill();
			base.OnDestroy();
		}

		public override void Activate(bool immediately = false)
		{
			Assert.IsFalse(this.IsActiveOrActivated());
			ActivatableState = immediately ? ActivatableState.Active : ActivatableState.ToActive;
			ValidateState();
		}

		public override void Deactivate(bool immediately = false)
		{
			Assert.IsFalse(this.IsInactiveOrDeactivated());
			ActivatableState = immediately ? ActivatableState.Inactive : ActivatableState.ToInactive;
			ValidateState();
		}

		private void ValidateState()
		{
			if (!_isStarted) return;

			_tween?.Kill();
			_tween = null;

			switch (ActivatableState)
			{
				case ActivatableState.Active:
					_canvasGroup.alpha = 1;
					break;
				case ActivatableState.Inactive:
					_canvasGroup.alpha = 0;
					break;
				case ActivatableState.ToActive:
					_tween = _canvasGroup.DOFade(1, 1).OnComplete(() =>
					{
						_tween = null;
						_canvasGroup.interactable = true;
						ActivatableState = ActivatableState.Active;
					});
					break;
				case ActivatableState.ToInactive:
					_canvasGroup.interactable = false;
					_tween = _canvasGroup.DOFade(0, 1).OnComplete(() =>
					{
						_tween = null;
						ActivatableState = ActivatableState.Inactive;
					});
					break;
			}
		}
	}
}