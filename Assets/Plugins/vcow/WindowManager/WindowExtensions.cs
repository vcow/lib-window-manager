// ReSharper disable once CheckNamespace
namespace Plugins.vcow.WindowManager
{
	/// <summary>
	/// Activatable extensions.
	/// </summary>
	public static class WindowExtensions
	{
		/// <summary>
		/// Check activatable object activity.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is active.</returns>
		public static bool IsActive(this IWindow a)
		{
			return a.State == WindowState.Active;
		}

		/// <summary>
		/// Check activatable object inactivity.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is inactive.</returns>
		public static bool IsInactive(this IWindow a)
		{
			return a.State == WindowState.Inactive;
		}

		/// <summary>
		/// Check activatable object for a transition state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is activated or deactivated right now.</returns>
		public static bool IsBusy(this IWindow a)
		{
			return a.State == WindowState.ToActive ||
			       a.State == WindowState.ToInactive;
		}

		/// <summary>
		/// Check activatable object for active or transit to the active state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is active or activated right now.</returns>
		public static bool IsActiveOrActivated(this IWindow a)
		{
			return a.State == WindowState.Active ||
			       a.State == WindowState.ToActive;
		}

		/// <summary>
		/// Check activatable object for inactive or transit to the inactive state.
		/// </summary>
		/// <param name="a">Checked object.</param>
		/// <returns>Returns true, if object is inactive or deactivated right now.</returns>
		public static bool IsInactiveOrDeactivated(this IWindow a)
		{
			return a.State == WindowState.Inactive ||
			       a.State == WindowState.ToInactive;
		}
	}
}