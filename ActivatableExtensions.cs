namespace Base.Activatable
{
	/// <summary>
	/// Activatable extensions.
	/// </summary>
	public static class ActivatableExtensions
	{
		/// <summary>
		/// Check activatable object activity.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is active.</returns>
		public static bool IsActive(this IActivatable a)
		{
			return a.ActivatableState == ActivatableState.Active;
		}

		/// <summary>
		/// Check activatable object inactivity.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is inactive.</returns>
		public static bool IsInactive(this IActivatable a)
		{
			return a.ActivatableState == ActivatableState.Inactive;
		}

		/// <summary>
		/// Check activatable object for a transition state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is activated or deactivated right now.</returns>
		public static bool IsBusy(this IActivatable a)
		{
			return a.ActivatableState == ActivatableState.ToActive ||
			       a.ActivatableState == ActivatableState.ToInactive;
		}

		/// <summary>
		/// Check activatable object for active or transit to the active state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is active or activated right now.</returns>
		public static bool IsActiveOrActivated(this IActivatable a)
		{
			return a.ActivatableState == ActivatableState.Active ||
			       a.ActivatableState == ActivatableState.ToActive;
		}

		/// <summary>
		/// Check activatable object for inactive or transit to the inactive state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is inactive or deactivated right now.</returns>
		public static bool IsInactiveOrDeactivated(this IActivatable a)
		{
			return a.ActivatableState == ActivatableState.Inactive ||
			       a.ActivatableState == ActivatableState.ToInactive;
		}
	}
}