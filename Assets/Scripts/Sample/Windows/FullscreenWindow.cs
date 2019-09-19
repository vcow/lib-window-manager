using Base.Activatable;
using Base.WindowManager;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public abstract class FullscreenWindow : Window<FullscreenWindow>
	{
		private bool _isStarted;
		private float _offset;
		private Vector2 _initialPosition;
		private Tween _tween;
		private CanvasGroup _windowCanvasGroup;

#pragma warning disable 649
		[SerializeField] private RectTransform _window;
		[SerializeField] private Button _closeButton;
		[SerializeField] private Text _ctrLabel;
#pragma warning restore 649

		[Inject]
		// ReSharper disable once UnusedMember.Local
		private void Construct(DiContainer container)
		{
			container.InjectGameObject(_window.gameObject);
		}

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

			_offset = _window.rect.size.x;
			_initialPosition = _window.anchoredPosition;
			_window.anchoredPosition = new Vector2(_initialPosition.x - _offset, _initialPosition.y);

			_windowCanvasGroup = _window.GetComponent<CanvasGroup>();
			if (!_windowCanvasGroup) _windowCanvasGroup = _window.gameObject.AddComponent<CanvasGroup>();
			_windowCanvasGroup.interactable = false;

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
				case ActivatableState.Inactive:
					_window.anchoredPosition = new Vector2(_initialPosition.x - _offset, _initialPosition.y);
					break;
				case ActivatableState.Active:
					_window.anchoredPosition = _initialPosition;
					break;
				case ActivatableState.ToActive:
					_tween = _window.DOAnchorPosX(_initialPosition.x, 1f)
						.OnComplete(() =>
						{
							_tween = null;
							_windowCanvasGroup.interactable = true;
							ActivatableState = ActivatableState.Active;
						});
					break;
				case ActivatableState.ToInactive:
					_windowCanvasGroup.interactable = false;
					_tween = _window.DOAnchorPosX(_initialPosition.x + _offset, 1f)
						.OnComplete(() =>
						{
							_tween = null;
							_window.anchoredPosition = new Vector2(_initialPosition.x - _offset, _initialPosition.y);
							ActivatableState = ActivatableState.Inactive;
						});
					break;
			}
		}
	}
}