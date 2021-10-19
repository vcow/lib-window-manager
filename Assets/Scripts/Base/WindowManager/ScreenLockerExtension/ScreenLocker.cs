using System;
using Base.Activatable;
using UnityEngine;

namespace Base.WindowManager.ScreenLockerExtension
{
	[DisallowMultipleComponent]
	public abstract class ScreenLocker : MonoBehaviour, IScreenLocker
	{
		public abstract void Activate(bool immediately = false);
		public abstract void Deactivate(bool immediately = false);
		public abstract ActivatableState ActivatableState { get; protected set; }
		public abstract IObservable<ActivatableState> ActivatableStateChangesStream { get; }
		public abstract LockerType LockerType { get; }

		/// <summary>
		/// Мгновенно завершить переходный процесс, если блокировщик находится в одном из переходных состояний (ToActive, ToInactive).
		/// </summary>
		public abstract bool Force();
	}

	public class ScreenLocker<TDerived> : ScreenLocker where TDerived : ScreenLocker<TDerived>
	{
		private ActivatableState _activatableState = ActivatableState.Inactive;
		private readonly ObservableImpl<ActivatableState> _activatableStateChangesStream =
			new ObservableImpl<ActivatableState>();
		
		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override LockerType LockerType => throw new NotImplementedException();

		public override bool Force()
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

		protected virtual void Start()
		{
			DontDestroyOnLoad(gameObject);
		}

		protected virtual void OnDestroy()
		{
			_activatableStateChangesStream.Dispose();
		}

		public override IObservable<ActivatableState> ActivatableStateChangesStream => _activatableStateChangesStream;
	}
}