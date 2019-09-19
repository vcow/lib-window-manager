namespace Base.WindowManager
{
	public interface IWindowManager
	{
		IWindow ShowWindow(string windowId, object[] args = null, bool isUnique = false, bool overlap = false);
	}
}