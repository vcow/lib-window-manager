using Base.WindowManager;

namespace Sample.Windows
{
	public class Popup : PopupBase
	{
		public override string WindowId => "popup";

		public override void Activate(bool immediately = false)
		{
		}

		public override void Deactivate(bool immediately = false)
		{
		}

		public override void SetArgs(object[] args)
		{
		}
	}
}