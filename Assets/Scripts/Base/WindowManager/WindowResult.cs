using System;

namespace Base.WindowManager
{
	[Flags]
	public enum DialogButtonType
	{
		None = 0x00,
		Ok = 0x01,
		Cancel = 0x02,
		Yes = 0x04,
		No = 0x08
	}

	public class WindowResult<T> : WindowResult
	{
		public T Value { get; }

		public WindowResult(IWindow window, T value) : base(window)
		{
			Value = value;
		}
	}

	public class WindowResult<T1, T2> : WindowResult
	{
		public T1 Value1 { get; }
		public T2 Value2 { get; }

		public WindowResult(IWindow window, T1 value1, T2 value2) : base(window)
		{
			Value1 = value1;
			Value2 = value2;
		}
	}

	public class WindowResult<T1, T2, T3> : WindowResult
	{
		public T1 Value1 { get; }
		public T2 Value2 { get; }
		public T3 Value3 { get; }

		public WindowResult(IWindow window, T1 value1, T2 value2, T3 value3) : base(window)
		{
			Value1 = value1;
			Value2 = value2;
			Value3 = value3;
		}
	}
}