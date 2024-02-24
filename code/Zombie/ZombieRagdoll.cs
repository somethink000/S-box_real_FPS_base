using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;

namespace GeneralGame;

public sealed class ZombieRagdoll : Component
{
	private TimeUntil? TimeUntilDestroy { get; set; } = 10;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	
		if ( TimeUntilDestroy.Value )
		{
			GameObject.Destroy();
		}
	}
}
