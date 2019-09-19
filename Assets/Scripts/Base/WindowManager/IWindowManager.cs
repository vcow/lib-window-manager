namespace Base.WindowManager
{
	public delegate void WindowOpenedHandler(IWindow window);
	public delegate void WindowClosedHandler(IWindowResult result);

	public interface IWindowManager
	{
		IWindow ShowWindow(string windowId, object[] args = null, bool isUnique = false, bool overlap = false);
		int CloseAll(params object[] args);
		IWindow GetWindow(params object[] args);
		IWindow[] GetWindows(params object[] args);
		event WindowOpenedHandler WindowOpenedEvent;
		event WindowClosedHandler WindowClosedEvent;
	}
}