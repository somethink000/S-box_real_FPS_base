using System;
using System.Linq;
using Sandbox;
namespace GeneralGame;
public sealed class ZombieSpawner : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public float RespawnTime { get; set; } = 50f;
	private TimeUntil? TimeUntilRespawn { get; set; }

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

	protected override void OnStart()
	{
		TimeUntilRespawn = 5f;
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Networking.IsHost )
			return;

		if ( !TimeUntilRespawn.HasValue )
		{
			TimeUntilRespawn = RespawnTime;
			return;
		}

		if ( !TimeUntilRespawn.Value )
			return;

		var zombie = ZombiePrefab.Clone( this.Transform.World );
		zombie.NetworkSpawn();

		TimeUntilRespawn = null;
	}

}
