using System;
using Base.Activatable;
using UnityEngine;
using UnityEngine.UI;

namespace Base.WindowManager
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public abstract class Window : MonoBehaviour, IWindow
	{
		private Canvas _canvas;
		private CanvasScaler _canvasScaler;
		private GraphicRaycaster _graphicRaycaster;

#pragma warning disable 649
		[SerializeField] private string _windowGroup;
		[SerializeField] private bool _isUnique;
		[SerializeField] private bool _overlap;
#pragma warning restore 649

		protected CanvasScaler CanvasScaler =>
			_canvasScaler ? _canvasScaler : _canvasScaler = GetComponent<CanvasScaler>();

		public GraphicRaycaster GraphicRaycaster =>
			_graphicRaycaster ? _graphicRaycaster : _graphicRaycaster = GetComponent<GraphicRaycaster>();

		public Canvas Canvas => _canvas ? _canvas : _canvas = GetComponent<Canvas>();
		public abstract string WindowId { get; }
		public virtual string WindowGroup => _windowGroup ?? string.Empty;
		public bool IsUnique => _isUnique;
		public bool Overlap => _overlap;
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract IObservable<ActivatableState> ActivatableStateChangesStream { get; }
		public abstract bool Close(bool immediately = false);
		public abstract void SetArgs(object[] args);
		public abstract bool IsClosed { get; }
		public abstract IObservable<WindowResult> CloseWindowStream { get; }
		public abstract IObservable<WindowResult> DestroyWindowStream { get; }
	}

	public class Window<TDerived> : Window where TDerived : Window<TDerived>
	{
		private bool _isClosed;
		private ActivatableState _activatableState = ActivatableState.Inactive;

		private readonly ObservableImpl<ActivatableState> _activatableStateChangesStream =
			new ObservableImpl<ActivatableState>();

		private readonly ObservableImpl<WindowResult> _closeWindowStream = new ObservableImpl<WindowResult>();
		private readonly ObservableImpl<WindowResult> _destroyWindowStream = new ObservableImpl<WindowResult>();

		protected WindowResult Result = null;

		public override string WindowId => throw new NotImplementedException();

		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState) return;
				_activatableState = value;
				_activatableStateChangesStream.OnNext(_activatableState);
			}
		}

		public override IObservable<ActivatableState> ActivatableStateChangesStream => _activatableStateChangesStream;

		public override bool Close(bool immediately = false)
		{
			if (_isClosed || this.IsInactiveOrDeactivated()) return false;

			if (ActivatableState == ActivatableState.ToActive)
			{
				Debug.LogWarningFormat("Trying to close window {0} before it was activated.", GetType().FullName);

				IDisposable autoCloseHandler = null;
				autoCloseHandler = ActivatableStateChangesStream
					.Subscribe(new ObserverImpl<ActivatableState>(state =>
					{
						if (state != ActivatableState.Active) return;
						// ReSharper disable once AccessToModifiedClosure
						autoCloseHandler?.Dispose();
						Close(immediately);
					}));

				return true;
			}

			_isClosed = true;

			_closeWindowStream.OnNext(Result ?? new WindowResult<EmptyWindowResult>(this, EmptyWindowResult.Instance));
			_closeWindowStream.OnCompleted();

			Deactivate(immediately);
			return true;
		}

		protected virtual void OnDestroy()
		{
			_destroyWindowStream.OnNext(
				Result ?? new WindowResult<EmptyWindowResult>(this, EmptyWindowResult.Instance));
			_destroyWindowStream.OnCompleted();

			_activatableStateChangesStream.Dispose();
			_closeWindowStream.Dispose();
			_destroyWindowStream.Dispose();
		}

		public override void SetArgs(object[] args)
		{
			throw new NotImplementedException();
		}

		public override bool IsClosed => _isClosed;

		public override IObservable<WindowResult> CloseWindowStream => _closeWindowStream;

		public override IObservable<WindowResult> DestroyWindowStream => _destroyWindowStream;
	}
}