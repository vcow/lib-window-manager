using Base.WindowManager;

namespace Sample.Windows
{
	public class ModalPopup : ModalPopupBase
	{
		public override string WindowId => "modal_popup";

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