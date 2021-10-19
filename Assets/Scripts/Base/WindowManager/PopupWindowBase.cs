using UnityEngine;
using UnityEngine.UI;

namespace Base.WindowManager
{
	/// <summary>
	/// Пустой результат для окон, не возвращающих результата.
	/// </summary>
	public class EmptyWindowResult
	{
		private static EmptyWindowResult _instance;
		public static EmptyWindowResult Instance => _instance ??= new EmptyWindowResult();

		private EmptyWindowResult()
		{
		}
	}

	/// <summary>
	/// Унифицированный базовый класс для всплывающх окон.
	/// </summary>
	/// <typeparam name="T">Тип возвращаемого результата.</typeparam>
	public abstract class PopupWindowBase<T> : Window<PopupWindowBase<T>>
	{
#pragma warning disable 649
		[SerializeField] private RectTransform _popup;
#pragma warning restore 649

		/// <summary>
		/// Результат, возвращаемый окном.
		/// </summary>
		public new T Result
		{
			get => base.Result is WindowResult<T> result ? result.Value : default;
			protected set => base.Result = new WindowResult<T>(this, value);
		}

		/// <summary>
		/// Возвращает идентификатор дочернего окна.
		/// </summary>
		/// <returns>Идентификатор дочернего окна.</returns>
		protected abstract string GetWindowId();

		/// <summary>
		/// Принимает аргументы, переданные окну при открытии.
		/// </summary>
		/// <param name="args">Список переданных аргументов.</param>
		protected abstract void DoSetArgs(object[] args);

		/// <summary>
		/// Обработчик активации окна. В процессе активации свойство ActivatableState должно принять состояние
		/// ActivatableState.Active с возможным переходом через промежуточное состояние ActivatableState.ToActive.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий на то, что переход в активное состояние происходит
		/// мгновенно, минуя промежуточную стадию.</param>
		protected abstract void DoActivate(bool immediately);

		/// <summary>
		/// Обработчик деактивации окна. В процессе деактивации свойство ActivatableState должно принять состояние
		/// ActivatableState.Inactive с возможным переходом через промежуточное состояние ActivatableState.ToInactive.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий на то, что переход в неактивное состояние происходит
		/// мгновенно, минуя промежуточную стадию.</param>
		protected abstract void DoDeactivate(bool immediately);

		/// <summary>
		/// Всплывающее окно, расположенное на канве.
		/// </summary>
		protected RectTransform Popup => _popup;

		/// <summary>
		/// Бленда модального окна.
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