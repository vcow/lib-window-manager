using System;

namespace Base.WindowManager.Template
{
	/// <summary>
	/// The enum of the dialog buttons that can be used for configuring dialog windows through the arguments
	/// (see IWindowManager.ShowWindow()) or serializable fields, and as the result type for dialog windows.
	/// </summary>
	[Serializable, Flags]
	public enum DialogButtonType
	{
		None = 0x00,
		Ok = 0x01,
		Cancel = 0x02,
		Yes = 0x04,
		No = 0x08,
		OkCancel = Ok | Cancel,
		YesNo = Yes | No,
		YesNoCancel = Yes | No | Cancel
	}
}