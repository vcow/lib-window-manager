namespace Base.WindowManager
{
	public delegate void WindowOpenedHandler(IWindow window);

	public delegate void WindowClosedHandler(IWindowResult result);

	public interface IWindowManager
	{
		/// <summary>
		/// Открыть окно.
		/// </summary>
		/// <param name="windowId">Идентификатор открываемого окна.</param>
		/// <param name="args">Аргументы, передаваемые окну при открытии.</param>
		/// <param name="isUnique">Флаг, указывающий на то, что окно должно быть показано эксклюзивно.</param>
		/// <param name="overlap">Флаг, указывающий на то, что на время показа окна,
		/// предыдущее окно должно быть скрыто.</param>
		/// <returns>Возвращает ссылку на экземпляр созданного окна, или <code>null</code>,
		/// если создание окна невозможно.</returns>
		IWindow ShowWindow(string windowId, object[] args = null, bool isUnique = false, bool overlap = false);

		/// <summary>
		/// Закрыть все окна указанного типа.
		/// </summary>
		/// <param name="args">В качестве аргументов могут выступать классы закрываемых окон
		/// или их идентификаторы.</param>
		/// <returns>Возвращает количество закрытых окон.</returns>
		int CloseAll(params object[] args);

		/// <summary>
		/// Получить окно указанного типа.
		/// </summary>
		/// <param name="arg">В качестве аргумента может выступать класс окна, или его идентификатор.</param>
		/// <returns>Возвращает первое окно указанного типа или с указанным идентификатором.</returns>
		IWindow GetWindow(object arg);

		/// <summary>
		/// Получить все окна указанного типа.
		/// </summary>
		/// <param name="args">В качестве аргументов могут выступать классы окон или их идентификаторы.</param>
		/// <returns></returns>
		IWindow[] GetWindows(params object[] args);

		/// <summary>
		/// Событие, возникающее при открытии нового окна.
		/// </summary>
		event WindowOpenedHandler WindowOpenedEvent;

		/// <summary>
		/// Событие, возникающее при закрытии окна методом IWindow.Close().
		/// </summary>
		event WindowClosedHandler WindowClosedEvent;

		/// <summary>
		/// Зарегистрировать новое окно в Менеджере.
		/// </summary>
		/// <param name="windowPrefab">Префаб нового окна.</param>
		/// <param name="overrideExisting">Флаг, указывающий перезаписать префаб окна с таким же идентификатором,
		/// если таковой уже зарегистрирован в Менеджере.</param>
		/// <returns>Возвращает <code>true</code>, если новое окно успешно зарегистрировано.</returns>
		bool RegisterWindow(Window windowPrefab, bool overrideExisting = false);

		/// <summary>
		/// Удалить регистрацию окна в Менеджере.
		/// </summary>
		/// <param name="windowId">Идентификатор удаляемого окна.</param>
		/// <returns>Возвращает <code>true</code>, если регистрация успешно удалена.</returns>
		bool UnregisterWindow(string windowId);
	}
}