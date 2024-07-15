﻿using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame;

public class BaseGun : WeaponComponent, IUse
{
	[Property] public float ReloadTime { get; set; } = 2f;
	[Property] public float EmptyReloadTime { get; set; } = 2f;
	[Property] public float Spread { get; set; } = 0.01f;
	[Property] public Angles Recoil { get; set; }
	[Property] public bool IsAuto { get; set; } = false;
	[Property] public SoundEvent FireSound { get; set; }
	[Property] public SoundEvent EmptyClipSound { get; set; }
	[Property] public SoundSequenceData ReloadSoundSequence { get; set; }
	[Property] public SoundSequenceData EmptyReloadSoundSequence { get; set; }
	[Property] public ParticleSystem MuzzleFlash { get; set; }
	[Property] public ParticleSystem ImpactEffect { get; set; }
	[Property] public AmmoType AmmoType { get; set; } = AmmoType.Pistol;
	[Property] public int DefaultAmmo { get; set; } = 60;
	[Property] public int ClipSize { get; set; } = 30;
	[Property] public Vector3 runPos { get; set; }
	[Property] public Rotation runRotation { get; set; }

	[Sync] public bool IsReloading { get; set; }
	[Sync] public int AmmoInClip { get; set; }

	public Angles curRecoil { get; set; }
	public SoundSequence ReloadSound { get; set; }
	public TimeUntil ReloadFinishTime { get; set; }
	public bool IsFiering { get; set; } = false;

	//AIM {
		[Property] public Vector3 aimPos { get; set; }
		[Property] public Rotation aimRotation { get; set; }
		[Property] public float AimFOVDec { get; set; } = 10f;
		[Sync] public bool IsAiming { get; set; } = false;
	// }

	[Broadcast]
	public virtual void OnUse( Guid pickerId )
	{
		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = picker.Components.GetInDescendantsOrSelf<PlayerObject>();
		if ( !player.IsValid() ) return;

		if ( player.IsProxy )
			return;

		if ( player.Weapons.Has( GameObject ) )
		{
			var ammoToGive = DefaultAmmo - player.Ammo.Get( AmmoType );

			if ( ammoToGive > 0 )
			{
				player.Ammo.Give( AmmoType, ammoToGive );
			}

			GameObject.Destroy();
		}
		else
		{
			player.Weapons.Give( GameObject, false );
			GameObject.Destroy();
		}
	}
	
	protected override void OnDeployed()
	{
		base.OnDeployed();
		EffectRenderer.Set( "b_empty", AmmoInClip == 0 );
	}
	protected override void OnHolstered()
	{
		ReloadSound?.Stop();
		base.OnHolstered();
	}
	
	public override void primaryAction()
	{
		IsFiering = true;
		fireBullet();
	}

	public override void primaryActionRelease()
	{
		IsFiering = false;
	}

	public override void seccondaryAction()
	{
		IsAiming = true;
		owner.setRunSpeed( owner.baseWalkSpeed );
	}
	public override void seccondaryActionRelease()
	{
		owner.setRunSpeed( owner.baseRunSpeed );
		IsAiming = false;
	}

	public override void reloadAction()
	{
		var ammoToTake = ClipSize - AmmoInClip;
		if ( ammoToTake <= 0 )
			return;


		if ( !owner.IsValid() || IsReloading )
			return;

		if ( !owner.Ammo.CanTake( AmmoType, ammoToTake, out var taken ) )
			return;

		EffectRenderer.Set( "b_reload", true );
		ReloadFinishTime = AmmoInClip == 0 ? EmptyReloadTime : ReloadTime;
		IsReloading = true;

		SendReloadMessage();
	}

