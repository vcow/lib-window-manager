using System;
using Base.Activatable;
using UnityEngine;

namespace Base.WindowManager
{
	public abstract class WindowResult
	{
		public IWindow Window { get; }

		protected WindowResult(IWindow window)
		{
			Window = window;
		}
	}

	public interface IWindow : IActivatable
	{
		/// <summary>
		/// Уникальный идентификатор окна.
		/// </summary>
		string WindowId { get; }

		/// <summary>
		/// Группа, к которой принадлежит окно. Такие флаги как Unique и Overlap при вызове окна работают
		/// в рамках группы.
		/// </summary>
		string WindowGroup { get; }

		/// <summary>
		/// Флаг, указывающий, что окно должно открываться эксклюзивно, т. е. окно не откроется, пока есть
		/// другие открытые окна из той же группы, и другие окна из той же группы не будут открыты, пока
		/// открыто эксклюзивное окно.
		/// </summary>
		bool IsUnique { get; }

		/// <summary>
		/// Флаг, указывающий на то, что окно перекрывает нижележащее окно из той же группы, т. е. нижележащее окно
		/// будет деактивировано, и возвращено в исходное состояние (актвировано) после закрытия перекрывающего окна.
		/// </summary>
		bool Overlap { get; }

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
		/// Флаг состояния, указывает на то, что окно закрыто.
		/// </summary>
		bool IsClosed { get; }

		/// <summary>
		/// Поток с результатом работы окна, инициируемый в момент закрытия окна.
		/// </summary>
		IObservable<WindowResult> CloseWindowStream { get; }

		/// <summary>
		/// Поток с результатом работы окна, инициируемый в момент удаления окна из сцены.
		/// </summary>
		IObservable<WindowResult> DestroyWindowStream { get; }
	}
}