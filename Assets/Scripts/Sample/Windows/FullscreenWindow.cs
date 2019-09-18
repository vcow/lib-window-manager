using Base.WindowManager;
using UnityEngine;

namespace Sample.Windows
{
	public abstract class FullscreenWindow : Window<FullscreenWindow>
	{
#pragma warning disable 649
		[SerializeField] private RectTransform _window;
#pragma warning restore 649

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