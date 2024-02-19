using System;
using System.Linq;
using Sandbox;
namespace Facepunch.Arena;
public sealed class ZombieSpawner : Component
{

	[Property] public GameObject ZombiePrefab { get; set; }
	public float GetRandom() => Random.Shared.Float(1, 100);
	protected override void OnUpdate()
	{
		
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

	void SpawnZombie()
	{
		
		var zombie = ZombiePrefab.Clone( this.Transform.World );

	}

	TimeUntil nextSecond = 5f;
	protected override void OnFixedUpdate()
	{
		if (nextSecond)
		{
			var random = GetRandom();
			GetRandom();

			if (random >= 60f)
			{
				SpawnZombie();
			}
			nextSecond = 5;
			//Log.Info(random);
		}
		
		
	}

}
