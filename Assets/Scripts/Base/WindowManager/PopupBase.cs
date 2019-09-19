using Helpers.TouchHelper;
using UnityEngine;
using UnityEngine.UI;

namespace Base.WindowManager
{
	public abstract class PopupBase : Window<PopupBase>
	{
		private CanvasGroup _popupCanvasGroup;

#pragma warning disable 649
		[SerializeField] private RectTransform _popup;
#pragma warning restore 649

		protected RectTransform Popup => _popup;

		protected CanvasGroup PopupCanvasGroup
		{
			get
			{
				if (!_popupCanvasGroup)
				{
					_popupCanvasGroup = Popup.GetComponent<CanvasGroup>();
					if (!_popupCanvasGroup) _popupCanvasGroup = Popup.gameObject.AddComponent<CanvasGroup>();
				}

				return _popupCanvasGroup;
			}
		}
	}

	[RequireComponent(typeof(RawImage))]
	public abstract class ModalPopupBase : PopupBase
	{
		private RawImage _rawImage;
		private int _lockId;

		public RawImage RawImage => _rawImage ? _rawImage : _rawImage = GetComponent<RawImage>();

		protected virtual void Start()
		{
			_lockId = TouchHelper.Lock();
		}

		protected override void OnDestroy()
		{
			TouchHelper.Unlock(_lockId);
			base.OnDestroy();
		}
	}
}