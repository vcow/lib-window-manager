using Base.WindowManager.ScreenLockerExtension;
using UnityEngine;

namespace Sample
{
	public class SceneScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.SceneLoader;

		~SceneScreenLocker()
		{
			Debug.Log("SceneScreenLocker destroyed!");
		}
	}
}