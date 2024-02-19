using Sandbox;
using Sandbox.Network;

namespace Facepunch.Arena;

[Group( "Arena" )]
[Title( "Pickup Spawner" )]
[Icon( "paid" )]
public class PickupSpawner : Component
{
	[Property] public GameObject PickupPrefab { get; set; }
	[Property] public float RespawnTime { get; set; } = 30f;
	
	private TimeUntil? TimeUntilRespawn { get; set; }
	private PickupComponent Pickup { get; set; }

	protected override void DrawGizmos()
	{
		const float boxSize = 4f;
		var bounds = new BBox( Vector3.One * -boxSize, Vector3.One * boxSize );
		
		Gizmo.Hitbox.BBox( bounds );
		
		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.5f : 0.2f );
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.SolidBox( bounds );
		
		Gizmo.Draw.Color = Color.Cyan.WithAlpha( (Gizmo.IsHovered || Gizmo.IsSelected) ? 0.8f : 0.6f );
		Gizmo.Draw.Text( PickupPrefab?.Name ?? "Pickup", global::Transform.Zero.WithPosition( Vector3.Up * (boxSize + 4f) ), "Roboto", 14f );
	}

	protected override void OnStart()
	{
		TimeUntilRespawn = 0f;
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		
		if ( !Networking.IsHost )
			return;
		
		if ( Pickup.IsValid() )
			return;

		if ( !TimeUntilRespawn.HasValue )
		{
			TimeUntilRespawn = RespawnTime;
			return;
		}

		if ( !TimeUntilRespawn.Value )
			return;

		var go = PickupPrefab.Clone();
			
		Pickup = go.Components.Get<PickupComponent>();
			
		go.Transform.Position = Transform.Position;
		go.Transform.Rotation = Transform.Rotation;
		go.NetworkSpawn();

		TimeUntilRespawn = null;
	}
}
