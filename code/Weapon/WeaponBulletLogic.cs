using Sandbox;
using System.Collections.Generic;
//It doesn't matter why it's here. I just love SOLID ;)
namespace FPSGame.Weapons
{
	public partial class Weapon
	{
		/// <summary>
		/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
		/// hits, like if you're going through layers or ricocheting or something.
		/// </summary>
		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool underWater = Trace.TestPoint( start, "water" );

			var trace = Trace.Ray( start, end )
					.UseHitboxes()
					.WithAnyTags( "solid", "player", "npc" )
					.Ignore( this )
					.Size( radius );

			//
			// If we're not underwater then we can hit water
			//
			if ( !underWater )
				trace = trace.WithAnyTags( "water" );

			var tr = trace.Run();

			if ( tr.Hit )
				yield return tr;
		}
		
		/// <summary>
		/// Shoot a single bullet
		/// </summary>
		public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
		{
			var forward = dir;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				//
				// We turn predictiuon off for this, so any exploding effects don't get culled etc
				//
				using ( Prediction.Off() )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Owner )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		/// <summary>
		/// Shoot a single bullet from owners view point
		/// </summary>
		public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
		{
			Game.SetRandomSeed( Time.Tick );

			var ray = Owner.AimRay;
			ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
		}
	}
}
