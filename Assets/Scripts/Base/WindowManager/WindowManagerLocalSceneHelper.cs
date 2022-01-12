using UnityEngine;
using UnityEngine.Events;

namespace Base.WindowManager
{
	/// <summary>
	/// Вспомогательный компонент, вешается на текущую сцену, если есть отложенные окна, чтобы корректно
	/// удалять их в случае, если сцена закрывается до того, как будут активированы и закрыты отложенные окна.
	/// </summary>
	[DisallowMultipleComponent]
	public class WindowManagerLocalSceneHelper : MonoBehaviour
	{
		public UnityEvent DestroyEvent { get; } = new UnityEvent();

		private void OnDestroy()
		{
			DestroyEvent.Invoke();
			DestroyEvent.RemoveAllListeners();
		}
	}
}