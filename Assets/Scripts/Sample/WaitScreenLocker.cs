using Base.WindowManager.ScreenLockerExtension;
using UnityEngine;

namespace Sample
{
	public class WaitScreenLocker : CommonScreenLockerBase
	{
		public override LockerType LockerType => LockerType.BusyWait;

		~WaitScreenLocker()
		{
			Debug.Log("WaitScreenLocker destroyed!");
		}
	}
}