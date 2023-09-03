using UnityEngine;
using UnityEngine.UI;

namespace Base.WindowManager.Template
{
	/// <summary>
	/// Common base class for the popup Window.
	/// </summary>
	/// <typeparam name="T">Returned result type.</typeparam>
	public abstract class PopupWindowBase<T> : Window<PopupWindowBase<T>, T>
	{
#pragma warning disable 649
		[SerializeField] private RectTransform _popup;
#pragma warning restore 649

		/// <summary>
		/// Result returned by Window.
		/// </summary>
		public new T Result
		{
			get => base.Result;
			protected set => base.Result = value;
		}

		/// <summary>
		/// Returns the Window identifier from the child class.
		/// </summary>
		/// <returns>The identifier.</returns>
		protected abstract string GetWindowId();

		/// <summary>
		/// Receiving the arguments, sent from WindowManager during opening.
		/// </summary>
		/// <param name="args">Arguments list.</param>
		protected abstract void DoSetArgs(object[] args);

		/// <summary>
		/// Window activation handler. During activation of the ActivableState property, the state
		/// ActivableState.Active must be assumed, with a possible transition through the intermediate
		/// state ActivableState.ToActive.
		/// </summary>
		/// <param name="immediately">A flag indicating that the transition to the active state occurs instantly,
		/// bypassing the intermediate stage.</param>
		protected abstract void DoActivate(bool immediately);

		/// <summary>
		/// Window deactivation handler. During deactivation, the ActivatableState property must assume the state
		/// ActivatableState.Inactive, with a possible transition through the intermediate state
		/// ActivatableState.ToInactive.
		/// </summary>
		/// <param name="immediately">A flag indicating that the transition to the inactive state occurs instantly,
		/// bypassing the intermediate stage.</param>
		protected abstract void DoDeactivate(bool immediately);

		/// <summary>
		/// A pop-up window located on the base canvas.
		/// </summary>
		protected RectTransform Popup => _popup;

		/// <summary>
		/// The modal window blend for bottom UI.
		/// </summary>
		protected RawImage Blend => GetComponent<RawImage>();

		public override string WindowId => GetWindowId();

		public override void Activate(bool immediately = false)
		{
			DoActivate(immediately);
		}

		public override void Deactivate(bool immediately = false)
		{
			DoDeactivate(immediately);
		}

		public override void SetArgs(object[] args)
		{
			DoSetArgs(args);
		}
	}
}