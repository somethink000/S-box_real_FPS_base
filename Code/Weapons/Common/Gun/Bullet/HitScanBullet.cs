using System;
using System.Linq;
using static Sandbox.Component;

namespace GeneralGame;

public class HitScanBullet : IBulletBase
{
	public void Shoot( Gun weapon, Vector3 spreadOffset )
	{
		var player = weapon.Owner;
		
		var forward = player.CameraController.EyeAngles.Forward + spreadOffset;
		forward = forward.Normal;
		var endPos = player.CameraController.EyePos + forward * 999999;
		var bulletTr = weapon.MakeTrace( player.CameraController.EyePos, endPos );
		var hitObj = bulletTr.GameObject;

		if ( SurfaceUtil.IsSkybox( bulletTr.Surface ) || bulletTr.HitPosition == Vector3.Zero ) return;

		//// Impact
		weapon.CreateBulletImpact( bulletTr );

		// Damage
		if ( hitObj is not null )
		{

			var damage = new DamageInfo( weapon.Damage, weapon.Owner.GameObject, weapon.GameObject, bulletTr.Hitbox );
			damage.Position = bulletTr.HitPosition;
			damage.Shape = bulletTr.Shape;


			if ( bulletTr.GameObject.Components.GetInAncestorsOrSelf<IHealthComponent>() is IHealthComponent damagable )
			{
				damagable.OnDamage( damage );
			}

		}
	}

	public Vector3 GetRandomSpread( float spread )
	{
		return (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
	}
}
