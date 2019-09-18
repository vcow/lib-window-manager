namespace Base.WindowManager
{
	public class CommonWindowResult : IWindowResult
	{
		public CommonWindowResult(IWindow window)
		{
			Window = window;
		}

		public IWindow Window { get; }
	}
}