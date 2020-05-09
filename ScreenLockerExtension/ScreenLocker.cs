using System;
using Base.Activatable;
using Base.ScreenLocker;
using UnityEngine;

namespace Base.WindowManager.ScreenLockerExtension
{
	[DisallowMultipleComponent]
	public abstract class ScreenLocker : MonoBehaviour, IScreenLocker
	{
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract event EventHandler ActivatableStateChangedEvent;
		public abstract LockerType LockerType { get; }
	}

	public class ScreenLocker<TDerived> : ScreenLocker where TDerived : ScreenLocker<TDerived>
	{
		private ActivatableState _activatableState = ActivatableState.Inactive;

		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override LockerType LockerType => throw new NotImplementedException();

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

		protected virtual void OnDestroy()
		{
			ActivatableStateChangedEvent = null;
		}

		public override event EventHandler ActivatableStateChangedEvent;
	}
}