	//Recoil {
	public void ApplyRecoil( Angles recoil )
	{
		if ( IsProxy ) return;

		curRecoil += recoil;
		/*owner.CameraController.Camera.FieldOfView += 1;*/
		
	}
	private void BuildRecoil()
	{
		var angles = owner.EyeAngles.Normal;
		angles += curRecoil * Time.Delta;

		owner.EyeAngles = angles.WithRoll( 0f );

		curRecoil = curRecoil.LerpTo( Angles.Zero, Time.Delta * 8f );
	}
	// }
	public virtual void fireBullet()
	{
		if ( !NextAttackTime ) return;
		if ( IsReloading ) return;

		if ( AmmoInClip <= 0 )
		{
			SendEmptyClipMessage();
			NextAttackTime = 1f / FireRate;
			return;
		}

		
		if ( owner.IsRunning ) return;

		ApplyRecoil( Recoil );

		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var startPos = owner.Camera.Transform.Position;
		var direction = owner.Camera.Transform.Rotation.Forward;
		direction += Vector3.Random * Spread;

		var endPos = startPos + direction * 10000f;
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.UsePhysicsWorld()
			.UseHitboxes()
			.Run();

		var damage = Damage;
		var origin = attachment?.Position ?? startPos;

		SendAttackMessage( origin, trace.EndPosition, trace.Distance );
		IHealthComponent damageable = null;

		//trace.Surface.DoBulletImpact( trace );

		if ( trace.Component.IsValid() )
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();


		if ( damageable is not null )
		{
			damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id );
		}
		else if ( trace.Hit )
		{
			SendImpactMessage( trace.EndPosition, trace.Normal );
		}

		NextAttackTime = 1f / FireRate;
		AmmoInClip--;


		EffectRenderer.Set( "b_empty", AmmoInClip == 0 );
		EffectRenderer.Set( "b_attack", true );


	}
	

	protected virtual void onReloadEnd()
	{
		var ammoToTake = ClipSize - AmmoInClip;

		owner.Ammo.TryTake( AmmoType, ammoToTake, out var taken );
		AmmoInClip += taken;
		EffectRenderer.Set( "b_empty", false );
		IsReloading = false;
	}
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !IsDeployed && !IsProxy ) return;

		BuildRecoil();

		if ( NextAttackTime && IsFiering && IsAuto ) fireBullet();

		if ( ReloadFinishTime && IsReloading )
		{
			onReloadEnd();
		}

		ReloadSound?.Update( Transform.Position );
	}

	[Broadcast]
	private void SendReloadMessage()
	{
		if ( ReloadSoundSequence is null )
			return;

		ReloadSound?.Stop();

		ReloadSound = new( AmmoInClip == 0 ? EmptyReloadSoundSequence : ReloadSoundSequence );
		ReloadSound.Start( Transform.Position );
	}

	[Broadcast]
	private void SendEmptyClipMessage()
	{
		if ( EmptyClipSound is not null )
		{
			Sound.Play( EmptyClipSound, Transform.Position );
		}
	}

	[Broadcast]
	private void SendImpactMessage( Vector3 position, Vector3 normal )
	{
		if ( ImpactEffect is null ) return;

		var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
		p.SetControlPoint( 0, position );
		p.SetControlPoint( 0, Rotation.LookAt( normal ) );
		p.PlayUntilFinished( Task );
	}

	[Broadcast]
	private void SendAttackMessage( Vector3 startPos, Vector3 endPos, float distance )
	{
		var p = new SceneParticles( Scene.SceneWorld, "particles/tracer/trail_smoke.vpcf" );
		p.SetControlPoint( 0, startPos );
		p.SetControlPoint( 1, endPos );
		p.SetControlPoint( 2, distance );
		p.PlayUntilFinished( Task );

		if ( MuzzleFlash is not null )
		{
			var transform = EffectRenderer.SceneModel.GetAttachment( "muzzle" );

			if ( transform.HasValue )
			{
				p = new( Scene.SceneWorld, MuzzleFlash );
				p.SetControlPoint( 0, transform.Value );
				p.PlayUntilFinished( Task );
			}
		}

		if ( FireSound is not null )
		{
			Sound.Play( FireSound, startPos );
		}
	} 
}
