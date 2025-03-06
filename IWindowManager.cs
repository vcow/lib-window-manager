using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Plugins.vcow.WindowManager
{
	/// <summary>
	/// Delegate for Window opened event.
	/// </summary>
	public delegate void WindowOpenedHandler(IWindowManager windowManager, IWindow window);

	/// <summary>
	/// Delegate for Window closed event.
	/// </summary>
	public delegate void WindowClosedHandler(IWindowManager windowManager, string windowId);

	public interface IWindowManager
	{
		/// <summary>
		/// Open Window.
		/// </summary>
		/// <param name="windowId">Identifier of the Window to open.</param>
		/// <param name="args">Arguments passed to the Window when it opens (see IWindow.SetArgs() method).</param>
		/// <param name="isUnique">Flag indicating that the Window should be shown exclusively. If value isn't present,
		/// the value returned by the Window is used (see IWindow.IsUnique flag).</param>
		/// <param name="overlap">Flag indicating that the previous window should be hidden for the duration
		/// of the Window. If value isn't present, the value returned bu the Window is used
		/// (see IWindow.Overlap flag).</param>
		/// <param name="windowGroup">The group in which the window should be opened. If value isn't present,
		/// the value returned by the Window is used (see IWindow.WindowGroup property).</param>
		/// <returns>Returns the reference to the instance of the created Window, or null if the Window
		/// can't be created.</returns>
		IWindow ShowWindow(string windowId, object[] args = null, bool? isUnique = null,
			bool? overlap = null, string windowGroup = null);

		/// <summary>
		/// Close all windows of the specified type.
		/// </summary>
		/// <param name="args">Can be classes of windows to be closed or their identifiers. If nothing,
		/// all open windows will be closed.</param>
		/// <returns>Returns the number of closed windows.</returns>
		int CloseAll(params object[] args);

		/// <summary>
		/// Get the Window of the specified type.
		/// </summary>
		/// <param name="arg">Can be class of the Window or its ID.</param>
		/// <returns>Returns first Window of the specified type or with the specified ID, or null, if not found.</returns>
		IWindow GetWindow(object arg);

		/// <summary>
		/// Get all windows or the specified type.
		/// </summary>
		/// <param name="args">Can be class of the Window or its ID.</param>
		/// <returns>Returns all windows of the specified type or with the specified ID. Empty list, if not found.</returns>
		IReadOnlyList<IWindow> GetWindows(params object[] args);

		/// <summary>
		/// Get the currently open exclusive Window from the specified group. If a group isn't specified, all
		/// currently open exclusive windows from all groups are returned.
		/// </summary>
		/// <param name="groupId">The group identifier.</param>
		/// <returns>Returns a currently open exclusive windows, or an empty list if there are no open
		/// exclusive windows.</returns>
		IReadOnlyList<IWindow> GetCurrentUnique(string groupId = null);

		/// <summary>
		/// Window opened event.
		/// </summary>
		event WindowOpenedHandler WindowOpenedEvent;

		/// <summary>
		/// Window closed with IWindow.Close() method event.
		/// </summary>
		event WindowClosedHandler WindowClosedEvent;

		/// <summary>
		/// Register new Window in the Manager.
		/// </summary>
		/// <param name="windowPrefab">Prefab of the new Window.</param>
		/// <param name="overrideExisting">Flag to override the Window prefab with the same ID, if one is already
		/// registered in the Manager.</param>
		/// <returns>Returns true if new Window was registered successfully.</returns>
		bool RegisterWindow(Window windowPrefab, bool overrideExisting = false);

		/// <summary>
		/// Remove Window's registration from Manager.
		/// </summary>
		/// <param name="windowId">Identifier of the removed Window.</param>
		/// <returns>Returns true if registration was removed successfully.</returns>
		bool UnregisterWindow(string windowId);
	}
}