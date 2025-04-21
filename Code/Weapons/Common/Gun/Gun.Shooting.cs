

using Sandbox;
using Sandbox.Citizen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Sandbox.Citizen.CitizenAnimationHelper;
using static Sandbox.SerializedProperty;

namespace GeneralGame;

public enum FiringType
{
	semi,
	auto,
	burst
}

public partial class Gun
{
	[Property, Group( "Shooting" )] public int Bullets { get; set; } = 1;
	[Property, Group( "Shooting" )] public int ClipSize { get; set; } = 10;
	[Property, Group( "Shooting" )] public float Damage { get; set; } = 5;
	[Property, Group( "Shooting" )] public float Force { get; set; } = 0.1f;
	[Property, Group( "Shooting" )] public float HitFlinch { get; set; } = 1.25f;
	[Property, Group( "Shooting" )] public float Spread { get; set; } = 0.1f;
	[Property, Group( "Shooting" )] public float Recoil { get; set; } = 0.1f;
	[Property, Group( "Shooting" )] public int RPM { get; set; } = 200;
	[Property, Group( "Shooting" )] public int ViewPunch { get; set; } = 5;
	[Property, Group( "Shooting" )] public bool BulletCocking { get; set; } = true; //stay 1 bullet in reciever
	[Property, Group( "Shooting" )] public FiringType FireMod { get; set; } = FiringType.semi;
	[Property, Group( "Shooting" )] public SoundEvent DryShootSound { get; set; }
	[Property, Group( "Shooting" )] public SoundEvent ShootSound { get; set; }
	public IBulletBase BulletType { get; set; } = new HitScanBullet();

	[Sync] public int Clip { get; set; }

	public TimeSince TimeSinceShoot { get; set; }

	public virtual bool CanShoot()
	{

		if ( IsShooting() || IsReloading || InBoltBack || IsHolstering ) return false;
		if ( !Owner.IsValid() || IsRunning || !Owner.IsAlive ) return false;

		if ( !HasAmmo() )
		{
			//if ( shootInfo.DryShootSound is not null )
			//	PlaySound( shootInfo.DryShootSound.ResourceId );

			if ( ShellReloading )
				StartShellReload();
			else
				Reload();

			return false;
		}

		if ( FireMod == FiringType.semi && !Input.Pressed( InputButtonHelper.PrimaryAttack ) ) return false;
		if ( FireMod == FiringType.burst )
		{
			if ( burstCount > 2 ) return false;

			if ( TimeSinceShoot > GetRealRPM( RPM ) )
			{
				burstCount++;
				return true;
			}

			return false;
		};

		if ( RPM <= 0 ) return true;

		return TimeSinceShoot > GetRealRPM( RPM );
	}

	public virtual void Shoot()
	{
		if ( !CanShoot() ) return;

		TimeSinceShoot = 0;

		// Ammo
		Clip -= 1;

		if ( Clip <= 0 ) IsEmpty = true;


		ViewModelRenderer.Set( ShootAnim, true );

		// Sound
		if ( ShootSound is not null )
			PlaySound( ShootSound.ResourceId );

		// Particles
		HandleShootEffects();

		if ( BoltBack && Clip > 0 )
		{
			AsyncBoltBack( GetRealRPM( RPM ) );
		}

		// Barrel smoke
		barrelHeat += 1;

		// Recoil
		Owner.CameraController.EyeAnglesOffset += GetRecoilAngles();


		// Bullet
		for ( int i = 0; i < Bullets; i++ )
		{
			var realSpread = IsScoping ? 0 : GetRealSpread( Spread );
			var spreadOffset = BulletType.GetRandomSpread( realSpread );
			ShootBullet( spreadOffset );
		}

		Owner.CameraController.ApplyFov( 10 );
		Owner.CameraController.ApplyShake( ViewPunch, 1 );
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void ShootBullet( Vector3 spreadOffset )
	{
		BulletType.Shoot( this, spreadOffset );
	}


	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void HandleShootEffects()
	{
		// Player
		Owner.BodyRenderer.Set( "b_attack", true );

	
		// Muzzle flash
		//if ( MuzzleFlashParticle is not null )
		//	CreateParticle( MuzzleFlashParticle, muzzleTransform.Value, ( particles ) => ParticleToMuzzlePos( particles ) );

		//// Barrel smoke
		//if ( !IsProxy && BarrelSmokeParticle is not null && barrelHeat >= ClipSize * 0.75 )
		//	CreateParticle( BarrelSmokeParticle, muzzleTransform.Value, ( particles ) => ParticleToMuzzlePos( particles ) );
	}

	
	// Burst Fire
	public void ResetBurstFireCount( string inputButton )
	{
		if ( FireMod != FiringType.burst ) return;

		if ( Input.Released( inputButton ) )
		{
			burstCount = 0;
		}
	}
	async void AsyncBoltBack( float boltBackDelay )
	{
		InBoltBack = true;
		// Start boltback
		await GameTask.DelaySeconds( boltBackDelay );
		if ( !IsValid ) return;
		if ( !IsProxy )
			ViewModelRenderer?.Set( BoltBackAnim, true );


	}
}
