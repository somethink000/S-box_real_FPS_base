using Sandbox;
using System;
using System.Diagnostics;

namespace GeneralGame;


public class BaseMele : WeaponComponent, IUse
{
	[Property] public float SeccondaryDamage { get; set; } = 50f;
	[Property] public float SeccondaryFireRate { get; set; } = 1f;
	[Property] public ParticleSystem ImpactEffect { get; set; }
	[Property] public SoundEvent AttackSound { get; set; }
	[Property] public SoundEvent HitSound { get; set; }
	[Property] public SoundEvent HitWorldSound { get; set; }
	[Property] public SoundEvent KillSound { get; set; }
	[Property] public float Range { get; set; }
	[Property] public float Punch { get; set; }


	[Broadcast]
	public virtual void OnUse( Guid pickerId )
	{
		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = picker.Components.GetInDescendantsOrSelf<PlayerObject>();
		if ( !player.IsValid() ) return;

		if ( player.IsProxy )
			return;

		if ( !player.Weapons.Has( GameObject ) )
		{
			player.Weapons.Give( GameObject, false );
			GameObject.Destroy();
		}
	}

	public override void primaryAction()
	{
		attackTrace( true );
	}
	public override void seccondaryAction()
	{
		attackTrace( false );
	}


	public virtual void attackTrace( bool primary )
	{
		if ( !NextAttackTime ) return;

		

		var startPos = owner.Camera.Transform.Position;
		var direction = owner.Camera.Transform.Rotation.Forward;

		var endPos = startPos + direction * Range;
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.UsePhysicsWorld()
			.UseHitboxes()
			.Run();

		var damage = Damage;

		

		Sound.Play( AttackSound, startPos );

		IHealthComponent damageable = null;

		if ( trace.Component.IsValid() )
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
		

		if ( damageable is not null )
		{
			if (damageable.Health - damage < 0)
			{
				//trace.Component.Velocity = direction * 500;
				Sound.Play( KillSound, startPos );
			}
			else
			{
				Sound.Play( HitSound, startPos );
			}
			damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id );

			
		}
		else if ( trace.Hit )
		{
			if ( ImpactEffect is null ) return;

			var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
			p.SetControlPoint( 0, trace.EndPosition );
			p.SetControlPoint( 0, Rotation.LookAt( trace.Normal ) );
			p.PlayUntilFinished( Task );

			Sound.Play( HitWorldSound, startPos );
		}

		NextAttackTime = 1f / (primary ? FireRate : SeccondaryFireRate);
		

		EffectRenderer.Set( (primary ? "b_attack" : "b_power_attack"), true );

		
	}
}
