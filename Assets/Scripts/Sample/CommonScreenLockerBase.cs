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
		private float _alpha = 0;

		private void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.interactable = false;
		}

		protected override void Start()
		{
			base.Start();
			_isStarted = true;
			_canvasGroup.alpha = _alpha;
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
			if (!ValidateState() && immediately)
			{
				_alpha = 1;
			}
		}

		public override void Deactivate(bool immediately = false)
		{
			Assert.IsFalse(this.IsInactiveOrDeactivated());
			ActivatableState = immediately ? ActivatableState.Inactive : ActivatableState.ToInactive;
			if (!ValidateState() && immediately)
			{
				_alpha = 0;
			}
		}

		public override bool Force()
		{
			switch (ActivatableState)
			{
				case ActivatableState.ToActive:
					ActivatableState = ActivatableState.Active;
					ValidateState();
					break;
				case ActivatableState.ToInactive:
					ActivatableState = ActivatableState.Inactive;
					ValidateState();
					break;
				default:
					return false;
			}

			return true;
		}

		private bool ValidateState()
		{
			if (!_isStarted) return false;

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

			return true;
		}
	}
}