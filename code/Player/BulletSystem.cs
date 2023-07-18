using Sandbox;
using System;
using System.Linq;

namespace MyGame;
public struct BulletInfo
{
	public BulletInfo()
	{

	}
	public Entity Owner;
	public Entity Weapon;
	public Vector3 Position;
	public Vector3 Direction;
	public float Damage;
	public float Spread;
	public float Force;
	public float Count;
	public float HeadshotMultiplier = 2;

	public Action<TraceResult> OnBulletHit;
	public Action<DamageInfo> OnDealDamage;
	public Entity IgnoreEntity;
	public Vector3? TracerPosition;
	public string TracerOverride;
}
public partial class Bullet
{
	public static void ShootBullet( BulletInfo info, int level = 0 )
	{
		for ( int i = 0; i < info.Count; i++ )
		{
			Game.SetRandomSeed( Time.Tick + i );
			var position = info.Position;
			var forward = info.Direction;

			forward += Vector3.Random * info.Spread;

			// check if we started in something penetrable
			var trpcheck = Trace.Ray( position, position + (forward * 1) ).WithAnyTags( "penetrable", "water" ).Run();
			if ( trpcheck.StartedSolid )
			{
				info.IgnoreEntity = trpcheck.Entity;
			}
			var tr = Trace.Ray( position, position + (forward * 10000) )
				.UseHitboxes()
				.Ignore( info.Owner )
				.Ignore( info.IgnoreEntity )
				.WithAnyTags( "solid", "player", "npc", "penetrable", "corpse", "glass", "water", "carriable", "debris" )
				.WithoutTags( "trigger", "skybox", "playerclip" )
				.Run();

			var tracerstart = info.TracerPosition ?? tr.StartPosition;
			CreateTracerEffect( info, tracerstart, tr.EndPosition );
			DoBulletFlyby( info.Owner, tr.StartPosition, tr.EndPosition, tr.Direction );
			if ( tr.Hit )
			{
				tr.Surface.DoBulletImpact( tr );
				var damage = info.Damage;

				if ( tr.Hitbox.HasTag( "head" ) )
				{
					damage *= info.HeadshotMultiplier;
				}
				
				var dmgInfo = DamageInfo.FromBullet( tr.HitPosition, forward * info.Force, damage )
					.WithWeapon( info.Weapon )
					.WithBone( tr.Bone )
					.WithAttacker( info.Owner )
					.WithTag( "bullet" );

				tr.Entity.TakeDamage( dmgInfo );

				if ( info.OnDealDamage != null ) info.OnDealDamage( dmgInfo );
				if ( info.OnBulletHit != null ) info.OnBulletHit( tr );


				if ( ((tr.Entity.Tags.Has( "penetrable" ) || tr.Entity.Tags.Has( "water" )) || tr.Entity is ShatterGlass || tr.Entity is GlassShard) && level < 16 )
				{
					var newinfo = info;
					newinfo.IgnoreEntity = tr.Entity;
					newinfo.Position = tr.HitPosition;
					newinfo.Direction = forward;
					newinfo.Spread = 0;
					ShootBullet( newinfo, level + 1 );
				}
			}
		}
	}
	public static void CreateTracerEffect( BulletInfo info, Vector3 StartPosition, Vector3 EndPosition )
	{
		var system = Particles.Create( info.TracerOverride ?? "particles/tracers/tracer.generic.vpcf" );
		system?.SetPosition( 0, StartPosition );
		system?.SetPosition( 1, EndPosition );
	}

	[ClientRpc]
	public static void DoBulletFlyby( Entity Owner, Vector3 StartPosition, Vector3 EndPosition, Vector3 Direction )
	{
		if ( Owner != null && Owner.Client != null && Owner.Client == Game.LocalClient ) return;

		var posforsound = StartPosition + Direction * Vector3.Dot( Camera.Position - StartPosition, Direction );
		posforsound = posforsound.Clamp( Vector3.Min( StartPosition + (Direction * 70), EndPosition ), Vector3.Max( StartPosition + (Direction * 70), EndPosition ) );

		Sound.FromWorld( "flyby.pistol", posforsound );
	}
}
