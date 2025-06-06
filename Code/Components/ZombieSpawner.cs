﻿using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombieSpawner : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float Delay { get; set; }

	[Property] public bool ActiveState = false;



	public void Activate()
	{
		ActiveState = true;
		
		NewZombie();	
	}

	public void Disable()
	{
		ActiveState = false;
	}

	async void NewZombie()
	{
		if ( !Networking.IsHost )
			return;

		if ( ActiveState )
		{
			if ( !this.IsValid() ) return;

			var zombie = ZombiePrefab.Clone( this.WorldTransform );
			zombie.NetworkSpawn();
			zombie.Components.Get<Zombie>().TargetPrimaryObject = Target;

			await GameTask.DelaySeconds( Delay );

			NewZombie();
		}
		
	}


	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );

		Gizmo.Hitbox.BBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );

		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );
	}



}
