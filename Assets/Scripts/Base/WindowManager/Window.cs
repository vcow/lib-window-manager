using System;
using Base.Activatable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Base.WindowManager
{
	/// <summary>
	/// Common abstract base Window component for references, used in the WindowManager.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public abstract class Window : MonoBehaviour, IWindow
	{
		private Canvas _canvas;
		private CanvasScaler _canvasScaler;
		private GraphicRaycaster _graphicRaycaster;

		// ReSharper disable InconsistentNaming
		private event CloseWindowHandler _closeWindowEvent;
		private event DestroyWindowHandler _destroyWindowEvent;
		// ReSharper restore InconsistentNaming

		public event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		event CloseWindowHandler IWindow.CloseWindowEvent
		{
			add => _closeWindowEvent += value;
			remove => _closeWindowEvent -= value;
		}

		event DestroyWindowHandler IWindow.DestroyWindowEvent
		{
			add => _destroyWindowEvent += value;
			remove => _destroyWindowEvent -= value;
		}

#pragma warning disable 649
		[SerializeField] private string _windowGroup;
		[SerializeField] private bool _isUnique;
		[SerializeField] private bool _overlap;
#pragma warning restore 649

		protected void InvokeActivatableStateChangedEvent(ActivatableState state)
		{
			ActivatableStateChangedEvent?.Invoke(this, state);
		}

		protected void InvokeCloseWindowEvent(object result)
		{
			_closeWindowEvent?.Invoke(this, result);
		}

		protected void InvokeDestroyWindowEvent(object result)
		{
			_destroyWindowEvent?.Invoke(this, result);
		}

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
		public abstract bool Close(bool immediately = false);
		public abstract void SetArgs(object[] args);
		public abstract bool IsClosed { get; }

		public virtual void Dispose()
		{
			_closeWindowEvent = null;
			_destroyWindowEvent = null;
			ActivatableStateChangedEvent = null;
		}
	}

	/// <summary>
	/// The final base Window component.
	/// </summary>
	/// <typeparam name="TDerived">Self type.</typeparam>
	/// <typeparam name="TResult">Returned result type.</typeparam>
	public class Window<TDerived, TResult> : Window where TDerived : Window<TDerived, TResult>
	{
		/// <summary>
		/// Delegate for close event of specific Window with the result of TResult type.
		/// </summary>
		public delegate void CloseWindowHandler(IWindow window, TResult result);

		/// <summary>
		/// Delegate for destroy event of specific Window with the result of TResult type.
		/// </summary>
		public delegate void DestroyWindowHandler(IWindow window, TResult result);

		private bool _isClosed;
		private ActivatableState _activatableState = ActivatableState.Inactive;

		protected TResult Result = default;
		protected bool IsDisposed { get; private set; }

		public event CloseWindowHandler CloseWindowEvent;
		public event DestroyWindowHandler DestroyWindowEvent;

		public override string WindowId => throw new NotImplementedException(
			"Specify the WindowId in the inherited class.");

		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException("Crete activation behaviour for the Window in the inherited class. " +
			                                  "Use IsDisposed flag to check the current state of the Window.");
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException("Crete deactivation behaviour for the Window in the inherited class. " +
			                                  "Use IsDisposed flag to check the current state of the Window.");
		}

		public override ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState || IsDisposed)
				{
					return;
				}

				_activatableState = value;
				InvokeActivatableStateChangedEvent(_activatableState);
			}
		}

		public override bool Close(bool immediately = false)
		{
			if (_isClosed || IsDisposed || this.IsInactiveOrDeactivated())
			{
				return false;
			}

			if (ActivatableState == ActivatableState.ToActive)
			{
				Debug.LogWarningFormat("Trying to close window {0} before it was activated.", GetType().FullName);

				ActivatableStateChangedHandler autoCloseHandler = null;
				autoCloseHandler = (activatable, state) =>
				{
					if (state != ActivatableState.Active)
					{
						return;
					}

					ActivatableStateChangedEvent -= autoCloseHandler;
					Close(immediately);
				};

				ActivatableStateChangedEvent += autoCloseHandler;
				return true;
			}

			_isClosed = true;
			InvokeCloseWindowEvent();

			Deactivate(immediately);
			return true;
		}

		protected virtual void OnDestroy()
		{
			if (IsDisposed)
			{
				return;
			}

			InvokeDestroyWindowEvent();
			Dispose();
		}

		public override void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			base.Dispose();

			CloseWindowEvent = null;
			DestroyWindowEvent = null;
		}

		private void InvokeCloseWindowEvent()
		{
			Assert.IsFalse(IsDisposed, "Window was disposed before CloseWindowEvent invoked.");

			CloseWindowEvent?.Invoke(this, Result);
			base.InvokeCloseWindowEvent(Result);
		}

		private void InvokeDestroyWindowEvent()
		{
			Assert.IsFalse(IsDisposed, "Window was disposed before DestroyWindowEvent invoked.");

			DestroyWindowEvent?.Invoke(this, Result);
			base.InvokeDestroyWindowEvent(Result);
		}

		public override void SetArgs(object[] args)
		{
			throw new NotImplementedException("Get and process arguments sent from the WindowManager " +
			                                  "in the inherited class.");
		}

		public override bool IsClosed => _isClosed;

		~Window()
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogFormat("The window {0} was successfully destroyed.", WindowId);
			}
		}
	}
}