using System;
using Base.Activatable;
using UnityEngine;

namespace Base.WindowManager.Extensions.ScreenLockerExtension
{
	/// <summary>
	/// The base class of the Screen Locker component, that implements IScreenLocker.
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class ScreenLockerBase : MonoBehaviour, IScreenLocker
	{
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract LockerType LockerType { get; }
		public abstract event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		/// <summary>
		/// This method immediately finished any transition process (if locker is in the ToActive or ToInactive state).
		/// </summary>
		public abstract bool Force();
	}

	/// <summary>
	/// The base class of the Screen Locker component, which should be added to the screen locker window prefab.
	/// </summary>
	/// <typeparam name="TDerived">Locker component class derived from the ScreenLocker.</typeparam>
	public class ScreenLocker<TDerived> : ScreenLockerBase where TDerived : ScreenLocker<TDerived>
	{
		private ActivatableState _activatableState = ActivatableState.Inactive;
		public override event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException("Implement an Activate() method of the " +
			                                  "ScreenLocker in the derived class.");
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException("Implement a Deactivate() method of the " +
			                                  "ScreenLocker in the derived class.");
		}

		public override LockerType LockerType => throw new NotImplementedException("Return LockerType of the " +
			"ScreenLocker from the derived class.");

		public override bool Force()
		{
			throw new NotImplementedException("Implement a Force() method of the " +
			                                  "ScreenLocker in the derived class.");
		}

		public override ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState) return;
				_activatableState = value;
				ActivatableStateChangedEvent?.Invoke(this, _activatableState);
			}
		}

		protected virtual void Start()
		{
			DontDestroyOnLoad(gameObject);
		}

		protected virtual void OnDestroy()
		{
			ActivatableStateChangedEvent = null;
		}
	}
}