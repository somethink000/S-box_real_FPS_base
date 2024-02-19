using Sandbox;
using System;
using System.Linq;

namespace Facepunch.Arena;

[Group( "Arena" )]
[Title( "Health Effects" )]
public sealed class HealthEffects : Component
{
	[Property] public ColorAdjustments Adjustments { get; set; }
	
	private PlayerController LocalPlayer { get; set; }
	private Vignette Vignette { get; set; }

	protected override void OnStart()
	{
		Vignette = Components.GetAll<Vignette>().ElementAt( 1 );
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		if ( !LocalPlayer.IsValid() )
		{
			LocalPlayer = Scene.GetAllComponents<PlayerController>()
				.FirstOrDefault( p => p.Network.IsOwner );
		}

		if ( !LocalPlayer.IsValid() )
			return;

		if ( !Vignette.IsValid() )
			return;

		var health = (1f / LocalPlayer.MaxHealth) * LocalPlayer.Health;

		Adjustments.Saturation = 1f - (1f - health) * 0.3f;
		Vignette.Intensity = 0.5f * (1f - health);
		
		base.OnFixedUpdate();
	}
}
