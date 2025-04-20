using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombieSpawner : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float Delay { get; set; }

	private bool ActiveState = true;


	protected override void OnStart()
	{
		NewZombie();
	}

	public void Toggle()
	{
		ActiveState = !ActiveState;

		if ( ActiveState ) 
		{
			NewZombie();
		}

	}

	async void NewZombie()
	{
		if ( !Networking.IsHost )
			return;

		await GameTask.DelaySeconds( Delay );

		if ( ActiveState )
		{
			if ( !this.IsValid() ) return;

			var zombie = ZombiePrefab.Clone( this.WorldTransform );
			zombie.NetworkSpawn();
			zombie.Components.Get<Zombie>().TargetPrimaryObject = Target;


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
