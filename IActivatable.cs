namespace Base.Activatable
{
	/// <summary>
	/// Activatqble states.
	/// </summary>
	public enum ActivatableState
	{
		Inactive,
		Active,
		ToActive,
		ToInactive
	}

	/// <summary>
	/// Delegate for state change event.
	/// </summary>
	public delegate void ActivatableStateChangedHandler(IActivatable activatable, ActivatableState state);

	/// <summary>
	/// Activatable - is the entity that can take an active/inactive state, as well as intermediate states
	/// when moving from inactive to active and vice versa.
	/// </summary>
	public interface IActivatable
	{
		/// <summary>
		/// Current state of the activatable object.
		/// </summary>
		ActivatableState ActivatableState { get; }

		/// <summary>
		/// Activtable object state change event.
		/// </summary>
		event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		/// <summary>
		/// Activate object.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий активировать объект немедленно.</param>
		void Activate(bool immediately = false);

		/// <summary>
		/// Deactivate object.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий деактивировать объект немедленно.</param>
		void Deactivate(bool immediately = false);
	}
}