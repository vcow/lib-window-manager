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

		protected CanvasScaler CanvasScaler =>
			_canvasScaler ? _canvasScaler : _canvasScaler = GetComponent<CanvasScaler>();

		public GraphicRaycaster GraphicRaycaster =>
			_graphicRaycaster ? _graphicRaycaster : _graphicRaycaster = GetComponent<GraphicRaycaster>();

		public Canvas Canvas => _canvas ? _canvas : _canvas = GetComponent<Canvas>();
		public abstract string WindowId { get; }
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract event EventHandler<ActivatableStateChangedEventArgs> ActivatableStateChangedEvent;
		public abstract bool Close(bool immediately = false);
		public abstract void SetArgs(object[] args);
		public abstract event EventHandler<WindowResultEventArgs> CloseWindowEvent;
		public abstract event EventHandler<WindowResultEventArgs> DestroyWindowEvent;
	}

	public class Window<TDerived> : Window where TDerived : Window<TDerived>
	{
		private bool _isClosed;
		private ActivatableState _activatableState = ActivatableState.Inactive;

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
				var args = new ActivatableStateChangedEventArgs(value, _activatableState);
				_activatableState = value;
				ActivatableStateChangedEvent?.Invoke(this, args);
			}
		}

		public override event EventHandler<ActivatableStateChangedEventArgs> ActivatableStateChangedEvent;

		public override bool Close(bool immediately = false)
		{
			if (_isClosed || this.IsInactiveOrDeactivated()) return false;

			if (!this.IsActive() && this.IsActiveOrActivated())
			{
				Debug.LogWarningFormat("Trying to close window {0} before it was activated.", GetType().FullName);

				void OnActivatableStateChanged(object sender, EventArgs args)
				{
					var activatableStateChangedEventArgs = (ActivatableStateChangedEventArgs) args;
					if (activatableStateChangedEventArgs.CurrentState != ActivatableState.Active) return;
					ActivatableStateChangedEvent -= OnActivatableStateChanged;
					Close(immediately);
				}

				ActivatableStateChangedEvent += OnActivatableStateChanged;
				return true;
			}

			_isClosed = true;
			CloseWindowEvent?.Invoke(this, null);
			Deactivate(immediately);
			return true;
		}

		protected virtual void OnDestroy()
		{
			ActivatableStateChangedEvent = null;
			CloseWindowEvent = null;

			DestroyWindowEvent?.Invoke(this, null);
			DestroyWindowEvent = null;
		}

		public override void SetArgs(object[] args)
		{
			throw new NotImplementedException();
		}

		public override event EventHandler<WindowResultEventArgs> CloseWindowEvent;

		public override event EventHandler<WindowResultEventArgs> DestroyWindowEvent;
	}
}