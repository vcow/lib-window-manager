using Base.WindowManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public class ModalPopup : ModalPopupBase
	{
#pragma warning disable 649
		[SerializeField] private Button _closeButton;
		[SerializeField] private Text _ctrLabel;
#pragma warning restore 649

		[Inject]
		// ReSharper disable once UnusedMember.Local
		private void Construct(DiContainer container)
		{
			container.InjectGameObject(Popup.gameObject);
		}

		public override string WindowId => "modal_popup";

		public override void Activate(bool immediately = false)
		{
		}

		public override void Deactivate(bool immediately = false)
		{
		}

		public override void SetArgs(object[] args)
		{
			foreach (var arg in args)
			{
				switch (arg)
				{
					case int intVal:
						_ctrLabel.text = $"#{intVal}";
						break;
				}
			}
		}

		private void Start()
		{
			_closeButton.onClick.AddListener(() => Close());
		}

		protected override void OnDestroy()
		{
			_closeButton.onClick.RemoveAllListeners();
			base.OnDestroy();
		}
	}
}