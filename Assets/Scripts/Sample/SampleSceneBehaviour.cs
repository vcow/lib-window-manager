using UnityEngine;
using Zenject;

namespace Sample
{
	public class SampleSceneBehaviour : MonoInstaller<SampleSceneBehaviour>
	{
#pragma warning disable 649
		[SerializeField] private DashboardController _dashboard;
#pragma warning restore 649

		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			Container.Inject(_dashboard);
		}
	}
}