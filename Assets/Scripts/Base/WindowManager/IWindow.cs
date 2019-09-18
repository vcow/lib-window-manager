using Base.Activatable;
using UnityEngine;

namespace Base.WindowManager
{
	public delegate void WindowResultHandler(IWindowResult result);

	public interface IWindow : IActivatable
	{
		/// <summary>
		/// Уникальный идентификатор окна.
		/// </summary>
		string WindowId { get; }

		/// <summary>
		/// Канва окна.
		/// </summary>
		Canvas Canvas { get; }

		/// <summary>
		/// Метод закрытия окна.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий закрывать окно мгновенно, без эффектов.</param>
		/// <returns>Возвращает <code>true</code>, если окно может быть закрыто.</returns>
		bool Close(bool immediately = false);

		/// <summary>
		/// В этот метод передаются аргументы, полученные при вызове метода ShowWindow() WindowManager-а.
		/// </summary>
		/// <param name="args">Список аргументов.</param>
		void SetArgs(object[] args);

		/// <summary>
		/// Событие, инициируемое в момент закрытия окна.
		/// </summary>
		event WindowResultHandler CloseWindowEvent;

		/// <summary>
		/// Событие, инициируемое в момент удаления окна из сцены.
		/// </summary>
		event WindowResultHandler DestroyWindowEvent;
	}
}